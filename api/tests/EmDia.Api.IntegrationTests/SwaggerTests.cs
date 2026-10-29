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

    private static HttpRequestMessage ComSenha(HttpMethod metodo, string url, string senha) =>
        new(metodo, url) { Headers = { { "senha", senha } } };

    [Fact(DisplayName = "DV22 · Fluxo completo só com rotas por parâmetros (conta, check-up, veículo, item, exclusão)")]
    public async Task FluxoCompletoPorParametros()
    {
        var http = ApiCliente.NovoHttp(f);
        var email = $"param-{Guid.NewGuid():N}@email.com";
        async Task<JsonNode> Ok(HttpResponseMessage r, HttpStatusCode esperado)
        {
            r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
            return esperado == HttpStatusCode.NoContent ? new JsonObject() : (await ApiCliente.Json(r))!;
        }

        await Ok(await http.SendAsync(ComSenha(HttpMethod.Post, $"/api/v1/auth/registrar/rapido?nome=Demo&email={email}", "Senha123")), HttpStatusCode.Created);
        var login = await Ok(await http.SendAsync(ComSenha(HttpMethod.Post, $"/api/v1/auth/login/rapido?email={email}", "Senha123")), HttpStatusCode.OK);
        http.DefaultRequestHeaders.Authorization = new("Bearer", login["accessToken"]!.GetValue<string>());

        var prefs = await Ok(await http.PutAsync(
            "/api/v1/usuarios/me/preferencias/rapido?receberEmail=false&maxReavisos=1&intervaloReavisoDias=2&modoFocoPadrao=true", null), HttpStatusCode.OK);
        prefs["modoFocoPadrao"]!.GetValue<bool>().Should().BeTrue();

        var checkup = await Ok(await http.PostAsync(
            "/api/v1/checkup/rapido?sim=TEM_CARRO&sim=TEM_CNH&veiculoApelido=Gol&veiculoKmAtual=50000" +
            "&modeloId=a7000000-0000-0000-0000-000000000001&dataBase=2026-06-01&kmBase=45000", null), HttpStatusCode.Created);
        checkup["itensCriados"]!.GetValue<int>().Should().Be(1);
        var veiculos = await Ok(await http.GetAsync("/api/v1/veiculos"), HttpStatusCode.OK);
        veiculos.AsArray().Single()!["quantidadeItens"]!.GetValue<int>().Should().Be(1);

        var moto = await Ok(await http.PostAsync("/api/v1/veiculos/rapido?apelido=Moto&tipo=Moto&kmAtual=1000&mediaKmMes=300", null), HttpStatusCode.Created);
        var motoId = moto["id"]!.GetValue<string>();
        (await Ok(await http.PutAsync($"/api/v1/veiculos/{motoId}/rapido?apelido=CG&tipo=Moto&mediaKmMes=400", null), HttpStatusCode.OK))
            ["apelido"]!.GetValue<string>().Should().Be("CG");
        (await Ok(await http.PatchAsync($"/api/v1/veiculos/{motoId}/km/rapido?kmAtual=1500", null), HttpStatusCode.OK))
            ["kmAtual"]!.GetValue<int>().Should().Be(1500);

        var item = await Ok(await http.PostAsync(
            "/api/v1/itens/rapido?nome=Passaporte&categoriaId=2&tipoVencimento=DataUnica&dataVencimento=2027-05-01&passos=Agendar", null), HttpStatusCode.Created);
        var itemId = item["id"]!.GetValue<string>();
        var editado = await Ok(await http.PutAsync(
            $"/api/v1/itens/{itemId}/rapido?nome=Passaporte%20novo&categoriaId=2&tipoVencimento=DataUnica&dataVencimento=2027-06-01&antecedenciaDias=60&passos=Agendar",
            null), HttpStatusCode.OK);
        editado["nome"]!.GetValue<string>().Should().Be("Passaporte novo");
        editado["antecedenciaDias"]!.GetValue<int>().Should().Be(60);
        var passoId = editado["passos"]![0]!["id"]!.GetValue<string>();
        (await Ok(await http.PatchAsync($"/api/v1/itens/{itemId}/passos/{passoId}/rapido?concluido=true", null), HttpStatusCode.OK))
            ["progressoPassos"]!.GetValue<double>().Should().Be(1);
        (await Ok(await http.PostAsync($"/api/v1/itens/{itemId}/concluir/rapido?dataConclusao=2026-10-15&novaDataVencimento=2036-10-15", null), HttpStatusCode.OK))
            ["item"]!["dataVencimento"]!.GetValue<string>().Should().Be("2036-10-15");

        await Ok(await http.DeleteAsync($"/api/v1/itens/{itemId}"), HttpStatusCode.NoContent);
        await Ok(await http.DeleteAsync($"/api/v1/veiculos/{motoId}"), HttpStatusCode.NoContent);
        (await http.SendAsync(ComSenha(HttpMethod.Delete, "/api/v1/usuarios/me/rapido", "Errada123"))).StatusCode
            .Should().Be(HttpStatusCode.UnprocessableEntity);
        await Ok(await http.SendAsync(ComSenha(HttpMethod.Delete, "/api/v1/usuarios/me/rapido", "Senha123")), HttpStatusCode.NoContent);
        (await http.SendAsync(ComSenha(HttpMethod.Post, $"/api/v1/auth/login/rapido?email={email}", "Senha123"))).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "DV22 · Rotas por parâmetros: obrigatório faltando → 400 \"Campo obrigatório.\"")]
    public async Task RotasPorParametros_Obrigatorios()
    {
        var c = await ApiCliente.CriarAsync(f);
        async Task<string> Erro(HttpResponseMessage r, string campo)
        {
            r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            return (await ApiCliente.Json(r))["errors"]![campo]![0]!.GetValue<string>();
        }

        (await Erro(await c.Http.PostAsync($"/api/v1/auth/login/rapido?email={c.Email}", null), "senha")).Should().Be("Campo obrigatório.");
        (await Erro(await c.Http.PostAsync("/api/v1/checkup/rapido", null), "sim")).Should().Be("Campo obrigatório.");
        (await Erro(await c.Http.PostAsync("/api/v1/veiculos/rapido?apelido=Gol&tipo=Carro&mediaKmMes=1000", null), "kmAtual"))
            .Should().Be("Campo obrigatório.");
        (await Erro(await c.Http.PutAsync("/api/v1/usuarios/me/preferencias/rapido?receberEmail=true", null), "maxReavisos"))
            .Should().Be("Campo obrigatório.");
        (await Erro(await c.Http.PostAsync($"/api/v1/itens/{Guid.NewGuid()}/concluir/rapido", null), "dataConclusao"))
            .Should().Be("Campo obrigatório.");
    }

    [Fact(DisplayName = "DV22 · Swagger documenta toda rota: resumo, descrição de cada parâmetro e de cada resposta")]
    public async Task SwaggerDocumentaTudo()
    {
        var swagger = await SwaggerAsync();
        var semDoc = new List<string>();

        foreach (var (caminho, rota) in swagger["paths"]!.AsObject())
            foreach (var (metodo, op) in rota!.AsObject())
            {
                var nome = $"{metodo.ToUpperInvariant()} {caminho}";
                if (string.IsNullOrWhiteSpace(op!["summary"]?.GetValue<string>())) semDoc.Add($"{nome}: resumo");
                foreach (var p in op["parameters"]?.AsArray() ?? [])
                    if (string.IsNullOrWhiteSpace(p!["description"]?.GetValue<string>())) semDoc.Add($"{nome}: {p["name"]}");
                foreach (var (codigo, resp) in op["responses"]!.AsObject())
                    if (resp!["description"]?.GetValue<string>() is null or "OK" or "Created" or "No Content" or "Bad Request"
                        or "Not Found" or "Conflict" or "Unprocessable Content" or "Unprocessable Entity")
                        semDoc.Add($"{nome}: resposta {codigo}");
            }

        semDoc.Should().BeEmpty();
        var senha = swagger["paths"]!["/api/v1/auth/login/rapido"]!["post"]!["parameters"]!.AsArray()
            .Single(p => p!["name"]!.GetValue<string>() == "senha")!;
        senha["in"]!.GetValue<string>().Should().Be("header", "a senha nunca vai no endereço");
        senha["required"]!.GetValue<bool>().Should().BeTrue();
        swagger["paths"]!["/api/v1/checkup/rapido"]!["post"]!["parameters"]!.AsArray()
            .Single(p => p!["name"]!.GetValue<string>() == "sim")!["schema"]!["items"]!["enum"]!.AsArray()
            .Should().Contain(v => v!.GetValue<string>() == "TEM_CNH");
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
