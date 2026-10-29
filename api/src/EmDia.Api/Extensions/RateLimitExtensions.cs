using System.Threading.RateLimiting;
using EmDia.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace EmDia.Api.Extensions;

/// <summary>
/// Limites de requisição por IP; excedeu → 429.
/// Login: 5 tentativas por minuto (spec §4.5). Geral (DV21): 300 requisições por minuto em toda a API, para uma
/// enxurrada de chamadas não derrubar o servidor nem esgotar a cota gratuita.
/// </summary>
public static class RateLimitExtensions
{
    public const string PoliticaLogin = "login";

    public static IServiceCollection AddLimitesDeRequisicao(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.AddPolicy(PoliticaLogin, ctx =>
                JanelaPorIp(ctx, s => s.LoginTentativasPorMinuto));

            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                JanelaPorIp(ctx, s => s.RequisicoesPorMinuto));

            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = async (ctx, ct) =>
            {
                var login = ctx.HttpContext.GetEndpoint()?.Metadata
                    .GetMetadata<EnableRateLimitingAttribute>()?.PolicyName == PoliticaLogin;

                ctx.HttpContext.Response.Headers.RetryAfter = "60";
                await ctx.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Muitas tentativas",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = login
                        ? "Você tentou entrar muitas vezes. Aguarde um minuto e tente de novo."
                        : "Muitas requisições em pouco tempo. Aguarde um minuto e tente de novo.",
                    Extensions = { ["code"] = login ? "MUITAS_TENTATIVAS" : "MUITAS_REQUISICOES" }
                }, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", ct);
            };
        });

        return services;
    }

    /// <summary>Janela fixa de 1 minuto por IP (o IP real vem do proxy quando <c>Proxy:Confiar</c> está ligado).</summary>
    private static RateLimitPartition<string> JanelaPorIp(HttpContext ctx, Func<SegurancaOptions, int> limite) =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limite(ctx.RequestServices.GetRequiredService<IOptions<SegurancaOptions>>().Value),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
}
