using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using EmDia.Domain.Services;
using FluentAssertions;
using Xunit;

namespace EmDia.Domain.Tests;

/// <summary>
/// Testes unitários das regras de negócio (spec §6.3, U01 a U15).
/// "Hoje" fixado em 15/10/2026 em todos os casos.
/// </summary>
public class RegrasNegocioTests
{
    private static readonly DateOnly Hoje = new(2026, 10, 15);
    private static readonly DateTimeOffset Agora = new(2026, 10, 15, 12, 0, 0, TimeSpan.FromHours(-3));
    private static readonly Guid UsuarioId = Guid.NewGuid();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Veiculo Veiculo(int kmAtual, int mediaKmMes) =>
        new(UsuarioId, "Gol", TipoVeiculo.Carro, kmAtual, mediaKmMes, Agora);

    private static Item Recorrente(DateOnly dataBase, int intervaloDias, short antecedencia = 30) =>
        new(UsuarioId, 3, "Filtro", TipoVencimento.RecorrentePorTempo, dataBase, null, intervaloDias,
            null, null, null, antecedencia, null, null, null, Agora);

    private static Item UsoOuTempo(Veiculo veiculo, DateOnly dataBase, int intervaloDias, int kmBase, int intervaloKm,
        short antecedencia = 30) =>
        new(UsuarioId, 1, "Troca de óleo", TipoVencimento.UsoOuTempo, dataBase, null, intervaloDias,
            intervaloKm, kmBase, null, antecedencia, null, veiculo.Id, null, Agora);

    private static Item DataUnica(DateOnly vencimento, short antecedencia = 45) =>
        new(UsuarioId, 2, "CNH", TipoVencimento.DataUnica, null, vencimento, null,
            null, null, null, antecedencia, null, null, null, Agora);

    private static Item Garantia(DateOnly compra, short meses) =>
        new(UsuarioId, 6, "Garantia da TV", TipoVencimento.Calculado, compra, null, null,
            null, null, meses, 15, null, null, null, Agora);

    private static Aviso AvisoEnviado(Item item, short numero, DateOnly dia) =>
        new(item.Id, numero == 0 ? TipoAviso.Antecipado : TipoAviso.Reaviso, CanalAviso.Email,
            item.ProximoVencimentoData!.Value, numero,
            new DateTimeOffset(dia.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero), sucesso: true);

    private static DateOnly DataEnvio(DateTimeOffset momento) => DateOnly.FromDateTime(momento.UtcDateTime);

    // ── U01 — RN01/RN02 ───────────────────────────────────────────────────────

    [Fact(DisplayName = "U01 · Recorrente: 15/04/2026 + 180 dias vence em 12/10/2026 → Vencido")]
    public void U01_Recorrente_Vencido()
    {
        var item = Recorrente(new DateOnly(2026, 4, 15), 180);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2026, 10, 12));
        item.CalcularStatus(Hoje, null).Should().Be(StatusItem.Vencido);
    }

    // ── U02 a U04 — RN01 ──────────────────────────────────────────────────────

    [Fact(DisplayName = "U02 · Uso ou tempo: faltam 500 km ≈ 6 dias → vence em 21/10/2026 (km vem primeiro) → Atenção")]
    public void U02_UsoOuTempo_KmVemPrimeiro()
    {
        var veiculo = Veiculo(84_500, 2_500);
        var item = UsoOuTempo(veiculo, new DateOnly(2026, 5, 1), 365, 75_000, 10_000);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 5, 1), "a data gravada é a do tempo");
        item.ProximoVencimentoKm.Should().Be(85_000);
        item.KmRestantes(veiculo).Should().Be(500);
        item.DataEfetiva(Hoje, veiculo).Should().Be(new DateOnly(2026, 10, 21));
        item.CalcularStatus(Hoje, veiculo).Should().Be(StatusItem.Atencao);
        item.TextoPrazo(Hoje, veiculo).Should().Be("faltam 500 km");
    }

    [Fact(DisplayName = "U03 · Uso ou tempo: tempo vem primeiro → 01/05/2027 → Em dia")]
    public void U03_UsoOuTempo_TempoVemPrimeiro()
    {
        var veiculo = Veiculo(76_000, 500);
        var item = UsoOuTempo(veiculo, new DateOnly(2026, 5, 1), 365, 75_000, 10_000);

        item.DataEfetiva(Hoje, veiculo).Should().Be(new DateOnly(2027, 5, 1));
        item.CalcularStatus(Hoje, veiculo).Should().Be(StatusItem.EmDia);
    }

    [Fact(DisplayName = "U04 · Garantia: compra 10/01/2026 + 12 meses + 90 dias = 10/04/2027")]
    public void U04_Garantia()
    {
        var item = Garantia(new DateOnly(2026, 1, 10), 12);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 4, 10));
    }

    // ── U05/U06 — RN02 ────────────────────────────────────────────────────────

    [Fact(DisplayName = "U05 · Data única: CNH vence 20/11/2026, antecedência 45 → faltam 36 dias → Atenção")]
    public void U05_DataUnica_Atencao()
    {
        var item = DataUnica(new DateOnly(2026, 11, 20), 45);

        item.DiasRestantes(Hoje, null).Should().Be(36);
        item.CalcularStatus(Hoje, null).Should().Be(StatusItem.Atencao);
    }

    [Fact(DisplayName = "U06 · Uso ou tempo com km atual 85.200 ≥ 85.000 e data futura → Vencido (a km manda)")]
    public void U06_UsoOuTempo_KmAtingida()
    {
        var veiculo = Veiculo(85_200, 2_500);
        var item = UsoOuTempo(veiculo, new DateOnly(2026, 5, 1), 365, 75_000, 10_000);

        item.ProximoVencimentoData.Should().BeAfter(Hoje);
        item.CalcularStatus(Hoje, veiculo).Should().Be(StatusItem.Vencido);
    }

    // ── U07 — RN03 ────────────────────────────────────────────────────────────

    [Theory(DisplayName = "U07 · Anéis: −3, 12, 60 e 200 dias → Centro, Anel 1, Anel 2, Externo")]
    [InlineData(-3, AnelRadar.Centro)]
    [InlineData(12, AnelRadar.Anel1)]
    [InlineData(60, AnelRadar.Anel2)]
    [InlineData(200, AnelRadar.Externo)]
    public void U07_Aneis(int dias, AnelRadar esperado)
    {
        var item = DataUnica(Hoje.AddDays(dias), 30);

        item.CalcularAnel(Hoje, null).Should().Be(esperado);
    }

    // ── U08 — RN04 ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "U08 · Modo foco: prazos −10, −2, 5, 20 e 100 dias → devolve −10, −2 e 5")]
    public void U08_ModoFoco()
    {
        var itens = new[] { 100, 5, -2, 20, -10 }.Select(d => DataUnica(Hoje.AddDays(d), 30)).ToList();

        var foco = RadarDomain.AplicarModoFoco(itens, Hoje, _ => null).ToList();

        foco.Select(i => i.DiasRestantes(Hoje, null)).Should().Equal(-10, -2, 5);
    }

    // ── U09 a U11 — RN05/RN06 ─────────────────────────────────────────────────

    [Fact(DisplayName = "U09 · Concluir recorrente (180 dias) em 15/10/2026 → histórico; próximo 13/04/2027; passos desmarcados")]
    public void U09_ConcluirRecorrente()
    {
        var item = Recorrente(new DateOnly(2026, 4, 15), 180);
        item.AdicionarPasso("Comprar refil");
        item.AdicionarPasso("Trocar");
        foreach (var p in item.Passos) item.MarcarPasso(p.Id, true, Agora);

        var resultado = item.Concluir(Hoje, null, 80m, null, false, null, null, Agora, Hoje);

        resultado.Historico.ConcluidoEm.Should().Be(Hoje);
        resultado.Historico.VencimentoAnterior.Should().Be(new DateOnly(2026, 10, 12));
        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 4, 13));
        item.Historicos.Should().ContainSingle();
        item.Passos.Should().OnlyContain(p => !p.Concluido);
        resultado.Mensagem.Should().StartWith("Boa!");
    }

    [Fact(DisplayName = "U10 · Concluir data única sem nova validade → item Encerrado")]
    public void U10_ConcluirDataUnica_Encerra()
    {
        var item = DataUnica(new DateOnly(2026, 11, 20));

        item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);

        item.Situacao.Should().Be(SituacaoItem.Encerrado);
        item.CalcularStatus(Hoje, null).Should().Be(StatusItem.Encerrado);
    }

    [Fact(DisplayName = "U11 · Concluir com 1 passo pendente, sem confirmação → DomainException PASSOS_PENDENTES")]
    public void U11_PassosPendentes()
    {
        var item = Recorrente(new DateOnly(2026, 4, 15), 180);
        item.AdicionarPasso("Comprar refil");

        var acao = () => item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);

        acao.Should().Throw<DomainException>()
            .Where(e => e.Code == CodigosErro.PassosPendentes)
            .WithMessage("Este item ainda tem 1 passo sem marcar. Quer concluir mesmo assim?");
    }

    // ── U12 — RN07 ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "U12 · Atualizar km de 84.500 para 84.000 → DomainException: km não pode diminuir")]
    public void U12_KmNaoDiminui()
    {
        var veiculo = Veiculo(84_500, 2_500);

        var acao = () => veiculo.AtualizarKm(84_000, Agora);

        acao.Should().Throw<DomainException>()
            .Where(e => e.Code == CodigosErro.KmMenor)
            .WithMessage("A km não pode ser menor que 84.500.");
        veiculo.KmAtual.Should().Be(84_500);
    }

    // ── U13/U14 — RN08/RN09 ───────────────────────────────────────────────────

    [Fact(DisplayName = "U13 · Atenção no dia D; máx. 2 reavisos a cada 3 dias → envia em D, D+3 e D+6; não envia em D+9")]
    public void U13_AvisoEReavisos()
    {
        var item = DataUnica(Hoje.AddDays(20), 30); // entra em Atenção hoje (dia D)
        var avisos = new List<Aviso>();
        var diasComEnvio = new List<int>();

        for (int d = 0; d <= 12; d++)
        {
            var dia = Hoje.AddDays(d);
            var decisao = AvisoDomain.VerificarAviso(avisos, dia, item.CalcularStatus(dia, null),
                item.ProximoVencimentoData!.Value, maxReavisos: 2, intervaloReavisoDias: 3, DataEnvio);
            if (!decisao.Enviar) continue;
            avisos.Add(AvisoEnviado(item, decisao.Numero, dia));
            diasComEnvio.Add(d);
        }

        diasComEnvio.Should().Equal(0, 3, 6);
        avisos.Select(a => a.Numero).Should().Equal((short)0, (short)1, (short)2);
    }

    [Fact(DisplayName = "U14 · Item concluído após 1 reaviso → novo ciclo: contagem de reavisos volta a zero")]
    public void U14_NovoCicloAposConcluir()
    {
        var item = Recorrente(new DateOnly(2026, 4, 30), 180, antecedencia: 200); // vence 27/10/2026
        var avisos = new List<Aviso>
        {
            AvisoEnviado(item, 0, Hoje.AddDays(-3)),
            AvisoEnviado(item, 1, Hoje)
        };

        item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);
        var decisao = AvisoDomain.VerificarAviso(avisos, Hoje, item.CalcularStatus(Hoje, null),
            item.ProximoVencimentoData!.Value, 2, 3, DataEnvio);

        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 4, 13));
        decisao.Enviar.Should().BeTrue();
        decisao.Tipo.Should().Be(TipoAviso.Antecipado);
        decisao.Numero.Should().Be(0);
    }

    // ── U15 — RN10 ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "U15 · Check-up com modelo IPVA já existente e ativo para o mesmo veículo → não cria duplicado")]
    public void U15_CheckupNaoDuplica()
    {
        var modeloIpva = Guid.NewGuid();
        var veiculo = Veiculo(50_000, 1_000);
        var outroVeiculo = Veiculo(10_000, 1_000);
        var ipva = new Item(UsuarioId, 1, "IPVA", TipoVencimento.RecorrentePorTempo, new DateOnly(2026, 3, 1), null,
            365, null, null, null, 30, modeloIpva, veiculo.Id, null, Agora);

        CheckupDomain.ItemJaExiste([ipva], modeloIpva, veiculo.Id).Should().BeTrue();
        CheckupDomain.ItemJaExiste([ipva], modeloIpva, outroVeiculo.Id).Should().BeFalse();
    }

    [Fact(DisplayName = "RN11 · Pular todas as perguntas → nenhuma área em dia (todas sem itens)")]
    public void ResultadoCheckup_SemNada()
    {
        var r = RadarDomain.GerarResultado([], [1, 2, 3, 4, 5, 6], Hoje, _ => null);

        r.AreasEmDia.Should().Be(0);
        r.AreasSemItens.Should().Be(6);
    }

    // ── Regras complementares ─────────────────────────────────────────────────

    [Fact(DisplayName = "RN05 · Concluir uso ou tempo atualiza a km do veículo (RN07) e recalcula")]
    public void ConcluirUsoOuTempo_AtualizaVeiculo()
    {
        var veiculo = Veiculo(84_500, 2_500);
        var item = UsoOuTempo(veiculo, new DateOnly(2025, 11, 10), 365, 75_000, 10_000);

        var r = item.Concluir(Hoje, 85_200, 280m, "Oficina do João", false, null, veiculo, Agora, Hoje);

        veiculo.KmAtual.Should().Be(85_200);
        item.ProximoVencimentoKm.Should().Be(95_200);
        item.ProximoVencimentoData.Should().Be(new DateOnly(2027, 10, 15));
        r.Mensagem.Should().Be("Boa! Próxima vez: 95.200 km ou 15/10/2027, o que vier primeiro.");
    }

    [Fact(DisplayName = "RN05 · Concluir com km menor que a do veículo → KM_MENOR")]
    public void ConcluirUsoOuTempo_KmMenorQueVeiculo()
    {
        var veiculo = Veiculo(84_500, 2_500);
        var item = UsoOuTempo(veiculo, new DateOnly(2025, 11, 10), 365, 75_000, 10_000);

        var acao = () => item.Concluir(Hoje, 80_000, null, null, false, null, veiculo, Agora, Hoje);

        acao.Should().Throw<DomainException>().Where(e => e.Code == CodigosErro.KmMenor);
    }

    [Fact(DisplayName = "RN05 · Data única com nova validade continua ativa")]
    public void ConcluirDataUnica_ComNovaValidade()
    {
        var item = DataUnica(new DateOnly(2026, 11, 20));

        item.Concluir(Hoje, null, null, null, false, new DateOnly(2031, 11, 20), null, Agora, Hoje);

        item.Situacao.Should().Be(SituacaoItem.Ativo);
        item.ProximoVencimentoData.Should().Be(new DateOnly(2031, 11, 20));
    }

    [Fact(DisplayName = "RN02 · Item encerrado não pode ser editado")]
    public void ItemEncerrado_NaoEdita()
    {
        var item = Garantia(new DateOnly(2026, 1, 10), 12);
        item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);

        var acao = () => item.Editar("TV", 6, TipoVencimento.Calculado, new DateOnly(2026, 1, 10), null, null, null, null, 12, 15, null, null, Agora);

        acao.Should().Throw<DomainException>().Where(e => e.Code == CodigosErro.ItemEncerrado);
    }

    [Fact(DisplayName = "§2.5 · Uso ou tempo sem veículo → VEICULO_OBRIGATORIO")]
    public void UsoOuTempo_SemVeiculo()
    {
        var acao = () => new Item(UsuarioId, 1, "Troca de óleo", TipoVencimento.UsoOuTempo, Hoje, null, 365,
            10_000, 75_000, null, 30, null, null, null, Agora);

        acao.Should().Throw<DomainException>().Where(e => e.Code == CodigosErro.VeiculoObrigatorio);
    }

    [Fact(DisplayName = "§2.5 · Recorrente sem intervalo → erro no campo intervaloDias")]
    public void Recorrente_SemIntervalo()
    {
        var acao = () => new Item(UsuarioId, 3, "Filtro", TipoVencimento.RecorrentePorTempo, Hoje, null, null,
            null, null, null, 30, null, null, null, Agora);

        acao.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "intervaloDias");
    }

    [Fact(DisplayName = "§4.1 · 5 falhas seguidas bloqueiam a conta por 15 minutos")]
    public void BloqueioDeLogin()
    {
        var usuario = new Usuario("Rodrigo", "Rodrigo@Email.com", "hash", Agora);
        for (int i = 0; i < 5; i++) usuario.RegistrarFalhaLogin(Agora, 5, 15);

        usuario.Email.Should().Be("rodrigo@email.com");
        usuario.EstaBloqueado(Agora.AddMinutes(14)).Should().BeTrue();
        usuario.EstaBloqueado(Agora.AddMinutes(16)).Should().BeFalse();
    }

    [Fact(DisplayName = "RN11 · Resultado do check-up: área sem itens fica sem status e não conta como em dia")]
    public void ResultadoCheckup()
    {
        var itens = new[]
        {
            Recorrente(new DateOnly(2026, 4, 15), 180),   // Casa vencido
            DataUnica(new DateOnly(2026, 11, 20), 45),     // Documentos atenção
        };

        var r = RadarDomain.GerarResultado(itens, [1, 2, 3, 4, 5, 6], Hoje, _ => null);

        r.Vencidos.Should().Be(1);
        r.Atencao.Should().Be(1);
        r.AreasEmDia.Should().Be(0);
        r.AreasSemItens.Should().Be(4);
        r.Areas.Single(a => a.CategoriaId == 1).Status.Should().BeNull();
    }
}
