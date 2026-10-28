using EmDia.Api.Extensions;
using EmDia.Api.Middlewares;

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

var app = builder.Build();

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
