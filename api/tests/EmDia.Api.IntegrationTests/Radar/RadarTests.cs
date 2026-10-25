using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Radar;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Radar;

/// <summary>
/// CT42-CT43 – Testes do radar (spec §6.5).
/// </summary>
[Collection("Integration")]
public class RadarTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public RadarTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"radar.{sufixo}@teste.com";
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

    // ── CT42 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT42_Radar_SemAuth_Retorna401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/v1/radar");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT43 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT43_Radar_ComAuth_Retorna200ComEstrutura()
    {
        var client = await ClienteAutenticadoAsync("ct43");
        var resp = await client.GetAsync("/api/v1/radar");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<RadarResponse>();
        body.Should().NotBeNull();
        body!.Centro.Should().NotBeNull();
        body.Anel1.Should().NotBeNull();
        body.Anel2.Should().NotBeNull();
        body.Externo.Should().NotBeNull();
        // usuário novo sem itens — todos os anéis vazios
        body.Centro.Should().BeEmpty();
        body.Anel1.Should().BeEmpty();
        body.Anel2.Should().BeEmpty();
        body.Externo.Should().BeEmpty();
    }
}
