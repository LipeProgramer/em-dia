using System.Text;
using System.Text.Json.Serialization;
using EmDia.Api.Middlewares;
using EmDia.Application;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Application.Services;
using EmDia.Application.Validators;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Jobs;
using EmDia.Infrastructure.Repositories;
using EmDia.Infrastructure.Security;
using EmDia.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace EmDia.Api.Extensions;

/// <summary>Configuração de serviços da API: dados, camadas, JWT, CORS e Swagger (spec §1.2).</summary>
public static class ServiceCollectionExtensions
{
    public const string PoliticaCors = "frontend";
    private const string CampoObrigatorio = "Campo obrigatório.";

    public static IServiceCollection AddEmDia(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Secao));
        services.Configure<SegurancaOptions>(config.GetSection(SegurancaOptions.Secao));

        services.AddDbContext<EmDiaDbContext>((sp, o) => o.UseNpgsql(
            sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")
                ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada."),
            npg => npg.MigrationsAssembly("EmDia.Infrastructure")));

        // Repositórios (interfaces na Application, implementação na Infrastructure)
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<ICheckupRepository, CheckupRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IHistoricoRepository, HistoricoRepository>();
        services.AddScoped<IAvisoRepository, AvisoRepository>();
        services.AddScoped<IInscricaoPushRepository, InscricaoPushRepository>();

        // Services (casos de uso)
        services.AddScoped<AuthService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<CatalogoService>();
        services.AddScoped<CheckupService>();
        services.AddScoped<RadarService>();
        services.AddScoped<ItemService>();
        services.AddScoped<VeiculoService>();
        services.AddScoped<HistoricoService>();
        services.AddScoped<NotificacoesService>();
        services.AddValidatorsFromAssemblyContaining<RegistrarRequestValidator>(ServiceLifetime.Scoped);

        // Segurança, relógio e e-mail
        services.AddSingleton<IRelogio, RelogioSaoPaulo>();
        services.AddSingleton<EmDia.Domain.Interfaces.IRelogio>(sp => sp.GetRequiredService<IRelogio>());
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        if (string.IsNullOrWhiteSpace(config["Email:Smtp:Host"]))
            services.AddSingleton<IEmailService, EmailServiceFake>(); // sem SMTP configurado, só registra em log
        else
            services.AddScoped<IEmailService, EmailService>();

        // Notificações no celular (Web Push · DV20): chaves VAPID derivadas da chave do JWT
        services.AddSingleton<ChavesVapid>();
        services.AddScoped<IPushService, PushService>();

        // Rotina diária de avisos (BackgroundService)
        services.AddScoped<RotinaAvisosService>();
        services.AddSingleton<AvisosWorker>();
        if (!env.IsEnvironment("Testing"))
            services.AddHostedService(sp => sp.GetRequiredService<AvisosWorker>());

        return services;
    }

    /// <summary>JWT Bearer (HS256, 30 s de tolerância) e política padrão: toda rota exige usuário autenticado (spec §4.2 e §4.4).</summary>
    public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((o, opcoes) =>
            {
                var jwt = opcoes.Value;
                if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
                    throw new InvalidOperationException(
                        "Jwt:Key ausente ou curta (mínimo 256 bits). Configure com: dotnet user-secrets set \"Jwt:Key\" \"...\"");

                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name"
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Title = "Não autorizado",
                            Status = StatusCodes.Status401Unauthorized,
                            Detail = "Sua sessão expirou ou você não entrou. Entre de novo.",
                            Extensions = { ["code"] = "NAO_AUTENTICADO" }
                        }, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    /// <summary>CORS: a API aceita apenas a origem do frontend configurada no ambiente (spec §4.5).</summary>
    public static IServiceCollection AddCorsFrontend(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
            .Configure<IConfiguration>((o, config) => o.AddPolicy(PoliticaCors, p => p
                .WithOrigins(config.GetSection("Cors:Origens").Get<string[]>() ?? [])
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                .WithHeaders("Authorization", "Content-Type", "X-Cliente", "X-Refresh-Token")
                .AllowCredentials()));
        return services;
    }

    /// <summary>Controllers com JSON em camelCase, enums como texto e erros de modelo no formato ProblemDetails.</summary>
    public static IServiceCollection AddControllersEmDia(this IServiceCollection services)
    {
        services.AddControllers(o =>
                o.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(_ => CampoObrigatorio))
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)))
            .ConfigureApiBehaviorOptions(o =>
                o.InvalidModelStateResponseFactory = ctx =>
                {
                    var erros = ctx.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            e => ErrorHandlingMiddleware.CamelCase(e.Key.TrimStart('$', '.')) is { Length: > 0 } k ? k : "corpo",
                            e => e.Value!.Errors
                                .Select(x => x.ErrorMessage == CampoObrigatorio ? CampoObrigatorio : "Valor inválido.")
                                .Distinct().ToArray());
                    return new BadRequestObjectResult(ErrorHandlingMiddleware.DadosInvalidos(erros))
                    {
                        ContentTypes = { "application/problem+json" }
                    };
                });
        return services;
    }

    public static IServiceCollection AddSwaggerEmDia(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Em Dia API",
                Version = "v1",
                Description = "Radar de vencimentos para adultos com TDAH — Entrega 2 (spec §3)"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Cole apenas o token de acesso (sem 'Bearer ').",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            });
            var xml = Path.Combine(AppContext.BaseDirectory, "EmDia.Api.xml");
            if (File.Exists(xml)) c.IncludeXmlComments(xml);

            // Exemplos válidos nos corpos e lista de opções nos filtros: testar pelo Swagger sem erro de digitação
            c.SchemaFilter<ExemplosSwagger>();
            c.OperationFilter<OpcoesDeFiltroSwagger>();
        });
        return services;
    }
}
