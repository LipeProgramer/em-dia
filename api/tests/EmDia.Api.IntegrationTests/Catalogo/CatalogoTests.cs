using System.Net;
using System.Net.Http.Json;
using EmDia.Api.IntegrationTests.Fixtures;
using EmDia.Application.DTOs.Catalogo;
using FluentAssertions;

namespace EmDia.Api.IntegrationTests.Catalogo;

/// <summary>
/// CT18-CT20 — Testes do catálogo público (spec §6.4).
/// </summary>
[Collection("Integration")]
public class CatalogoTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public CatalogoTests(IntegrationTestFactory factory)
    {
        // Catálogo é público — cliente sem autenticação
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // ── CT18 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT18_ListarCategorias_Retorna200ComLista()
    {
        var resp = await _client.GetAsync("/api/v1/catalogo/categorias");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        lista.Should().NotBeNullOrEmpty();
        lista!.Should().AllSatisfy(c =>
        {
            c.Id.Should().BeGreaterThan(0);
            c.Nome.Should().NotBeNullOrEmpty();
        });
    }

    // ── CT19 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT19_ListarModelos_Retorna200ComLista()
    {
        var resp = await _client.GetAsync("/api/v1/catalogo/modelos");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<ModeloResponse>>();
        lista.Should().NotBeNullOrEmpty();
        lista!.Should().AllSatisfy(m => m.Nome.Should().NotBeNullOrEmpty());
    }

    // ── CT20 ─────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CT20_ListarModelosPorCategoria_FiltraCorretamente()
    {
        var resp = await _client.GetAsync("/api/v1/catalogo/modelos?categoriaId=1");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var lista = await resp.Content.ReadFromJsonAsync<List<ModeloResponse>>();
        lista.Should().NotBeNull();
        lista!.Should().AllSatisfy(m => m.CategoriaId.Should().Be(1));
    }
}
