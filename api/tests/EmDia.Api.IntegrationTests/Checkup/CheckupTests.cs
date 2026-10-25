using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Checkup;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Checkup;

/// <summary>
/// CT44-CT47 – Testes de check-up (spec §6.5).
/// </summary>
[Collection("Integration")]
public class CheckupTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public CheckupTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"checkup.{sufixo}@teste.com";
        await reg.PostAsJsonAsync("/api/v1/auth/registrar",
            new RegistrarRequest($"User {sufixo}", email, "Senha@123"));
        var loginResp = await reg.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(email, "Senha@123"));
        var login = await loginResp.Content.ReadFromJsonAsync<LoginResponse>();

        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    // ── CT44 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT44_Checkup_SemAuth_Retorna401()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/v1/checkup",
            new ExecutarCheckupRequest(new Dictionary<string, bool>()));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT45 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT45_Checkup_RespostasVazias_Retorna200ItensCriadosZero()
    {
        var client = await ClienteAutenticadoAsync("ct45");
        var resp = await client.PostAsJsonAsync("/api/v1/checkup",
            new ExecutarCheckupRequest(new Dictionary<string, bool>()));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<CheckupResponse>();
        body!.ItensCriados.Should().Be(0);
    }

    // ── CT46 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT46_CheckupAtual_SemCheckupAnterior_Retorna204()
    {
        var client = await ClienteAutenticadoAsync("ct46");
        var resp = await client.GetAsync("/api/v1/checkup/atual");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // ── CT47 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT47_Checkup_ComRespostasTrue_CriaItens()
    {
        var client = await ClienteAutenticadoAsync("ct47");

        // TEM_PET (RecorrentePorTempo) e COMPROU_ELETRONICO (Calculado) não precisam
        // de data de vencimento nem veículo — podem ser criados com dataBase = hoje
        var respostas = new Dictionary<string, bool>
        {
            ["TEM_PET"]            = true,
            ["COMPROU_ELETRONICO"] = true,
            ["TEM_CNH"]            = false,
            ["TEM_CARRO"]          = false,
            ["TEM_PURIFICADOR"]    = false
        };
        var resp = await client.PostAsJsonAsync("/api/v1/checkup",
            new ExecutarCheckupRequest(respostas));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<CheckupResponse>();
        body!.ItensCriados.Should().Be(2);
        body.Respostas["TEM_PET"].Should().BeTrue();
        body.Respostas["COMPROU_ELETRONICO"].Should().BeTrue();

        // GET /checkup/atual deve retornar o checkup recém-feito
        var atualResp = await client.GetAsync("/api/v1/checkup/atual");
        atualResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var atual = await atualResp.Content.ReadFromJsonAsync<CheckupResponse>();
        atual!.ItensCriados.Should().Be(2);
    }
}
