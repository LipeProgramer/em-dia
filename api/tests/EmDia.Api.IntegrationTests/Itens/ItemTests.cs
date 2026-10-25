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
/// CT34-CT41 — Testes de itens (spec §6.4).
/// </summary>
[Collection("Integration")]
public class ItemTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public ItemTests(IntegrationTestFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string sufixo)
    {
        var reg = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var email = $"item.{sufixo}@teste.com";
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

    private static CriarItemRequest ItemDataUnica(string nome = "CNH") =>
        new(
            Nome: nome,
            CategoriaId: 2,
            TipoVencimento: TipoVencimento.DataUnica,
            DataBase: null,
            DataVencimento: new DateOnly(2027, 6, 30),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 30,
            VeiculoId: null,
            ModeloItemId: null,
            Observacao: "Renovar na Detran",
            Passos: ["Separar documentos", "Agendar atendimento"]);

    // ── CT34 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT34_CriarItem_Retorna201()
    {
        var client = await ClienteAutenticadoAsync("ct34");
        var resp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("CNH CT34"));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<ItemResponse>();
        body!.Nome.Should().Be("CNH CT34");
        body.TipoVencimento.Should().Be(TipoVencimento.DataUnica);
        body.Passos.Should().HaveCount(2);
        body.Id.Should().NotBeEmpty();
    }

    // ── CT35 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT35_ListarItens_RetornaItemCriado()
    {
        var client = await ClienteAutenticadoAsync("ct35");
        await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("IPTU CT35"));

        var resp = await client.GetAsync("/api/v1/itens");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<ItemResponse>>();
        lista.Should().NotBeNullOrEmpty();
        lista!.Should().Contain(i => i.Nome == "IPTU CT35");
    }

    // ── CT36 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT36_EditarItem_Retorna200ComNovoNome()
    {
        var client = await ClienteAutenticadoAsync("ct36");
        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("Item Original CT36"));
        var criado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        var editReq = new EditarItemRequest(
            Nome: "Item Editado CT36",
            CategoriaId: 2,
            DataBase: null,
            DataVencimento: new DateOnly(2028, 1, 1),
            IntervaloDias: null,
            IntervaloKm: null,
            KmBase: null,
            GarantiaFabricanteMeses: null,
            AntecedenciaDias: 15,
            VeiculoId: null,
            Observacao: null);

        var resp = await client.PutAsJsonAsync($"/api/v1/itens/{criado!.Id}", editReq);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ItemResponse>();
        body!.Nome.Should().Be("Item Editado CT36");
        body.AntecedenciaDias.Should().Be(15);
    }

    // ── CT37 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT37_ExcluirItem_Retorna204ESumeDaLista()
    {
        var client = await ClienteAutenticadoAsync("ct37");
        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("Deletar CT37"));
        var criado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        var resp = await client.DeleteAsync($"/api/v1/itens/{criado!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await client.GetAsync($"/api/v1/itens/{criado.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CT38 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT38_MarcarPasso_Retorna200()
    {
        var client = await ClienteAutenticadoAsync("ct38");
        var criarResp = await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("Item Passos CT38"));
        var criado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();
        var passoId = criado!.Passos[0].Id;

        var resp = await client.PatchAsJsonAsync(
            $"/api/v1/itens/{criado.Id}/passos/{passoId}",
            new MarcarPassoRequest(true));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<ItemResponse>();
        body!.Passos.First(p => p.Id == passoId).Concluido.Should().BeTrue();
    }

    // ── CT39 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT39_ObterItemOutroUsuario_Retorna404()
    {
        var clientA = await ClienteAutenticadoAsync("ct39a");
        var clientB = await ClienteAutenticadoAsync("ct39b");

        var criarResp = await clientA.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("Item A CT39"));
        var criado = await criarResp.Content.ReadFromJsonAsync<ItemResponse>();

        var resp = await clientB.GetAsync($"/api/v1/itens/{criado!.Id}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CT40 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT40_CriarItemSemNome_Retorna400()
    {
        var client = await ClienteAutenticadoAsync("ct40");
        var req = ItemDataUnica("") with { Nome = "" };

        var resp = await client.PostAsJsonAsync("/api/v1/itens", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── CT41 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT41_ListarItensPorCategoria_FiltraCorretamente()
    {
        var client = await ClienteAutenticadoAsync("ct41");
        // Categoria 2 = Documentos
        await client.PostAsJsonAsync("/api/v1/itens", ItemDataUnica("Doc Cat2 CT41"));
        // Categoria 3 = Casa
        await client.PostAsJsonAsync("/api/v1/itens",
            ItemDataUnica("Item Casa CT41") with { CategoriaId = 3 });

        var resp = await client.GetAsync("/api/v1/itens?categoriaId=2");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<ItemResponse>>();
        lista.Should().NotBeNullOrEmpty();
        lista!.Should().AllSatisfy(i => i.CategoriaId.Should().Be(2));
    }
}
