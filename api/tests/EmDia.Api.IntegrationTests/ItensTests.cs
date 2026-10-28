using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>CT18–CT30 · Itens, conclusão e passos (E13–E19 · RN01, RN02, RN05–RN07, RN12).</summary>
[Collection("Integration")]
public class ItensTests(IntegrationTestFactory f)
{
    [Fact(DisplayName = "CT18 · Criar data única (CNH com validade futura e passos) → 201; próximo vencimento e status corretos")]
    public async Task CT18_CriarDataUnica()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/itens", new
        {
            nome = "CNH", categoriaId = 2, modeloId = "a7000000-0000-0000-0000-000000000003", tipoVencimento = "DataUnica",
            dataVencimento = "2026-11-20", antecedenciaDias = 45, passos = new[] { "Agendar exame", "Pagar taxa" }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        resp.Headers.Location!.ToString().Should().StartWith("/api/v1/itens/");
        var json = await ApiCliente.Json(resp);
        json["proximoVencimentoData"]!.GetValue<string>().Should().Be("2026-11-20");
        json["status"]!.GetValue<string>().Should().Be("Atencao");
        json["textoPrazo"]!.GetValue<string>().Should().Be("faltam 36 dias");
        json["passos"]!.AsArray().Select(p => p!["descricao"]!.GetValue<string>())
            .Should().Equal("Agendar exame", "Pagar taxa");
    }

    [Fact(DisplayName = "CT19 · Recorrente sem intervaloDias → 400")]
    public async Task CT19_RecorrenteSemIntervalo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/itens", new
        {
            nome = "Filtro", categoriaId = 3, tipoVencimento = "RecorrentePorTempo", dataBase = "2026-05-01", antecedenciaDias = 15
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(resp))["errors"]!["intervaloDias"]![0]!.GetValue<string>().Should().Be("Informe o intervalo em dias.");
    }

    [Fact(DisplayName = "CT20 · veiculoId de outro usuário → 404")]
    public async Task CT20_VeiculoDeOutraPessoa()
    {
        var dono = await ApiCliente.CriarAsync(f);
        var veiculoAlheio = await dono.CriarVeiculoAsync();
        var c = await ApiCliente.CriarAsync(f);

        var resp = await c.Post("/api/v1/itens", new
        {
            nome = "Troca de óleo", categoriaId = 1, tipoVencimento = "UsoOuTempo", veiculoId = veiculoAlheio,
            dataBase = "2026-05-01", kmBase = 75_000, intervaloKm = 10_000, intervaloDias = 365, antecedenciaDias = 30
        });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "CT21 · GET /itens/{id} de outro usuário → 404 (não revela que existe)")]
    public async Task CT21_ItemAlheio()
    {
        var dono = await ApiCliente.CriarAsync(f);
        var item = await dono.CriarDataUnicaAsync("2027-03-20");
        var c = await ApiCliente.CriarAsync(f);

        var resp = await c.Get($"/api/v1/itens/{item["id"]}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ApiCliente.Json(resp))["detail"]!.GetValue<string>().Should().Be("Não encontramos este item.");
    }

    [Fact(DisplayName = "CT22 · Editar intervalo de 180 para 365 dias → 200; próximo vencimento recalculado")]
    public async Task CT22_EditarIntervalo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-05-01", 180);
        item["proximoVencimentoData"]!.GetValue<string>().Should().Be("2026-10-28");

        var resp = await c.Put($"/api/v1/itens/{item["id"]}", new
        {
            nome = "Filtro", categoriaId = 3, tipoVencimento = "RecorrentePorTempo", dataBase = "2026-05-01",
            intervaloDias = 365, antecedenciaDias = 15
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ApiCliente.Json(resp))["proximoVencimentoData"]!.GetValue<string>().Should().Be("2027-05-01");
    }

    [Fact(DisplayName = "CT23 · PUT em item Encerrado → 422")]
    public async Task CT23_EditarEncerrado()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarDataUnicaAsync("2026-11-20");
        (await c.Post($"/api/v1/itens/{item["id"]}/concluir", new { dataConclusao = "2026-10-15" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var resp = await c.Put($"/api/v1/itens/{item["id"]}", new
        {
            nome = "CNH", categoriaId = 2, tipoVencimento = "DataUnica", dataVencimento = "2031-11-20", antecedenciaDias = 45
        });

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ApiCliente.Json(resp))["code"]!.GetValue<string>().Should().Be("ITEM_ENCERRADO");
    }

    [Fact(DisplayName = "CT24 · DELETE de item com passos e histórico → 204; passos, histórico e avisos removidos")]
    public async Task CT24_ExcluirItem()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180, passos: ["Comprar refil"]);
        await c.Post($"/api/v1/itens/{item["id"]}/concluir", new { dataConclusao = "2026-10-15", confirmarPassosPendentes = true });

        (await c.Delete($"/api/v1/itens/{item["id"]}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await c.Get($"/api/v1/itens/{item["id"]}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ApiCliente.Json(await c.Get("/api/v1/historico"))).AsArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "CT25 · Concluir recorrente (data de hoje, valor 280) → 200; histórico; novo vencimento; mensagem positiva")]
    public async Task CT25_ConcluirRecorrente()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180);

        var resp = await c.Post($"/api/v1/itens/{item["id"]}/concluir", new { dataConclusao = "2026-10-15", valorGasto = 280.00 });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ApiCliente.Json(resp);
        json["item"]!["proximoVencimentoData"]!.GetValue<string>().Should().Be("2027-04-13");
        json["mensagem"]!.GetValue<string>().Should().Be("Boa! Próximo prazo: 13/04/2027.");
        json["historicoId"]!.GetValue<string>().Should().NotBeNullOrEmpty();

        var historico = (await ApiCliente.Json(await c.Get("/api/v1/historico"))).AsArray();
        historico.Should().ContainSingle();
        historico[0]!["valorGasto"]!.GetValue<decimal>().Should().Be(280m);
        historico[0]!["itemNome"]!.GetValue<string>().Should().Be("Filtro");
    }

    [Fact(DisplayName = "CT26 · Concluir com 1 passo sem marcar, sem confirmação → 422 PASSOS_PENDENTES")]
    public async Task CT26_PassosPendentes()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180, passos: ["Comprar refil"]);

        var resp = await c.Post($"/api/v1/itens/{item["id"]}/concluir", new { dataConclusao = "2026-10-15" });

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var json = await ApiCliente.Json(resp);
        json["code"]!.GetValue<string>().Should().Be("PASSOS_PENDENTES");
        json["detail"]!.GetValue<string>().Should().Be("Este item ainda tem 1 passo sem marcar. Quer concluir mesmo assim?");
    }

    [Fact(DisplayName = "CT27 · Mesma situação com confirmarPassosPendentes = true → 200")]
    public async Task CT27_PassosConfirmados()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180, passos: ["Comprar refil"]);

        var resp = await c.Post($"/api/v1/itens/{item["id"]}/concluir",
            new { dataConclusao = "2026-10-15", confirmarPassosPendentes = true });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "CT28 · dataConclusao = amanhã → 400")]
    public async Task CT28_ConclusaoNoFuturo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180);

        var resp = await c.Post($"/api/v1/itens/{item["id"]}/concluir", new { dataConclusao = "2026-10-16" });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(resp))["errors"]!["dataConclusao"]![0]!.GetValue<string>().Should().Be("A data não pode estar no futuro.");
    }

    [Fact(DisplayName = "CT29 · Marcar passo (PATCH concluido = true) → 200; progresso atualizado")]
    public async Task CT29_MarcarPasso()
    {
        var c = await ApiCliente.CriarAsync(f);
        var item = await c.CriarRecorrenteAsync("2026-04-15", 180, passos: ["Comprar refil", "Trocar", "Avisar"]);
        var passoId = item["passos"]![0]!["id"]!.GetValue<string>();

        var resp = await c.Patch($"/api/v1/itens/{item["id"]}/passos/{passoId}", new { concluido = true });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ApiCliente.Json(resp);
        json["passoId"]!.GetValue<string>().Should().Be(passoId);
        json["concluido"]!.GetValue<bool>().Should().BeTrue();
        json["progressoPassos"]!.GetValue<double>().Should().Be(0.33);
    }

    [Fact(DisplayName = "CT30 · passoId que não pertence ao item → 404")]
    public async Task CT30_PassoDeOutroItem()
    {
        var c = await ApiCliente.CriarAsync(f);
        var a = await c.CriarRecorrenteAsync("2026-04-15", 180, nome: "A", passos: ["Passo A"]);
        var b = await c.CriarRecorrenteAsync("2026-04-15", 180, nome: "B", passos: ["Passo B"]);

        var resp = await c.Patch($"/api/v1/itens/{a["id"]}/passos/{b["passos"]![0]!["id"]}", new { concluido = true });

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "E13 · Filtros por categoria e status; filtro inválido → 400")]
    public async Task E13_Filtros()
    {
        var c = await ApiCliente.CriarAsync(f);
        await c.CriarRecorrenteAsync("2026-04-15", 180);   // Casa, vencido
        await c.CriarDataUnicaAsync("2027-03-20");         // Documentos, em dia

        (await ApiCliente.Json(await c.Get("/api/v1/itens?status=Vencido"))).AsArray().Should().ContainSingle();
        (await ApiCliente.Json(await c.Get("/api/v1/itens?categoriaId=2"))).AsArray().Should().ContainSingle();
        (await c.Get("/api/v1/itens?status=Atrasado")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "E25 · Histórico: de > ate → 400; itemId de outro usuário → 404")]
    public async Task E25_Historico()
    {
        var c = await ApiCliente.CriarAsync(f);
        var outro = await ApiCliente.CriarAsync(f);
        var alheio = await outro.CriarDataUnicaAsync("2027-01-01");

        (await c.Get("/api/v1/historico?de=2026-10-10&ate=2026-10-01")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.Get($"/api/v1/historico?itemId={alheio["id"]}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
