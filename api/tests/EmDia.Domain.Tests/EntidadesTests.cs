using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EmDia.Domain.Tests;

/// <summary>Regras complementares das entidades (spec §2.3, §2.5 e §4), além de U01–U15.</summary>
public class EntidadesTests
{
    private static readonly DateOnly Hoje = new(2026, 10, 15);
    private static readonly DateTimeOffset Agora = new(2026, 10, 15, 15, 0, 0, TimeSpan.Zero);

    private static Item DataUnica(DateOnly vencimento) =>
        new(Guid.NewGuid(), 2, "CNH", TipoVencimento.DataUnica, null, vencimento, null, null, null, null, 45, null, null, null, Agora);

    // ── Textos de prazo (RN15: linguagem simples) ─────────────────────────────

    [Theory(DisplayName = "Texto do prazo por data")]
    [InlineData(0, "vence hoje")]
    [InlineData(1, "vence amanhã")]
    [InlineData(-1, "venceu ontem")]
    [InlineData(-5, "venceu há 5 dias")]
    [InlineData(12, "faltam 12 dias")]
    public void TextoPrazoPorData(int dias, string esperado)
    {
        DataUnica(Hoje.AddDays(dias)).TextoPrazo(Hoje, null).Should().Be(esperado);
    }

    [Fact(DisplayName = "Texto do prazo por km: passou e chegou na km")]
    public void TextoPrazoPorKm()
    {
        var veiculo = new Veiculo(Guid.NewGuid(), "Gol", TipoVeiculo.Carro, 85_100, 2_500, Agora);
        var item = new Item(veiculo.UsuarioId, 1, "Óleo", TipoVencimento.UsoOuTempo, new DateOnly(2026, 5, 1), null, 365,
            10_000, 75_000, null, 30, null, veiculo.Id, null, Agora);

        item.TextoPrazo(Hoje, veiculo).Should().Be("passou 100 km");
        veiculo.AtualizarKm(85_100, Agora);
        item.CalcularAnel(Hoje, veiculo).Should().Be(AnelRadar.Centro);
    }

    [Fact(DisplayName = "Item encerrado: texto 'concluído', anel centro e sem prazo")]
    public void ItemEncerrado()
    {
        var item = DataUnica(Hoje.AddDays(10));
        item.Concluir(Hoje, null, null, "  ", false, null, null, Agora, Hoje);

        item.TextoPrazo(Hoje, null).Should().Be("concluído");
        item.CalcularAnel(Hoje, null).Should().Be(AnelRadar.Centro);
        item.Historicos.Single().Observacao.Should().BeNull("observação em branco não é gravada");
        item.Historicos.Single().ProximoVencimento.Should().BeNull();
    }

    // ── Passos ───────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Passos: no máximo 10; substituir recria a lista; progresso com duas casas")]
    public void Passos()
    {
        var item = DataUnica(Hoje.AddDays(100));
        item.ProgressoPassos().Should().Be(0);

        item.SubstituirPassos(Enumerable.Range(1, 10).Select(i => $"Passo {i}"));
        var acao = () => item.AdicionarPasso("Passo 11");
        acao.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "passos");

        item.SubstituirPassos(["A", "B", "C"]);
        item.MarcarPasso(item.Passos[0].Id, true, Agora);
        item.Passos.Select(p => p.Ordem).Should().Equal((short)1, (short)2, (short)3);
        item.ProgressoPassos().Should().Be(0.33);

        item.MarcarPasso(item.Passos[0].Id, false, Agora).ConcluidoEm.Should().BeNull();
    }

    [Fact(DisplayName = "Marcar passo inexistente → NotFound; em item encerrado → ITEM_ENCERRADO")]
    public void MarcarPassoInvalido()
    {
        var item = DataUnica(Hoje.AddDays(100));
        item.AdicionarPasso("Agendar");

        var inexistente = () => item.MarcarPasso(Guid.NewGuid(), true, Agora);
        inexistente.Should().Throw<NotFoundException>().Where(e => e.Code == "PASSO_NAO_ENCONTRADO");

        item.Concluir(Hoje, null, null, null, true, null, null, Agora, Hoje);
        var encerrado = () => item.MarcarPasso(item.Passos[0].Id, true, Agora);
        encerrado.Should().Throw<DomainException>().Where(e => e.Code == CodigosErro.ItemEncerrado);
    }

    // ── Conclusão: validações ────────────────────────────────────────────────

    [Fact(DisplayName = "Concluir: data futura, km ausente/menor que a base e nova validade no passado são recusadas")]
    public void ConcluirValidacoes()
    {
        var veiculo = new Veiculo(Guid.NewGuid(), "Gol", TipoVeiculo.Carro, 80_000, 1_000, Agora);
        var oleo = new Item(veiculo.UsuarioId, 1, "Óleo", TipoVencimento.UsoOuTempo, new DateOnly(2026, 5, 1), null, 365,
            10_000, 75_000, null, 30, null, veiculo.Id, null, Agora);
        var cnh = DataUnica(Hoje.AddDays(10));

        ((Action)(() => oleo.Concluir(Hoje.AddDays(1), 81_000, null, null, false, null, veiculo, Agora, Hoje)))
            .Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "dataConclusao");
        ((Action)(() => oleo.Concluir(Hoje, null, null, null, false, null, veiculo, Agora, Hoje)))
            .Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "km");
        ((Action)(() => oleo.Concluir(Hoje, 70_000, null, null, false, null, veiculo, Agora, Hoje)))
            .Should().Throw<CampoInvalidoException>().WithMessage("A km não pode ser menor que 75.000.");
        ((Action)(() => cnh.Concluir(Hoje, null, null, null, false, Hoje, null, Agora, Hoje)))
            .Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "novaDataVencimento");
    }

    [Fact(DisplayName = "Concluir item já encerrado → ITEM_ENCERRADO")]
    public void ConcluirEncerrado()
    {
        var item = DataUnica(Hoje.AddDays(10));
        item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);

        var acao = () => item.Concluir(Hoje, null, null, null, false, null, null, Agora, Hoje);
        acao.Should().Throw<DomainException>().Where(e => e.Code == CodigosErro.ItemEncerrado);
    }

    [Fact(DisplayName = "Editar pode mudar o tipo e recalcula o vencimento")]
    public void EditarMudaTipo()
    {
        var item = DataUnica(Hoje.AddDays(10));

        item.Editar("Filtro", 3, TipoVencimento.RecorrentePorTempo, new DateOnly(2026, 10, 1), null, 30,
            null, null, null, 7, null, "obs", Agora);

        item.TipoVencimento.Should().Be(TipoVencimento.RecorrentePorTempo);
        item.ProximoVencimentoData.Should().Be(new DateOnly(2026, 10, 31));
        item.Observacao.Should().Be("obs");
    }

    [Theory(DisplayName = "§2.5 · Campos exigidos por tipo de vencimento")]
    [InlineData(TipoVencimento.DataUnica, "dataVencimento")]
    [InlineData(TipoVencimento.RecorrentePorTempo, "dataBase")]
    [InlineData(TipoVencimento.Calculado, "dataBase")]
    public void CamposPorTipo(TipoVencimento tipo, string campo)
    {
        var acao = () => Item.ValidarCamposPorTipo(tipo, null, null, null, null, null, null, null);
        acao.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == campo);
    }

    [Fact(DisplayName = "§2.5 · Uso ou tempo e garantia: cada campo exigido é verificado")]
    public void CamposUsoOuTempoEGarantia()
    {
        var v = Guid.NewGuid();
        Action semKm = () => Item.ValidarCamposPorTipo(TipoVencimento.UsoOuTempo, Hoje, null, 365, 10_000, null, null, v);
        Action semIntervaloKm = () => Item.ValidarCamposPorTipo(TipoVencimento.UsoOuTempo, Hoje, null, 365, null, 1, null, v);
        Action semIntervaloDias = () => Item.ValidarCamposPorTipo(TipoVencimento.UsoOuTempo, Hoje, null, null, 10_000, 1, null, v);
        Action semData = () => Item.ValidarCamposPorTipo(TipoVencimento.UsoOuTempo, null, null, 365, 10_000, 1, null, v);
        Action semGarantia = () => Item.ValidarCamposPorTipo(TipoVencimento.Calculado, Hoje, null, null, null, null, null, null);

        semKm.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "kmBase");
        semIntervaloKm.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "intervaloKm");
        semIntervaloDias.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "intervaloDias");
        semData.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "dataBase");
        semGarantia.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "garantiaFabricanteMeses");
    }

    // ── Veículo, conta e tokens ─────────────────────────────────────────────

    [Fact(DisplayName = "Veículo: km negativa e média fora de 1 a 20.000 são recusadas")]
    public void VeiculoValidacoes()
    {
        var kmNegativa = () => new Veiculo(Guid.NewGuid(), "Gol", TipoVeiculo.Carro, -1, 1_000, Agora);
        var mediaAlta = () => new Veiculo(Guid.NewGuid(), "Gol", TipoVeiculo.Carro, 0, 20_001, Agora);
        var veiculo = new Veiculo(Guid.NewGuid(), " Gol ", TipoVeiculo.Carro, 0, 1_000, Agora);
        veiculo.Editar("Moto azul", TipoVeiculo.Moto, 500);

        kmNegativa.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "kmAtual");
        mediaAlta.Should().Throw<CampoInvalidoException>().Where(e => e.Campo == "mediaKmMes");
        veiculo.Apelido.Should().Be("Moto azul");
        veiculo.Tipo.Should().Be(TipoVeiculo.Moto);
    }

    [Fact(DisplayName = "Conta: preferências padrão (spec §2.3) e faixas válidas")]
    public void Preferencias()
    {
        var usuario = new Usuario(" Rodrigo ", "R@E.com", "hash", Agora);
        var p = usuario.Preferencias!;

        usuario.Nome.Should().Be("Rodrigo");
        (p.ReceberEmail, p.MaxReavisos, p.IntervaloReavisoDias, p.ModoFocoPadrao).Should().Be((true, (short)2, (short)3, false));

        p.Atualizar(false, 5, 7, true, Agora);
        p.MaxReavisos.Should().Be(5);
        ((Action)(() => p.Atualizar(true, 6, 3, false, Agora))).Should().Throw<CampoInvalidoException>();
        ((Action)(() => p.Atualizar(true, 2, 0, false, Agora))).Should().Throw<CampoInvalidoException>();
    }

    [Fact(DisplayName = "Conta: precisaCheckup até o primeiro check-up; login certo zera as falhas")]
    public void UsuarioCheckupELogin()
    {
        var usuario = new Usuario("Rodrigo", "r@e.com", "hash", Agora);
        usuario.PrecisaCheckup.Should().BeTrue();
        usuario.RegistrarCheckup(Agora);
        usuario.PrecisaCheckup.Should().BeFalse();

        usuario.RegistrarFalhaLogin(Agora, 5, 15);
        usuario.FalhasLogin.Should().Be(1);
        usuario.RegistrarLoginComSucesso();
        usuario.FalhasLogin.Should().Be(0);
        usuario.BloqueadoAte.Should().BeNull();
    }

    [Fact(DisplayName = "Refresh token: válido por 7 dias; revogar uma vez mantém a primeira data")]
    public void RefreshToken()
    {
        var rt = new RefreshToken(Guid.NewGuid(), "hash", Agora, 7);

        rt.EstaAtivo(Agora.AddDays(6)).Should().BeTrue();
        rt.EstaAtivo(Agora.AddDays(7)).Should().BeFalse();
        rt.Revogar(Agora.AddHours(1));
        rt.Revogar(Agora.AddHours(2));
        rt.Revogado.Should().BeTrue();
        rt.RevogadoEm.Should().Be(Agora.AddHours(1));
        rt.EstaAtivo(Agora).Should().BeFalse();
    }

    [Fact(DisplayName = "Check-up guarda respostas e itens criados; catálogo expõe seus dados")]
    public void CheckupECatalogo()
    {
        var checkup = new Checkup(Guid.NewGuid(), new() { ["TEM_CARRO"] = true }, 3, Agora);
        var categoria = new Categoria(1, "Veículo", "car", 1);
        var modelo = new ModeloItem(Guid.NewGuid(), 1, "Troca de óleo", TipoVencimento.UsoOuTempo, 365, 10_000, null, 30, "TEM_CARRO");
        var passo = new PassoModelo(modelo.Id, 1, "Agendar oficina");

        checkup.Respostas.Should().Contain("TEM_CARRO");
        checkup.ItensCriados.Should().Be(3);
        categoria.Nome.Should().Be("Veículo");
        modelo.ExigeVeiculo.Should().BeTrue();
        modelo.Ativo.Should().BeTrue();
        passo.ModeloItemId.Should().Be(modelo.Id);
    }

    [Fact(DisplayName = "Aviso registra o resultado do envio (erro limitado a 300 caracteres)")]
    public void Aviso()
    {
        var aviso = new Aviso(Guid.NewGuid(), TipoAviso.Antecipado, CanalAviso.Email, Hoje, 0, Agora, false);

        aviso.RegistrarResultado(false, new string('x', 400), Agora);
        aviso.Erro.Should().HaveLength(300);
        aviso.RegistrarResultado(true, null, Agora);
        aviso.Sucesso.Should().BeTrue();
    }

    [Fact(DisplayName = "Exceções carregam o código usado no ProblemDetails")]
    public void Excecoes()
    {
        new ConflictException("X", "m").Code.Should().Be("X");
        new NotFoundException("Y", "m").Code.Should().Be("Y");
        new UnauthorizedException("Z", "m").Code.Should().Be("Z");
    }
}
