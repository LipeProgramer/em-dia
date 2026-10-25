using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Preferencias;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Preferencias;

/// <summary>
/// CT52-CT55 – Testes de preferências de aviso (spec §6.5).
/// </summary>
[Collection("Integration")]
public class PreferenciasTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public PreferenciasTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"pref.{sufixo}@teste.com";
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

    // ── CT52 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT52_Preferencias_SemAuth_Retorna401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/v1/usuarios/me/preferencias");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT53 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT53_GetPreferencias_Retorna200ComCampos()
    {
        var client = await ClienteAutenticadoAsync("ct53");
        var resp = await client.GetAsync("/api/v1/usuarios/me/preferencias");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<PreferenciasResponse>();
        body.Should().NotBeNull();
        // Valores padrão definidos na entidade PreferenciasAviso
        body!.ReceberEmail.Should().BeTrue();
        body.MaxReavisos.Should().Be(2);
        body.IntervaloReavisoDias.Should().Be(3);
        body.ModoFocoPadrao.Should().BeFalse();
        body.AtualizadoEm.Should().NotBe(default);
    }

    // ── CT54 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT54_PutPreferencias_Retorna200ComValoresAtualizados()
    {
        var client = await ClienteAutenticadoAsync("ct54");

        var req = new AtualizarPreferenciasRequest(
            ReceberEmail: false,
            MaxReavisos: 5,
            IntervaloReavisoDias: 7,
            ModoFocoPadrao: true);

        var resp = await client.PutAsJsonAsync("/api/v1/usuarios/me/preferencias", req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<PreferenciasResponse>();
        body!.ReceberEmail.Should().BeFalse();
        body.MaxReavisos.Should().Be(5);
        body.IntervaloReavisoDias.Should().Be(7);
        body.ModoFocoPadrao.Should().BeTrue();
    }

    // ── CT55 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT55_GetPreferencias_AposPut_ConfirmaValores()
    {
        var client = await ClienteAutenticadoAsync("ct55");

        var req = new AtualizarPreferenciasRequest(
            ReceberEmail: false,
            MaxReavisos: 3,
            IntervaloReavisoDias: 5,
            ModoFocoPadrao: true);
        await client.PutAsJsonAsync("/api/v1/usuarios/me/preferencias", req);

        var resp = await client.GetAsync("/api/v1/usuarios/me/preferencias");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<PreferenciasResponse>();
        body!.ReceberEmail.Should().BeFalse();
        body.MaxReavisos.Should().Be(3);
        body.IntervaloReavisoDias.Should().Be(5);
        body.ModoFocoPadrao.Should().BeTrue();
    }
}
