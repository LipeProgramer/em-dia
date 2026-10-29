using EmDia.Api.Extensions;
using EmDia.Api.Middlewares;
using EmDia.Infrastructure.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

// Em Dia — API ASP.NET Core (spec §1). Camadas: Api → Application → Domain; Infrastructure implementa as interfaces.

var builder = WebApplication.CreateBuilder(args);

// Limite de tamanho do corpo da requisição (spec §4.5: entrada maliciosa ou enorme)
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 64 * 1024);

builder.Services
    .AddEmDia(builder.Configuration, builder.Environment)
    .AddAutenticacaoJwt()
    .AddRateLimitDeLogin()
    .AddCorsFrontend()
    .AddControllersEmDia()
    .AddSwaggerEmDia();

if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(365));

// Atrás de proxy (Vercel → Render): IP real do cliente para o rate limit e HTTPS vindo do proxy
var atrasDeProxy = builder.Configuration.GetValue<bool>("Proxy:Confiar");
if (atrasDeProxy)
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.ForwardLimit = null;       // a cadeia passa por mais de um proxy
        o.KnownIPNetworks.Clear();   // os IPs dos proxies do Render e do Vercel não são fixos
        o.KnownProxies.Clear();
    });

var app = builder.Build();

if (atrasDeProxy)
    app.UseForwardedHeaders();

// No servidor, cria/atualiza as tabelas e os dados iniciais ao subir
if (app.Configuration.GetValue<bool>("Database:MigrarAoIniciar"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<EmDiaDbContext>().Database.Migrate();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHsts();               // HTTPS obrigatório com HSTS (spec §4.5)
    app.UseHttpsRedirection();
}

// Cabeçalhos de segurança (CSP, sem sniffing, sem embutir em frames)
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h.XContentTypeOptions = "nosniff";
    h.XFrameOptions = "DENY";
    h["Referrer-Policy"] = "no-referrer";
    if (!ctx.Request.Path.StartsWithSegments("/swagger"))
        h.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
    await next();
});

// Documentação OpenAPI interativa em /swagger (referência para o front — spec §1.5)
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Em Dia API v1"));

app.UseCors(ServiceCollectionExtensions.PoliticaCors);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();

app.Run();

public partial class Program;
