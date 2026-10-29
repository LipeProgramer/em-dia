using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>Extra (DV22) · Teste pelo Swagger: criar item por parâmetros e exemplos válidos nos corpos.</summary>
[Collection("Integration")]
public class SwaggerTests(IntegrationTestFactory f)
{
    private async Task<JsonNode> SwaggerAsync() =>
        (await ApiCliente.Json(await ApiCliente.NovoHttp(f).GetAsync("/swagger/v1/swagger.json")))!;

    private static string Exemplo(JsonNode swagger, string schema) =>
        swagger["components"]!["schemas"]![schema]!["example"]!.ToJsonString();

    [Fact(DisplayName = "DV22 · POST /itens/rapido com parâmetros → 201; GET /itens/{id} devolve o item")]
    public async Task CriarPorParametros()
    {
        var c = await ApiCliente.CriarAsync(f);

        var resp = await c.Http.PostAsync(
            "/api/v1/itens/rapido?nome=IPVA&categoriaId=1&tipoVencimento=DataUnica&dataVencimento=2026-10-10&passos=Emitir%20o%20boleto&passos=Pagar",
            null);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await ApiCliente.Json(resp))["id"]!.GetValue<string>();
        var item = await ApiCliente.Json(await c.Get($"/api/v1/itens/{id}"));
        item["nome"]!.GetValue<string>().Should().Be("IPVA");
        item["status"]!.GetValue<string>().Should().Be("Vencido");
        item["antecedenciaDias"]!.GetValue<int>().Should().Be(30);
        item["passos"]!.AsArray().Should().HaveCount(2);
    }

    [Fact(DisplayName = "DV22 · Sem parâmetro obrigatório → 400 \"Campo obrigatório.\"; tipo sem a data → 400 no campo")]
    public async Task CriarPorParametros_Validacao()
    {
        var c = await ApiCliente.CriarAsync(f);

        var semNome = await c.Http.PostAsync("/api/v1/itens/rapido?categoriaId=1&tipoVencimento=DataUnica", null);
        var semData = await c.Http.PostAsync("/api/v1/itens/rapido?nome=CNH&categoriaId=2&tipoVencimento=DataUnica", null);

        semNome.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(semNome))["errors"]!["nome"]![0]!.GetValue<string>().Should().Be("Campo obrigatório.");
        semData.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(semData))["errors"]!["dataVencimento"].Should().NotBeNull();
    }

    [Fact(DisplayName = "DV22 · Exemplos do Swagger são aceitos pela API (cadastro, login, check-up, item, veículo)")]
    public async Task ExemplosDoSwagger_SaoValidos()
    {
        var swagger = await SwaggerAsync();
        var http = ApiCliente.NovoHttp(f);
        StringContent Corpo(string schema) => new(Exemplo(swagger, schema), System.Text.Encoding.UTF8, "application/json");

        (await http.PostAsync("/api/v1/auth/registrar", Corpo("RegistrarRequest"))).StatusCode.Should().Be(HttpStatusCode.Created);
        var login = await http.PostAsync("/api/v1/auth/login", Corpo("LoginRequest"));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = (await ApiCliente.Json(login))["accessToken"]!.GetValue<string>();
        http.DefaultRequestHeaders.Authorization = new("Bearer", token);

        (await http.PostAsync("/api/v1/checkup", Corpo("CheckupRequest"))).StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await http.PostAsync("/api/v1/itens", Corpo("SalvarItemRequest"));
        item.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await ApiCliente.Json(item))["id"]!.GetValue<string>();
        (await http.PostAsync($"/api/v1/itens/{id}/concluir", Corpo("ConcluirRequest"))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await http.PostAsync("/api/v1/veiculos", Corpo("CriarVeiculoRequest"))).StatusCode.Should().Be(HttpStatusCode.Created);
        (await http.PutAsync("/api/v1/usuarios/me/preferencias", Corpo("PreferenciasDto"))).StatusCode.Should().Be(HttpStatusCode.OK);

        swagger["paths"]!["/api/v1/itens"]!["get"]!["parameters"]!.AsArray()
            .Single(p => p!["name"]!.GetValue<string>() == "status")!["schema"]!["enum"]!.AsArray()
            .Select(v => v!.GetValue<string>()).Should().Equal("Vencido", "Atencao", "EmDia", "Encerrado");
    }
}
