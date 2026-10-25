using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Itens;
using EmDia.Domain.Enums;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Itens;

/// <summary>
/// CT48-CT51 – Testes de conclusão de item e histórico (spec §6.5).
/// </summary>
[Collection("Integration")]
public class ConcluirTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public ConcluirTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"concluir.{sufixo}@teste.com";
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

    private static CriarItemRequest ItemDataUnica(string nome) => new(
        Nome: nome,
        CategoriaId: 2,
        TipoVencimento: TipoVencimento.DataUnica,
        DataBase: null,
        DataVencimento: new DateOnly(2027, 12, 31),
        IntervaloDias: null,
        IntervaloKm: null,
        KmBase: null,
        GarantiaFabricanteMeses: null,
        AntecedenciaDias: 30,
        VeiculoId: null,
        ModeloItemId: null,
        Observacao: null,
        Passos: []);

    // ── CT48 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT48_ConcluirItem_SemAuth_Retorna401()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync(
            $"/api/v1/itens/{Guid.NewGuid()}/concluir",
            new ConcluirItemRequest(new DateOnly(2026, 10, 15), null, null, null, null, false));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CT49 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT49_ConcluirItem_OutroUsuario_Retorna404()
    {
        // Usuário A cria item
        var clientA = await ClienteAutenticadoAsync("ct49a");
        var criarResp = await clientA.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("CNH A"));
        var itemA = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        // Usuário B tenta concluir item de A
        var clientB = await ClienteAutenticadoAsync("ct49b");
        var resp = await clientB.PostAsJsonAsync(
            $"/api/v1/itens/{itemA!.Id}/concluir",
            new ConcluirItemRequest(new DateOnly(2026, 10, 15), null, null, null, null, false));
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CT50 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT50_ConcluirItem_Proprio_Retorna200()
    {
        var client = await ClienteAutenticadoAsync("ct50");
        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("CNH CT50"));
        criarResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        var req = new ConcluirItemRequest(
            DataConclusao: new DateOnly(2026, 10, 15),
            KmNaConclusao: null,
            ValorGasto: 150.00m,
            NovaDataVencimento: null,
            Observacao: "Renovada com sucesso",
            ConfirmarPassosPendentes: false);

        var resp = await client.PostAsJsonAsync($"/api/v1/itens/{item!.Id}/concluir", req);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ItemResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(item.Id);
    }

    // ── CT51 ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT51_HistoricoItem_AposConcluir_Retorna200ComRegistro()
    {
        var client = await ClienteAutenticadoAsync("ct51");
        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("CNH CT51"));
        var item = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        // Concluir o item
        var reqConcluir = new ConcluirItemRequest(
            DataConclusao: new DateOnly(2026, 10, 15),
            KmNaConclusao: null,
            ValorGasto: null,
            NovaDataVencimento: null,
            Observacao: null,
            ConfirmarPassosPendentes: false);
        await client.PostAsJsonAsync($"/api/v1/itens/{item!.Id}/concluir", reqConcluir);

        // Obter histórico
        var resp = await client.GetAsync($"/api/v1/itens/{item.Id}/historico");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<HistoricoResponse>>();
        lista.Should().HaveCount(1);
        lista![0].ItemId.Should().Be(item.Id);
        lista[0].ConcluidoEm.Should().Be(new DateOnly(2026, 10, 15));
    }
}
