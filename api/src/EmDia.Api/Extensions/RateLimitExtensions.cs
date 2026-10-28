using System.Threading.RateLimiting;
using EmDia.Application;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Extensions;

/// <summary>Limite de 5 tentativas de login por minuto por IP; excedeu → 429 (spec §4.5).</summary>
public static class RateLimitExtensions
{
    public const string PoliticaLogin = "login";

    public static IServiceCollection AddRateLimitDeLogin(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.AddPolicy(PoliticaLogin, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = ctx.RequestServices
                            .GetRequiredService<Microsoft.Extensions.Options.IOptions<SegurancaOptions>>()
                            .Value.LoginTentativasPorMinuto,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.Headers.RetryAfter = "60";
                await ctx.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Muitas tentativas",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Você tentou entrar muitas vezes. Aguarde um minuto e tente de novo.",
                    Extensions = { ["code"] = "MUITAS_TENTATIVAS" }
                }, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", ct);
            };
        });

        return services;
    }
}
