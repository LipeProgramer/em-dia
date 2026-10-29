using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>Extra (DV23) · Verificação leve usada para manter a API acordada no plano gratuito.</summary>
[Collection("Integration")]
public class SaudeTests(IntegrationTestFactory f)
{
    [Fact(DisplayName = "DV23 · GET /health responde 200 \"ok\" sem login e fica fora do Swagger")]
    public async Task Health()
    {
        var http = ApiCliente.NovoHttp(f);

        var resp = await http.GetAsync("/health");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).Should().Be("ok");
        (await http.GetStringAsync("/swagger/v1/swagger.json")).Should().NotContain("/health");
    }
}
