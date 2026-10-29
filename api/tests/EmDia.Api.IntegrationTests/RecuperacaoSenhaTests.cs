using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>Extra (DV24) · Esqueci minha senha: link por e-mail, uso único, 30 minutos.</summary>
[Collection("Integration")]
public partial class RecuperacaoSenhaTests(IntegrationTestFactory f)
{
    [GeneratedRegex(@"/redefinir-senha\?token=([A-Za-z0-9_\-%]+)")]
    private static partial Regex LinkRegex();

    private List<string> EmailsPara(string email) =>
        f.Emails.EmailsEnviados.Where(e => e.Destinatario == email).Select(e => e.Corpo).ToList();

    private string TokenDoUltimoEmail(string email) =>
        Uri.UnescapeDataString(LinkRegex().Match(EmailsPara(email).Last()).Groups[1].Value);

    [Fact(DisplayName = "DV24 · Pedir link → e-mail com link; nova senha vale, antiga não, sessões encerradas, link não reusa")]
    public async Task FluxoCompleto()
    {
        var c = await ApiCliente.CriarAsync(f);
        var cookieAntigo = c.RefreshCookie;

        var pedido = await ApiCliente.NovoHttp(f).PostAsJsonAsync("/api/v1/auth/esqueci-senha", new { email = c.Email.ToUpperInvariant() });

        pedido.StatusCode.Should().Be(HttpStatusCode.NoContent);
        EmailsPara(c.Email).Should().ContainSingle().Which.Should().Contain("/redefinir-senha?token=");
        var token = TokenDoUltimoEmail(c.Email);

        var redefinir = await ApiCliente.NovoHttp(f).PostAsJsonAsync("/api/v1/auth/redefinir-senha", new { token, novaSenha = "NovaSenha456" });
        redefinir.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await c.EntrarAsync()).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await c.EntrarAsync("NovaSenha456")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.RefreshAsync(cookieAntigo)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var deNovo = await ApiCliente.NovoHttp(f).PostAsJsonAsync("/api/v1/auth/redefinir-senha", new { token, novaSenha = "Outra789x" });
        deNovo.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ApiCliente.Json(deNovo))["code"]!.GetValue<string>().Should().Be("LINK_INVALIDO");
    }

    [Fact(DisplayName = "DV24 · E-mail sem conta → mesma resposta 204 e nenhum e-mail; pedido repetido em 1 minuto não reenvia")]
    public async Task NaoRevelaContaENaoRepete()
    {
        var http = ApiCliente.NovoHttp(f);
        var inexistente = $"ninguem-{Guid.NewGuid():N}@email.com";

        (await http.PostAsJsonAsync("/api/v1/auth/esqueci-senha", new { email = inexistente })).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        EmailsPara(inexistente).Should().BeEmpty();

        var c = await ApiCliente.CriarAsync(f);
        await http.PostAsJsonAsync("/api/v1/auth/esqueci-senha", new { email = c.Email });
        (await http.PostAsJsonAsync("/api/v1/auth/esqueci-senha", new { email = c.Email })).StatusCode
            .Should().Be(HttpStatusCode.NoContent);
        EmailsPara(c.Email).Should().HaveCount(1);
    }

    [Fact(DisplayName = "DV24 · Link vencido (mais de 30 minutos) → 422; senha fraca → 400; versões por parâmetros funcionam")]
    public async Task ValidadeERotasPorParametros()
    {
        var c = await ApiCliente.CriarAsync(f);
        var http = ApiCliente.NovoHttp(f);

        (await http.PostAsync($"/api/v1/auth/esqueci-senha/rapido?email={c.Email}", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var token = TokenDoUltimoEmail(c.Email);

        var fraca = await http.PostAsJsonAsync("/api/v1/auth/redefinir-senha", new { token, novaSenha = "123" });
        fraca.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(fraca))["errors"]!["novaSenha"].Should().NotBeNull();

        try
        {
            f.Relogio.Hoje = RelogioFixo.HojePadrao.AddDays(1);
            var vencido = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/redefinir-senha/rapido?token={Uri.EscapeDataString(token)}")
            {
                Headers = { { "novaSenha", "NovaSenha456" } }
            };
            (await http.SendAsync(vencido)).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        finally
        {
            f.Relogio.Hoje = RelogioFixo.HojePadrao;
        }

        var valido = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/auth/redefinir-senha/rapido?token={Uri.EscapeDataString(token)}")
        {
            Headers = { { "novaSenha", "NovaSenha456" } }
        };
        (await http.SendAsync(valido)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await c.EntrarAsync("NovaSenha456")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
