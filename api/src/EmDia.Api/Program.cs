using System.Text;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Application.Services;
using EmDia.Application.Validators;
using EmDia.Api.Middlewares;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Repositories;
using EmDia.Infrastructure.Security;
using EmDia.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Carregar appsettings por ambiente (inclui Testing para testes de integração)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// ── Banco ────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<EmDiaDbContext>(opts =>
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        o => o.MigrationsAssembly("EmDia.Infrastructure")));

// ── Repositórios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IModeloItemRepository, ModeloItemRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ICheckupRepository, CheckupRepository>();
builder.Services.AddScoped<IHistoricoRepository, HistoricoRepository>();
builder.Services.AddScoped<IAvisoRepository, AvisoRepository>();

// ── Serviços de aplicação ────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
// IRelogio da Application (herda de Domain.Interfaces.IRelogio)
builder.Services.AddScoped<IRelogio, RelogioSaoPaulo>();
// Registrar também como Domain.Interfaces.IRelogio para serviços que o injetam diretamente
builder.Services.AddScoped<EmDia.Domain.Interfaces.IRelogio>(sp =>
    sp.GetRequiredService<IRelogio>());

// ── E-mail ───────────────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<IEmailService, EmailServiceFake>();
else
    builder.Services.AddScoped<IEmailService, EmailService>();

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrarRequestValidator>();

// ── JWT Bearer ───────────────────────────────────────────────────────────────
var chaveJwt = builder.Configuration["Jwt:Chave"]
    ?? throw new InvalidOperationException("Jwt:Chave não configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        // MapInboundClaims = false: mantém "sub" como "sub" (sem remapear para NameIdentifier)
        opts.MapInboundClaims = false;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Emissor"] ?? "emdia-api",
            ValidAudience            = builder.Configuration["Jwt:Audiencia"] ?? "emdia-web",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt)),
            ClockSkew                = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
// Em testes de integração os limites são altíssimos para não interferir
var isTesting = builder.Environment.EnvironmentName == "Testing";

builder.Services.AddRateLimiter(opts =>
{
    opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = isTesting ? 1_000_000 : 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    opts.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = isTesting ? 1_000_000 : 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueLimit = 0;
    });

    opts.RejectionStatusCode = 429;
});

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p =>
        p.WithOrigins(
            builder.Configuration["Cors:Origens"]?.Split(',') ?? ["http://localhost:5173"])
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials()));

// ── Controllers + Swagger ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Em Dia API",
        Version = "v1",
        Description = "Radar de vencimentos — UNICV Engenharia de Software"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Em Dia API v1"));
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
