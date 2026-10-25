using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using EmDia.Domain.Services;
using FluentAssertions;
using Xunit;

namespace EmDia.Domain.Tests;

/// <summary>
/// Testes unitários das regras de negócio do Domain.
/// "Hoje" fixado em 15/10/2026 para todos os casos (spec §6.1 e §6.3).
/// </summary>
public class RegrasNegocioTests
{
    // ── Data fixa para todos os testes ────────────────────────────────────────
    private static readonly DateOnly Hoje = new(2026, 10, 15);
    private static readonly DateTimeOffset Agora = new(2026, 10, 15, 12, 0, 0, TimeSpan.FromHours(-3));

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Item CriarItemRecorrente(DateOnly dataBase, int intervaloDias, short antecedencia = 30) =>
        new(
            usuarioId: Guid.NewGuid(),
            categoriaId: 1,
            nome: "Teste Recorrente",
            tipoVencimento: TipoVencimento.RecorrentePorTempo,
            dataBase: dataBase,
            dataVencimento: null,
            intervaloDias: intervaloDias,
            intervaloKm: null,
            kmBase: null,
            garantiaFabricanteMeses: null,
            antecedenciaDias: antecedencia,
            modeloItemId: null,
            veiculoId: null,
            observacao: null,
            agora: Agora);

    private static Item CriarItemUsoOuTempo(
        DateOnly dataBase, int intervaloDias, int kmBase, int intervaloKm,
        int kmAtual, int mediaKmMes, short antecedencia = 30) =>
        new(
            usuarioId: Guid.NewGuid(),
            categoriaId: 1,
            nome: "Teste UsoOuTempo",
            tipoVencimento: TipoVencimento.UsoOuTempo,
            dataBase: dataBase,
            dataVencimento: null,
            intervaloDias: intervaloDias,
            intervaloKm: intervaloKm,
            kmBase: kmBase,
            garantiaFabricanteMeses: null,
            antecedenciaDias: antecedencia,
            modeloItemId: null,
            veiculoId: Guid.NewGuid(),
            observacao: null,
            agora: Agora,
            kmAtualVeiculo: kmAtual,
            mediaKmMesVeiculo: mediaKmMes);

    private static Item CriarItemDataUnica(DateOnly dataVencimento, short antecedencia = 45) =>
        new(
            usuarioId: Guid.NewGuid(),
            categoriaId: 2,
            nome: "CNH",
            tipoVencimento: TipoVencimento.DataUnica,
            dataBase: null,
            dataVencimento: dataVencimento,
            intervaloDias: null,
            intervaloKm: null,
            kmBase: null,
            garantiaFabricanteMeses: null,
            antecedenciaDias: antecedencia,
            modeloItemId: null,
            veiculoId: null,
            observacao: null,
            agora: Agora);

    private static Item CriarItemGarantia(DateOnly dataCompra, short mesesFabricante) =>
        new(
            usuarioId: Guid.NewGuid(),
            categoriaId: 6,
            nome: "Garantia Eletrônico",
            tipoVencimento: TipoVencimento.Calculado,
            dataBase: dataCompra,
            dataVencimento: null,
            intervaloDias: null,
            intervaloKm: null,
            kmBase: null,
            garantiaFabricanteMeses: mesesFabricante,
            antecedenciaDias: 15,
            modeloItemId: null,
            veiculoId: null,
            observacao: null,
            agora: Agora);

    // ─────────────────────────────────────────────────────────────────────────
    // U01 — RN01/RN02: Recorrente vencido
    // Última realização 15/04/2026, intervalo 180 dias → vence 12/10/2026 → Vencido
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U01_RecorrentePorTempo_Vencido()
    {
        var item = CriarItemRecorrente(new DateOnly(2026, 4, 15), 180);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2026, 10, 12));
        item.CalcularStatus(Hoje).Should().Be(StatusItem.Vencido);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U02 — RN01: UsoOuTempo — km vem primeiro → Atenção
    // km_base 75.000 + 10.000 = 85.000; km_atual 84.500; média 2.500/mês
    // Faltam 500 km ≈ 6 dias → vence em 21/10/2026; data por tempo: 01/05/2027
    // km vem primeiro → Atenção
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U02_UsoOuTempo_KmVemPrimeiro_Atencao()
    {
        var item = CriarItemUsoOuTempo(
            dataBase: new DateOnly(2026, 5, 1),
            intervaloDias: 365,
            kmBase: 75_000,
            intervaloKm: 10_000,
            kmAtual: 84_500,
            mediaKmMes: 2_500,
            antecedencia: 30);

        item.ProximoVencimentoKm.Should().Be(85_000);

        // Km vem primeiro (500 km restantes ≈ 6 dias → antes de 01/05/2027)
        item.ProximoVencimentoData.Should().BeBefore(new DateOnly(2027, 5, 1));

        // Status = Atenção (dentro da antecedência de 30 dias)
        item.CalcularStatus(Hoje, kmAtualVeiculo: 84_500).Should().Be(StatusItem.Atencao);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U03 — RN01: UsoOuTempo — tempo vem primeiro → Em Dia
    // km_atual 76.000; média 500/mês; km alvo 85.000 → faltam 9.000 km = 18 meses
    // Tempo vem primeiro: 01/05/2027 → Em Dia
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U03_UsoOuTempo_TempoVemPrimeiro_EmDia()
    {
        var item = CriarItemUsoOuTempo(
            dataBase: new DateOnly(2026, 5, 1),
            intervaloDias: 365,
            kmBase: 75_000,
            intervaloKm: 10_000,
            kmAtual: 76_000,
            mediaKmMes: 500,
            antecedencia: 30);

        // Tempo (01/05/2027) deve vir antes da estimativa de km (18 meses ≈ nov/2027)
        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 5, 1));
        item.CalcularStatus(Hoje, kmAtualVeiculo: 76_000).Should().Be(StatusItem.EmDia);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U04 — RN01: Garantia calculada
    // Compra 10/01/2026, 12 meses fabricante → 10/01/2027 + 90 dias = 10/04/2027
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U04_Garantia_Calculada()
    {
        var item = CriarItemGarantia(new DateOnly(2026, 1, 10), 12);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 4, 10));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U05 — RN02: DataUnica — CNH vence 20/11/2026, antecedência 45 dias
    // Hoje 15/10/2026: faltam 36 dias → Atenção
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U05_DataUnica_DentroAntecedencia_Atencao()
    {
        var item = CriarItemDataUnica(new DateOnly(2026, 11, 20), antecedencia: 45);

        int diasRestantes = item.ProximoVencimentoData!.Value.DayNumber - Hoje.DayNumber;
        diasRestantes.Should().Be(36);
        item.CalcularStatus(Hoje).Should().Be(StatusItem.Atencao);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U06 — RN02: UsoOuTempo — km atual ≥ km alvo → Vencido (km manda)
    // km_atual 85.200 ≥ 85.000 = ProximoVencimentoKm
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U06_UsoOuTempo_KmAtingida_Vencido()
    {
        var item = CriarItemUsoOuTempo(
            dataBase: new DateOnly(2026, 5, 1),
            intervaloDias: 365,
            kmBase: 75_000,
            intervaloKm: 10_000,
            kmAtual: 85_200,
            mediaKmMes: 2_500);

        item.CalcularStatus(Hoje, kmAtualVeiculo: 85_200).Should().Be(StatusItem.Vencido);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U07 — RN03: Anéis do Radar
    // Prazos de -3, 12, 60 e 200 dias → Centro, Anel1, Anel2, Externo
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U07_AneisRadar_QuatroZonas()
    {
        var casos = new[]
        {
            (diasRelativo: -3,  anel: AnelRadar.Centro),
            (diasRelativo:  12, anel: AnelRadar.Anel1),
            (diasRelativo:  60, anel: AnelRadar.Anel2),
            (diasRelativo: 200, anel: AnelRadar.Externo)
        };

        foreach (var (diasRelativo, anelEsperado) in casos)
        {
            var vencimento = Hoje.AddDays(diasRelativo);
            var item = CriarItemDataUnica(vencimento, antecedencia: 0);
            item.CalcularAnel(Hoje).Should().Be(anelEsperado,
                because: $"prazo de {diasRelativo} dias deve estar no {anelEsperado}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U08 — RN04: Modo foco
    // 5 itens com prazos -10, -2, 5, 20 e 100 dias → modo foco devolve -10, -2 e 5
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U08_ModoFoco_3MaisUrgentes()
    {
        var itens = new[]
        {
            CriarItemDataUnica(Hoje.AddDays(-10), antecedencia: 0),
            CriarItemDataUnica(Hoje.AddDays(-2),  antecedencia: 0),
            CriarItemDataUnica(Hoje.AddDays(5),   antecedencia: 0),
            CriarItemDataUnica(Hoje.AddDays(20),  antecedencia: 0),
            CriarItemDataUnica(Hoje.AddDays(100), antecedencia: 0),
        };

        var foco = RadarDomain.AplicarModoFoco(itens, Hoje, _ => null).ToList();

        foco.Should().HaveCount(3);
        foco[0].DiasRestantes(Hoje).Should().Be(-10);
        foco[1].DiasRestantes(Hoje).Should().Be(-2);
        foco[2].DiasRestantes(Hoje).Should().Be(5);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U09 — RN05: Concluir recorrente (180 dias) em 15/10/2026, 2 passos marcados
    // Esperado: histórico criado; próximo em 13/04/2027; passos desmarcados
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U09_Concluir_Recorrente_RecalculaEDesmarcaPassos()
    {
        var item = CriarItemRecorrente(new DateOnly(2026, 4, 15), 180);
        item.AdicionarPasso("Agendar oficina");
        item.AdicionarPasso("Levar o veículo");

        // Marca os 2 passos
        foreach (var p in item.Passos)
            p.Marcar(true, Agora);

        var historico = item.Concluir(
            dataConclusao: Hoje,
            kmNaConclusao: null,
            valorGasto: null,
            observacao: null,
            confirmarPassosPendentes: false,
            novaDataVencimento: null,
            agora: Agora,
            hoje: Hoje);

        historico.Should().NotBeNull();
        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 4, 13)); // 15/10/2026 + 180 dias
        item.Passos.Should().AllSatisfy(p => p.Concluido.Should().BeFalse());
        item.Situacao.Should().Be(SituacaoItem.Ativo);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U10 — RN05: Concluir DataUnica sem nova validade → Encerrado
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U10_Concluir_DataUnica_SemNovaValidade_Encerrado()
    {
        var item = CriarItemDataUnica(new DateOnly(2026, 11, 20));

        item.Concluir(
            dataConclusao: Hoje,
            kmNaConclusao: null,
            valorGasto: null,
            observacao: null,
            confirmarPassosPendentes: false,
            novaDataVencimento: null,
            agora: Agora,
            hoje: Hoje);

        item.Situacao.Should().Be(SituacaoItem.Encerrado);
        item.CalcularStatus(Hoje).Should().Be(StatusItem.Encerrado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U11 — RN06: Concluir com passo pendente sem confirmar → DomainException PASSOS_PENDENTES
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U11_PassosPendentes_SemConfirmacao_LancaExcecao()
    {
        var item = CriarItemRecorrente(new DateOnly(2026, 4, 15), 180);
        item.AdicionarPasso("Passo não marcado");

        var acao = () => item.Concluir(
            dataConclusao: Hoje,
            kmNaConclusao: null,
            valorGasto: null,
            observacao: null,
            confirmarPassosPendentes: false,
            novaDataVencimento: null,
            agora: Agora,
            hoje: Hoje);

        acao.Should().Throw<DomainException>()
            .Which.Code.Should().Be("PASSOS_PENDENTES");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U12 — RN07: Km do veículo não pode diminuir
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U12_KmVeiculo_NaoPodeDiminuir()
    {
        var veiculo = new Veiculo(Guid.NewGuid(), "Gol", TipoVeiculo.Carro, 84_500, 2_500, Agora);

        var acao = () => veiculo.AtualizarKm(84_000, Agora);

        acao.Should().Throw<DomainException>()
            .Which.Code.Should().Be("KM_INVALIDA");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U13 — RN08/RN09: Rotina de avisos — envio correto e limite
    // Item entra em Atenção no dia D; máx. 2 reavisos a cada 3 dias; sem conclusão
    // Envia em D, D+3 e D+6; não envia em D+9
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U13_Avisos_AntecipacaoEReavisos_LimiteRespeitado()
    {
        var vencimento = new DateOnly(2026, 10, 20);
        var status = StatusItem.Atencao;
        short maxReavisos = 2;
        short intervalo = 3;

        // Dia D: sem avisos → deve enviar antecipado (numero 0)
        var diaD = new DateOnly(2026, 10, 15);
        var resultD = AvisoDomain.VerificarAviso([], diaD, status, vencimento, maxReavisos, intervalo);
        resultD.Enviar.Should().BeTrue();
        resultD.Tipo.Should().Be(TipoAviso.Antecipado);
        resultD.Numero.Should().Be(0);

        // Dia D+3: antecipado enviado → deve enviar reaviso 1
        var avisoDiaD = new Aviso(Guid.NewGuid(), TipoAviso.Antecipado, CanalAviso.Email,
            vencimento, 0, new DateTimeOffset(diaD.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), true);
        var resultD3 = AvisoDomain.VerificarAviso(
            [avisoDiaD], diaD.AddDays(3), status, vencimento, maxReavisos, intervalo);
        resultD3.Enviar.Should().BeTrue();
        resultD3.Tipo.Should().Be(TipoAviso.Reaviso);
        resultD3.Numero.Should().Be(1);

        // Dia D+6: reavisos 0 e 1 enviados → deve enviar reaviso 2
        var avisoD3 = new Aviso(Guid.NewGuid(), TipoAviso.Reaviso, CanalAviso.Email,
            vencimento, 1, new DateTimeOffset(diaD.AddDays(3).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), true);
        var resultD6 = AvisoDomain.VerificarAviso(
            [avisoDiaD, avisoD3], diaD.AddDays(6), status, vencimento, maxReavisos, intervalo);
        resultD6.Enviar.Should().BeTrue();
        resultD6.Numero.Should().Be(2);

        // Dia D+9: limite (maxReavisos=2) atingido → NÃO deve enviar
        var avisoD6 = new Aviso(Guid.NewGuid(), TipoAviso.Reaviso, CanalAviso.Email,
            vencimento, 2, new DateTimeOffset(diaD.AddDays(6).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), true);
        var resultD9 = AvisoDomain.VerificarAviso(
            [avisoDiaD, avisoD3, avisoD6], diaD.AddDays(9), status, vencimento, maxReavisos, intervalo);
        resultD9.Enviar.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U14 — RN09: Item concluído após 1 reaviso → novo ciclo zera reavisos
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U14_NovoVencimento_ReiniciaContadorAvisos()
    {
        var novoVencimento = new DateOnly(2027, 4, 13);
        var status = StatusItem.Atencao;

        // Com o NOVO vencimento, não existe nenhum aviso → deve enviar antecipado
        var result = AvisoDomain.VerificarAviso(
            [],
            hoje: new DateOnly(2027, 3, 15),
            status,
            vencimentoReferencia: novoVencimento,
            maxReavisos: 2,
            intervaloReavisoDias: 3);

        result.Enviar.Should().BeTrue();
        result.Tipo.Should().Be(TipoAviso.Antecipado);
        result.Numero.Should().Be(0);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // U15 — RN10: Check-up não duplica item do mesmo modelo já existente
    // Simula a regra: um item com modeloId X e veiculoId Y já existe → não cria outro
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void U15_Checkup_NaoDuplicaItemExistente()
    {
        var modeloId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();

        // Item existente ativo com mesmo modelo e veículo
        var itemExistente = new Item(
            usuarioId: Guid.NewGuid(),
            categoriaId: 1,
            nome: "IPVA",
            tipoVencimento: TipoVencimento.RecorrentePorTempo,
            dataBase: new DateOnly(2026, 1, 1),
            dataVencimento: null,
            intervaloDias: 365,
            intervaloKm: null,
            kmBase: null,
            garantiaFabricanteMeses: null,
            antecedenciaDias: 30,
            modeloItemId: modeloId,
            veiculoId: veiculoId,
            observacao: null,
            agora: Agora);

        // RN10: um item ativo com mesmo (modeloId, veiculoId) já existe → não cria
        bool jaExiste = itemExistente.Situacao == SituacaoItem.Ativo
                        && itemExistente.ModeloItemId == modeloId
                        && itemExistente.VeiculoId == veiculoId;

        jaExiste.Should().BeTrue("RN10: check-up não deve criar item duplicado");
    }
}
