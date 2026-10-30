using System.Buffers.Text;
using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Infrastructure.Jobs;
using EmDia.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmDia.Api.IntegrationTests;

/// <summary>Extra (DV20) · Notificações no celular: inscrição do aparelho e envio pela rotina de avisos.</summary>
[Collection("Integration")]
public class NotificacoesTests(IntegrationTestFactory f)
{
    private static object Inscricao(string endpoint) =>
        new { endpoint, p256dh = "BNcRdreALRFXTkOOUHK1EtK2wtaz5Ry4YfYCA_0QTpQtUbVlUls0VJXg7A8u-Ts1XbjhazAkj7I99e8QcYP7DkM", auth = "tBHItJI5svbpez7KI4CCXg" };

    private int NotificacoesPara(ApiCliente c) => f.Notificacoes.Enviadas.Count(n => n.UsuarioId == c.UsuarioId);

    private async Task RodarRotinaAsync()
    {
        await f.Services.GetRequiredService<AvisosWorker>().ExecutarAgoraAsync();
    }

    [Fact(DisplayName = "Push · Chave VAPID derivada da chave do JWT: ponto P-256 (65 bytes) e sempre a mesma")]
    public void ChaveVapid_Valida()
    {
        var chaves = f.Services.GetRequiredService<ChavesVapid>();
        var publica = Base64Url.DecodeFromChars(chaves.Publica);

        publica.Should().HaveCount(65);
        publica[0].Should().Be(0x04);
        Base64Url.DecodeFromChars(chaves.Privada).Should().HaveCount(32);
        chaves.Assunto.Should().StartWith("mailto:");
    }

    [Fact(DisplayName = "Push · Endereço fora dos serviços de push (SSRF) → 400 com o campo endpoint")]
    public async Task Inscricao_EnderecoQualquer_Recusa()
    {
        var c = await ApiCliente.CriarAsync(f);

        var http = await c.Post("/api/v1/push/inscricoes", Inscricao("http://fcm.googleapis.com/fcm/send/abc"));
        var interno = await c.Post("/api/v1/push/inscricoes", Inscricao("https://169.254.169.254/latest/meta-data"));

        http.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        interno.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(interno))["errors"]!["endpoint"].Should().NotBeNull();
    }

    [Fact(DisplayName = "Push · Aparelho inscrito recebe o aviso mesmo com e-mail desligado; ao remover, para de receber")]
    public async Task Rotina_EnviaParaAparelhoInscrito()
    {
        var c = await ApiCliente.CriarAsync(f);
        (await c.Put("/api/v1/usuarios/me/preferencias",
            new { receberEmail = false, maxReavisos = 3, intervaloReavisoDias = 3, modoFocoPadrao = false }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}";
        (await c.Post("/api/v1/push/inscricoes", Inscricao(endpoint))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await c.CriarDataUnicaAsync("2026-11-01", nome: "IPVA", antecedencia: 30); // em Atenção hoje

        await RodarRotinaAsync();

        NotificacoesPara(c).Should().Be(1);
        f.Emails.EmailsEnviados.Should().NotContain(e => e.Destinatario == c.Email);
        var (_, notificacao) = f.Notificacoes.Enviadas.Last(n => n.UsuarioId == c.UsuarioId);
        notificacao.Titulo.Should().Contain("IPVA");
        notificacao.Url.Should().StartWith("/itens/");
        notificacao.Corpo.Should().Be("Faltam 17 dias (01/11/2026).");

        (await c.Delete("/api/v1/push/inscricoes", new { endpoint })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await c.CriarDataUnicaAsync("2026-11-02", nome: "Licenciamento", antecedencia: 30);
        await RodarRotinaAsync();

        NotificacoesPara(c).Should().Be(1);
    }
}
