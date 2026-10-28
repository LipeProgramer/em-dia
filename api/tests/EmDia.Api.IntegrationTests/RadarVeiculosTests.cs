using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>CT31–CT37 · Radar, modo foco e veículos (E12, E20–E24 · RN02–RN04, RN07, RN13).</summary>
[Collection("Integration")]
public class RadarVeiculosTests(IntegrationTestFactory f)
{
    [Fact(DisplayName = "CT31 · Usuário com itens nos 4 anéis → 200; resumo correto; do mais urgente ao menos urgente")]
    public async Task CT31_RadarCompleto()
    {
        var c = await ApiCliente.CriarAsync(f);
        await c.CriarDataUnicaAsync("2027-05-03", "Externo", antecedencia: 30);  // 200 dias
        await c.CriarDataUnicaAsync("2026-10-12", "Centro", antecedencia: 30);   // −3 dias
        await c.CriarDataUnicaAsync("2026-12-14", "Anel2", antecedencia: 30);    // 60 dias
        await c.CriarDataUnicaAsync("2026-10-27", "Anel1", antecedencia: 30);    // 12 dias

        var resp = await c.Get("/api/v1/radar?foco=false");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ApiCliente.Json(resp);
        json["resumo"]!["vencidos"]!.GetValue<int>().Should().Be(1);
        json["resumo"]!["atencao"]!.GetValue<int>().Should().Be(1);
        json["resumo"]!["emDia"]!.GetValue<int>().Should().Be(2);
        var itens = json["itens"]!.AsArray();
        itens.Select(i => i!["anel"]!.GetValue<string>()).Should().Equal("Centro", "Anel1", "Anel2", "Externo");
        itens[0]!["textoPrazo"]!.GetValue<string>().Should().Be("venceu há 3 dias");
    }

    [Fact(DisplayName = "CT32 · foco = true → 200; apenas 3 itens, conforme RN04")]
    public async Task CT32_ModoFoco()
    {
        var c = await ApiCliente.CriarAsync(f);
        foreach (var (data, nome) in new[] { ("2027-01-23", "+100"), ("2026-10-20", "+5"), ("2026-10-13", "-2"),
                                             ("2026-11-04", "+20"), ("2026-10-05", "-10") })
            await c.CriarDataUnicaAsync(data, nome, antecedencia: 30);

        var json = await ApiCliente.Json(await c.Get("/api/v1/radar?foco=true"));

        json["modoFoco"]!.GetValue<bool>().Should().BeTrue();
        json["itens"]!.AsArray().Select(i => i!["nome"]!.GetValue<string>()).Should().Equal("-10", "-2", "+5");
        json["resumo"]!["vencidos"]!.GetValue<int>().Should().Be(2, "o resumo considera todos os itens");
    }

    [Fact(DisplayName = "CT33 · Usuário novo → 200; resumo zerado e lista vazia")]
    public async Task CT33_SemItens()
    {
        var c = await ApiCliente.CriarAsync(f);

        var json = await ApiCliente.Json(await c.Get("/api/v1/radar"));

        json["resumo"]!["vencidos"]!.GetValue<int>().Should().Be(0);
        json["resumo"]!["atencao"]!.GetValue<int>().Should().Be(0);
        json["resumo"]!["emDia"]!.GetValue<int>().Should().Be(0);
        json["itens"]!.AsArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "CT34 · PATCH kmAtual menor que a atual → 422")]
    public async Task CT34_KmMenor()
    {
        var c = await ApiCliente.CriarAsync(f);
        var v = await c.CriarVeiculoAsync();

        var resp = await c.Patch($"/api/v1/veiculos/{v}/km", new { kmAtual = 84_000 });

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ApiCliente.Json(resp))["detail"]!.GetValue<string>().Should().Be("A km não pode ser menor que 84.500.");
    }

    [Fact(DisplayName = "CT35 · PATCH que ultrapassa a km de um item → 200; item afetado passa a Vencido")]
    public async Task CT35_KmMaior()
    {
        var c = await ApiCliente.CriarAsync(f);
        var v = await c.CriarVeiculoAsync();
        var item = await c.CriarItemAsync(new
        {
            nome = "Troca de óleo", categoriaId = 1, tipoVencimento = "UsoOuTempo", veiculoId = v, dataBase = "2026-05-01",
            kmBase = 75_000, intervaloKm = 10_000, intervaloDias = 365, antecedenciaDias = 30
        });
        item["status"]!.GetValue<string>().Should().Be("Atencao");

        var resp = await c.Patch($"/api/v1/veiculos/{v}/km", new { kmAtual = 85_600 });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ApiCliente.Json(resp);
        json["kmAtual"]!.GetValue<int>().Should().Be(85_600);
        json["itensAfetados"]![0]!["status"]!.GetValue<string>().Should().Be("Vencido");
    }

    [Fact(DisplayName = "CT36 · DELETE de veículo ligado a itens → 409")]
    public async Task CT36_ExcluirVeiculoComItens()
    {
        var c = await ApiCliente.CriarAsync(f);
        var v = await c.CriarVeiculoAsync();
        await c.CriarItemAsync(new
        {
            nome = "Troca de óleo", categoriaId = 1, tipoVencimento = "UsoOuTempo", veiculoId = v, dataBase = "2026-05-01",
            kmBase = 75_000, intervaloKm = 10_000, intervaloDias = 365, antecedenciaDias = 30
        });

        (await c.Delete($"/api/v1/veiculos/{v}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var lista = (await ApiCliente.Json(await c.Get("/api/v1/veiculos"))).AsArray();
        lista[0]!["quantidadeItens"]!.GetValue<int>().Should().Be(1);
    }

    [Fact(DisplayName = "CT37 · POST com apelido já usado pelo usuário → 409")]
    public async Task CT37_ApelidoRepetido()
    {
        var c = await ApiCliente.CriarAsync(f);
        await c.CriarVeiculoAsync("Gol");

        var resp = await c.Post("/api/v1/veiculos", new { apelido = "gol", tipo = "Carro", kmAtual = 1, mediaKmMes = 100 });

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "E22/E24 · Editar veículo e excluir veículo sem itens → 200 e 204")]
    public async Task EditarEExcluirVeiculo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var v = await c.CriarVeiculoAsync();

        var editado = await c.Put($"/api/v1/veiculos/{v}", new { apelido = "Gol prata", tipo = "Carro", mediaKmMes = 2_200 });
        editado.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ApiCliente.Json(editado))["apelido"]!.GetValue<string>().Should().Be("Gol prata");

        (await c.Delete($"/api/v1/veiculos/{v}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
