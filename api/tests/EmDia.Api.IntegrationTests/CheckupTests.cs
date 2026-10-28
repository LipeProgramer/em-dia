using System.Net;
using EmDia.Api.IntegrationTests.Fixtures;

namespace EmDia.Api.IntegrationTests;

/// <summary>CT13–CT17 · Check-up (E08–E11 · RN10, RN11).</summary>
[Collection("Integration")]
public class CheckupTests(IntegrationTestFactory f)
{
    private const string TrocaOleo = "a7000000-0000-0000-0000-000000000001";
    private const string Ipva = "a7000000-0000-0000-0000-000000000002";
    private const string Cnh = "a7000000-0000-0000-0000-000000000003";
    private const string VacinaPet = "a7000000-0000-0000-0000-000000000005";

    private static object CheckupCompleto() => new
    {
        respostas = new[] { new { codigo = "TEM_CARRO", resposta = true }, new { codigo = "TEM_CNH", resposta = true } },
        veiculos = new[] { new { @ref = "v1", apelido = "Gol", tipo = "Carro", kmAtual = 84_500, mediaKmMes = 2_500 } },
        itens = new object[]
        {
            new { modeloId = TrocaOleo, veiculoRef = "v1", dataBase = "2026-05-01", kmBase = 75_000 },
            new { modeloId = Ipva, veiculoRef = "v1", dataBase = "2025-10-10" },
            new { modeloId = Cnh, dataVencimento = "2027-03-20" }
        }
    };

    [Fact(DisplayName = "E08/E09/E10 · Categorias, modelos e perguntas do check-up")]
    public async Task Catalogo()
    {
        var c = await ApiCliente.CriarAsync(f);

        var categorias = (await ApiCliente.Json(await c.Get("/api/v1/categorias"))).AsArray();
        categorias.Should().HaveCount(6);
        categorias[0]!["nome"]!.GetValue<string>().Should().Be("Veículo");

        var modelos = (await ApiCliente.Json(await c.Get("/api/v1/modelos?categoriaId=1"))).AsArray();
        modelos.Should().Contain(m => m!["nome"]!.GetValue<string>() == "Troca de óleo"
                                      && m["tipoVencimento"]!.GetValue<string>() == "UsoOuTempo");
        (await c.Get("/api/v1/modelos?categoriaId=9")).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var perguntas = (await ApiCliente.Json(await c.Get("/api/v1/checkup/perguntas"))).AsArray();
        perguntas.Should().HaveCount(17);
        perguntas[0]!["codigo"]!.GetValue<string>().Should().Be("TEM_CARRO");
        perguntas.Select(p => p!["categoriaId"]!.GetValue<int>()).Should().BeInAscendingOrder("as perguntas vêm agrupadas por área");
        perguntas.Select(p => p!["categoriaId"]!.GetValue<int>()).Distinct().Should().HaveCount(6, "todas as áreas têm perguntas");
    }

    [Fact(DisplayName = "CT13 · Check-up válido (TEM_CARRO e TEM_CNH) → 201; itens criados; resumo por status e área")]
    public async Task CT13_CheckupValido()
    {
        var c = await ApiCliente.CriarAsync(f);

        var resp = await c.Post("/api/v1/checkup", CheckupCompleto());

        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        var json = await ApiCliente.Json(resp);
        json["itensCriados"]!.GetValue<int>().Should().Be(3);
        // Troca de óleo: faltam 500 km → Atenção; IPVA venceu em 10/10/2026 → Vencido; CNH → Em dia
        json["resumo"]!["vencidos"]!.GetValue<int>().Should().Be(1);
        json["resumo"]!["atencao"]!.GetValue<int>().Should().Be(1);
        json["resumo"]!["emDia"]!.GetValue<int>().Should().Be(1);
        json["areas"]!.AsArray().Should().HaveCount(6);
        json["areasEmDia"]!.GetValue<int>().Should().Be(1, "só Documentos tem itens e todos em dia");
        json["areasSemItens"]!.GetValue<int>().Should().Be(4);
        json["destaques"]![0]!["nome"]!.GetValue<string>().Should().Be("IPVA");
        json["destaques"]![0]!["textoPrazo"]!.GetValue<string>().Should().Be("venceu há 5 dias");

        var me = await ApiCliente.Json(await c.Get("/api/v1/usuarios/me"));
        me["precisaCheckup"]!.GetValue<bool>().Should().BeFalse();

        var itens = (await ApiCliente.Json(await c.Get("/api/v1/itens"))).AsArray();
        var oleo = itens.Single(i => i!["nome"]!.GetValue<string>() == "Troca de óleo")!;
        var detalhe = await ApiCliente.Json(await c.Get($"/api/v1/itens/{oleo["id"]}"));
        detalhe["passos"]!.AsArray().Should().HaveCount(2, "RN14: os passos do modelo são copiados");
        detalhe["textoPrazo"]!.GetValue<string>().Should().Be("faltam 500 km");
    }

    [Fact(DisplayName = "CT14 · Item de TEM_PET com TEM_PET = não → 400")]
    public async Task CT14_ItemSemRespostaSim()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/checkup", new
        {
            respostas = new[] { new { codigo = "TEM_PET", resposta = false } },
            itens = new[] { new { modeloId = VacinaPet, dataBase = "2026-01-10" } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(resp))["errors"]!["itens[0].modeloId"].Should().NotBeNull();
    }

    [Fact(DisplayName = "CT15 · Troca de óleo sem veiculoRef → 422")]
    public async Task CT15_SemVeiculo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/checkup", new
        {
            respostas = new[] { new { codigo = "TEM_CARRO", resposta = true } },
            itens = new[] { new { modeloId = TrocaOleo, dataBase = "2026-05-01", kmBase = 75_000 } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ApiCliente.Json(resp))["code"]!.GetValue<string>().Should().Be("VEICULO_OBRIGATORIO");
    }

    [Fact(DisplayName = "CT16 · dataBase = 01/01/2027 (futuro) → 400 com erro no campo")]
    public async Task CT16_DataNoFuturo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/checkup", new
        {
            respostas = new[] { new { codigo = "TEM_PET", resposta = true } },
            itens = new[] { new { modeloId = VacinaPet, dataBase = "2027-01-01" } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ApiCliente.Json(resp))["errors"]!["itens[0].dataBase"]![0]!.GetValue<string>()
            .Should().Be("A data não pode estar no futuro.");
    }

    [Fact(DisplayName = "RN11 · Pular tudo → nenhuma área em dia; todas sem itens")]
    public async Task PularTudo()
    {
        var c = await ApiCliente.CriarAsync(f);
        var resp = await c.Post("/api/v1/checkup", new { respostas = Array.Empty<object>() });

        var json = await ApiCliente.Json(resp);
        json["areasEmDia"]!.GetValue<int>().Should().Be(0);
        json["areasSemItens"]!.GetValue<int>().Should().Be(6);
        json["areas"]![0]!["status"].Should().BeNull();
    }

    [Fact(DisplayName = "CT17 · Mesmo check-up enviado duas vezes → a segunda vez não duplica itens (U15)")]
    public async Task CT17_RefazerCheckup()
    {
        var c = await ApiCliente.CriarAsync(f);
        (await c.Post("/api/v1/checkup", CheckupCompleto())).StatusCode.Should().Be(HttpStatusCode.Created);

        var segunda = await c.Post("/api/v1/checkup", CheckupCompleto());

        segunda.StatusCode.Should().Be(HttpStatusCode.Created);
        (await ApiCliente.Json(segunda))["itensCriados"]!.GetValue<int>().Should().Be(0);
        (await ApiCliente.Json(await c.Get("/api/v1/itens"))).AsArray().Should().HaveCount(3);
        (await ApiCliente.Json(await c.Get("/api/v1/veiculos"))).AsArray().Should().HaveCount(1);
    }
}
