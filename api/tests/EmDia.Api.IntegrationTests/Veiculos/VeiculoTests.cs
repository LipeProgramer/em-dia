using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Domain.Enums;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Veiculos;

/// <summary>
/// CT29-CT33 — Testes de veículos (spec §6.4).
/// </summary>
[Collection("Integration")]
public class VeiculoTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public VeiculoTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"veiculo.{sufixo}@teste.com";
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

    // ── CT29 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT29_CriarVeiculo_Retorna201()
    {
        var client = await ClienteAutenticadoAsync("ct29");
        var req = new CriarVeiculoRequest("Meu Carro", TipoVeiculo.Carro, 15000, 500);

        var resp = await client.PostAsJsonAsync("/api/v1/veiculos", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<VeiculoResponse>();
        body!.Apelido.Should().Be("Meu Carro");
        body.KmAtual.Should().Be(15000);
        body.MediaKmMes.Should().Be(500);
        body.Id.Should().NotBeEmpty();
    }

    // ── CT30 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT30_ObterVeiculo_Retorna200()
    {
        var client = await ClienteAutenticadoAsync("ct30");
        var criarResp = await client.PostAsJsonAsync("/api/v1/veiculos",
            new CriarVeiculoRequest("Minha Moto", TipoVeiculo.Moto, 5000, 200));
        var criado = await criarResp.Content.ReadFromJsonAsync<VeiculoResponse>();

        var resp = await client.GetAsync($"/api/v1/veiculos/{criado!.Id}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<VeiculoResponse>();
        body!.Id.Should().Be(criado.Id);
        body.Apelido.Should().Be("Minha Moto");
    }

    // ── CT31 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT31_EditarVeiculo_Retorna200()
    {
        var client = await ClienteAutenticadoAsync("ct31");
        var criarResp = await client.PostAsJsonAsync("/api/v1/veiculos",
            new CriarVeiculoRequest("Carro Velho", TipoVeiculo.Carro, 80000, 800));
        var criado = await criarResp.Content.ReadFromJsonAsync<VeiculoResponse>();

        var resp = await client.PutAsJsonAsync($"/api/v1/veiculos/{criado!.Id}",
            new EditarVeiculoRequest("Carro Novo", TipoVeiculo.Carro, 600));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<VeiculoResponse>();
        body!.Apelido.Should().Be("Carro Novo");
        body.MediaKmMes.Should().Be(600);
    }

    // ── CT32 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT32_ExcluirVeiculo_Retorna204()
    {
        var client = await ClienteAutenticadoAsync("ct32");
        var criarResp = await client.PostAsJsonAsync("/api/v1/veiculos",
            new CriarVeiculoRequest("Deletar", TipoVeiculo.Moto, 0, 100));
        var criado = await criarResp.Content.ReadFromJsonAsync<VeiculoResponse>();

        var resp = await client.DeleteAsync($"/api/v1/veiculos/{criado!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Confirmar que sumiu
        var getResp = await client.GetAsync($"/api/v1/veiculos/{criado.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CT33 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT33_ObterVeiculoOutroUsuario_Retorna404()
    {
        var clientA = await ClienteAutenticadoAsync("ct33a");
        var clientB = await ClienteAutenticadoAsync("ct33b");

        var criarResp = await clientA.PostAsJsonAsync("/api/v1/veiculos",
            new CriarVeiculoRequest("Carro A", TipoVeiculo.Carro, 10000, 300));
        var criado = await criarResp.Content.ReadFromJsonAsync<VeiculoResponse>();

        // Usuário B tenta acessar veículo de A → 404
        var resp = await clientB.GetAsync($"/api/v1/veiculos/{criado!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
