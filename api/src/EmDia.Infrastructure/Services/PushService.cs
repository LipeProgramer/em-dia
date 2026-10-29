using System.Buffers.Text;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EmDia.Application;
using EmDia.Application.Interfaces;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// Chaves VAPID que identificam a API para os serviços de push. Por padrão são derivadas da chave do JWT
/// (HKDF), então não há outro segredo para configurar; <c>Push:VapidPrivada</c> (32 bytes em base64url)
/// substitui a derivação. Trocar a chave do JWT exige que os aparelhos ativem as notificações de novo.
/// </summary>
public sealed class ChavesVapid
{
    public string Publica { get; }
    public string Privada { get; }
    public string Assunto { get; }

    public ChavesVapid(IOptions<JwtOptions> jwt, IConfiguration config)
    {
        var d = config["Push:VapidPrivada"] is { Length: > 0 } privada
            ? Base64Url.DecodeFromChars(privada)
            : HKDF.DeriveKey(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(jwt.Value.Key), 32,
                info: "emdia-vapid-p256"u8.ToArray());

        using var ec = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, D = d });
        var q = ec.ExportParameters(false).Q;
        Publica = Base64Url.EncodeToString([0x04, .. q.X!, .. q.Y!]);
        Privada = Base64Url.EncodeToString(d);
        Assunto = "mailto:" + (config["Email:Remetente"] is { Length: > 0 } r ? r : "noreply@emdia.app");
    }
}

/// <summary>Envia notificações Web Push para os aparelhos do usuário; remove os que não existem mais (404/410).</summary>
public class PushService(
    EmDiaDbContext db,
    ChavesVapid chaves,
    ILogger<PushService> logger) : IPushService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public string ChavePublica => chaves.Publica;

    public async Task<int> EnviarAsync(Guid usuarioId, NotificacaoPush notificacao, CancellationToken ct = default)
    {
        var inscricoes = await db.InscricoesPush.Where(i => i.UsuarioId == usuarioId).ToListAsync(ct);
        if (inscricoes.Count == 0)
            return 0;

        var cliente = new WebPushClient(Http);
        var vapid = new VapidDetails(chaves.Assunto, chaves.Publica, chaves.Privada);
        var conteudo = JsonSerializer.Serialize(new { titulo = notificacao.Titulo, corpo = notificacao.Corpo, url = notificacao.Url });
        var enviadas = 0;

        foreach (var inscricao in inscricoes)
        {
            try
            {
                await cliente.SendNotificationAsync(
                    new PushSubscription(inscricao.Endpoint, inscricao.P256dh, inscricao.Auth), conteudo, vapid, ct);
                enviadas++;
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                db.InscricoesPush.Remove(inscricao); // o aparelho desativou ou desinstalou o app
            }
            catch (WebPushException ex)
            {
                logger.LogWarning("Falha ao enviar notificação ({Status})", ex.StatusCode);
            }
        }

        await db.SaveChangesAsync(ct);
        return enviadas;
    }
}
