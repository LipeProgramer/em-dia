using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;

namespace EmDia.Domain.Entities;

/// <summary>
/// Item de vencimento acompanhado pelo usuário. Entidade central do sistema.
/// Contém o comportamento das regras RN01 a RN07 (spec §3.2).
/// O status nunca é gravado — é calculado na leitura via CalcularStatus().
/// </summary>
public class Item
{
    // ── Identidade ────────────────────────────────────────────────────────────
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }

    // ── Referências ───────────────────────────────────────────────────────────
    public short CategoriaId { get; private set; }
    public Guid? ModeloItemId { get; private set; }
    public Guid? VeiculoId { get; private set; }

    // ── Dados do item ────────────────────────────────────────────────────────
    public string Nome { get; private set; } = string.Empty;
    public TipoVencimento TipoVencimento { get; private set; }

    // Campos por tipo de vencimento
    public DateOnly? DataBase { get; private set; }
    public DateOnly? DataVencimento { get; private set; }
    public int? IntervaloDias { get; private set; }
    public int? IntervaloKm { get; private set; }
    public int? KmBase { get; private set; }
    public short? GarantiaFabricanteMeses { get; private set; }

    public short AntecedenciaDias { get; private set; }

    // ── Vencimento calculado e gravado ────────────────────────────────────────
    /// <summary>
    /// Próximo vencimento por data. Gravado e recalculado a cada alteração (spec §2.1).
    /// Para UsoOuTempo armazena o prazo por tempo; data estimada por km é calculada na leitura (RN01).
    /// </summary>
    public DateOnly? ProximoVencimentoData { get; private set; }

    /// <summary>Km alvo do próximo vencimento (apenas UsoOuTempo).</summary>
    public int? ProximoVencimentoKm { get; private set; }

    // ── Situação persistida ───────────────────────────────────────────────────
    public SituacaoItem Situacao { get; private set; }

    public string? Observacao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    // ── Navegação (EF Core) ───────────────────────────────────────────────────
    private readonly List<Passo> _passos = new();
    public IReadOnlyList<Passo> Passos => _passos.AsReadOnly();

    private readonly List<Historico> _historicos = new();
    public IReadOnlyList<Historico> Historicos => _historicos.AsReadOnly();

    // EF Core
    protected Item() { }

    // ── Construtor ────────────────────────────────────────────────────────────
    public Item(
        Guid usuarioId,
        short categoriaId,
        string nome,
        TipoVencimento tipoVencimento,
        DateOnly? dataBase,
        DateOnly? dataVencimento,
        int? intervaloDias,
        int? intervaloKm,
        int? kmBase,
        short? garantiaFabricanteMeses,
        short antecedenciaDias,
        Guid? modeloItemId,
        Guid? veiculoId,
        string? observacao,
        DateTimeOffset agora,
        int? kmAtualVeiculo = null,
        int? mediaKmMesVeiculo = null)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        CategoriaId = categoriaId;
        Nome = nome;
        TipoVencimento = tipoVencimento;
        DataBase = dataBase;
        DataVencimento = dataVencimento;
        IntervaloDias = intervaloDias;
        IntervaloKm = intervaloKm;
        KmBase = kmBase;
        GarantiaFabricanteMeses = garantiaFabricanteMeses;
        AntecedenciaDias = antecedenciaDias;
        ModeloItemId = modeloItemId;
        VeiculoId = veiculoId;
        Observacao = observacao;
        Situacao = SituacaoItem.Ativo;
        CriadoEm = agora;
        AtualizadoEm = agora;

        RecalcularProximoVencimento(kmAtualVeiculo, mediaKmMesVeiculo);
    }

    // ── RN01 — Cálculo do próximo vencimento ──────────────────────────────────

    /// <summary>
    /// RN01: recalcula e grava ProximoVencimentoData e ProximoVencimentoKm
    /// a cada criação ou alteração do item.
    /// Para UsoOuTempo, a data estimada pela km é calculada na leitura pelo Radar.
    /// </summary>
    public void RecalcularProximoVencimento(int? kmAtualVeiculo, int? mediaKmMesVeiculo)
    {
        switch (TipoVencimento)
        {
            case TipoVencimento.DataUnica:
                ProximoVencimentoData = DataVencimento;
                ProximoVencimentoKm = null;
                break;

            case TipoVencimento.RecorrentePorTempo:
                ProximoVencimentoData = DataBase!.Value.AddDays(IntervaloDias!.Value);
                ProximoVencimentoKm = null;
                break;

            case TipoVencimento.UsoOuTempo:
                // Km alvo
                ProximoVencimentoKm = KmBase!.Value + IntervaloKm!.Value;

                // Data pelo tempo
                var dataPorTempo = DataBase!.Value.AddDays(IntervaloDias!.Value);

                // Data estimada pela km (calcula apenas se tiver dados do veículo)
                DateOnly? dataPorKm = null;
                if (kmAtualVeiculo.HasValue && mediaKmMesVeiculo.HasValue && mediaKmMesVeiculo.Value > 0)
                {
                    int kmRestante = ProximoVencimentoKm.Value - kmAtualVeiculo.Value;
                    if (kmRestante > 0)
                    {
                        double mesesRestantes = (double)kmRestante / mediaKmMesVeiculo.Value;
                        int diasRestantes = (int)Math.Ceiling(mesesRestantes * 30);
                        // dataPorKm é a data estimada; não é gravada, mas usamos para escolher a menor
                        dataPorKm = DateOnly.FromDateTime(DateTime.Today.AddDays(diasRestantes));
                    }
                    else
                    {
                        // Km já atingida — vencimento passou
                        dataPorKm = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
                    }
                }

                // Grava a menor das duas datas como ProximoVencimentoData
                ProximoVencimentoData = dataPorKm.HasValue && dataPorKm.Value < dataPorTempo
                    ? dataPorKm.Value
                    : dataPorTempo;
                break;

            case TipoVencimento.Calculado:
                // Garantia: compra + meses fabricante + 90 dias legais
                ProximoVencimentoData = DataBase!.Value
                    .AddMonths(GarantiaFabricanteMeses!.Value)
                    .AddDays(90);
                ProximoVencimentoKm = null;
                break;
        }
    }

    // ── RN02 — Status calculado na leitura ────────────────────────────────────

    /// <summary>
    /// RN02: calcula o status a partir da data de hoje e da km atual do veículo.
    /// Nunca gravado no banco.
    /// </summary>
    public StatusItem CalcularStatus(DateOnly hoje, int? kmAtualVeiculo = null)
    {
        if (Situacao == SituacaoItem.Encerrado)
            return StatusItem.Encerrado;

        bool vencidoPorKm = TipoVencimento == TipoVencimento.UsoOuTempo
                            && kmAtualVeiculo.HasValue
                            && ProximoVencimentoKm.HasValue
                            && kmAtualVeiculo.Value >= ProximoVencimentoKm.Value;

        bool vencidoPorData = ProximoVencimentoData.HasValue && hoje >= ProximoVencimentoData.Value;

        if (vencidoPorKm || vencidoPorData)
            return StatusItem.Vencido;

        // Atenção: dentro da antecedência
        if (ProximoVencimentoData.HasValue)
        {
            int diasRestantes = ProximoVencimentoData.Value.DayNumber - hoje.DayNumber;
            if (diasRestantes <= AntecedenciaDias)
                return StatusItem.Atencao;
        }

        return StatusItem.EmDia;
    }

    // ── RN03 — Anel do Radar ──────────────────────────────────────────────────

    /// <summary>
    /// RN03: determina o anel do Radar com base no status e nos dias restantes.
    /// </summary>
    public AnelRadar CalcularAnel(DateOnly hoje, int? kmAtualVeiculo = null)
    {
        var status = CalcularStatus(hoje, kmAtualVeiculo);
        if (status == StatusItem.Vencido || status == StatusItem.Encerrado)
            return AnelRadar.Centro;

        if (!ProximoVencimentoData.HasValue)
            return AnelRadar.Externo;

        int dias = ProximoVencimentoData.Value.DayNumber - hoje.DayNumber;
        return dias switch
        {
            <= 30 => AnelRadar.Anel1,
            <= 90 => AnelRadar.Anel2,
            _ => AnelRadar.Externo
        };
    }

    /// <summary>
    /// Dias restantes para o próximo vencimento (negativo = vencido há X dias).
    /// </summary>
    public int DiasRestantes(DateOnly hoje) =>
        ProximoVencimentoData.HasValue
            ? ProximoVencimentoData.Value.DayNumber - hoje.DayNumber
            : int.MaxValue;

    // ── RN05 — Concluir ───────────────────────────────────────────────────────

    /// <summary>
    /// RN05 + RN06 + RN07: conclui o item, registra histórico e recalcula vencimento.
    /// </summary>
    /// <param name="dataConclusao">Data da conclusão (não pode ser futura).</param>
    /// <param name="kmNaConclusao">Obrigatório para UsoOuTempo.</param>
    /// <param name="valorGasto">Opcional.</param>
    /// <param name="observacao">Opcional, até 500 chars.</param>
    /// <param name="confirmarPassosPendentes">true = usuário confirmou passos pendentes.</param>
    /// <param name="novaDataVencimento">DataUnica: nova validade (opcional). Se omitida, encerra o item.</param>
    /// <param name="agora">Momento atual (IRelogio).</param>
    /// <param name="hoje">Data de hoje (IRelogio).</param>
    /// <param name="kmAtualVeiculoAntes">Km atual do veículo ANTES de atualizar.</param>
    /// <param name="mediaKmMesVeiculo">Média mensal do veículo.</param>
    /// <returns>Registro de histórico criado.</returns>
    public Historico Concluir(
        DateOnly dataConclusao,
        int? kmNaConclusao,
        decimal? valorGasto,
        string? observacao,
        bool confirmarPassosPendentes,
        DateOnly? novaDataVencimento,
        DateTimeOffset agora,
        DateOnly hoje,
        int? kmAtualVeiculoAntes = null,
        int? mediaKmMesVeiculo = null)
    {
        // Validação: item já encerrado
        if (Situacao == SituacaoItem.Encerrado)
            throw new DomainException("ITEM_ENCERRADO", "Este item já foi encerrado.");

        // RN06 — passos pendentes
        bool temPassosPendentes = _passos.Any(p => !p.Concluido);
        if (temPassosPendentes && !confirmarPassosPendentes)
            throw new DomainException(
                "PASSOS_PENDENTES",
                $"Este item ainda tem {_passos.Count(p => !p.Concluido)} passo(s) sem marcar. Quer concluir mesmo assim?");

        DateOnly? vencimentoAnterior = ProximoVencimentoData;

        switch (TipoVencimento)
        {
            case TipoVencimento.RecorrentePorTempo:
                DataBase = dataConclusao;
                RecalcularProximoVencimento(null, null);
                ReiniciarPassos();
                break;

            case TipoVencimento.UsoOuTempo:
                // RN07: km da conclusão se torna a nova base; veículo será atualizado pelo service
                DataBase = dataConclusao;
                KmBase = kmNaConclusao;
                RecalcularProximoVencimento(kmNaConclusao, mediaKmMesVeiculo);
                ReiniciarPassos();
                break;

            case TipoVencimento.DataUnica:
                if (novaDataVencimento.HasValue)
                {
                    DataVencimento = novaDataVencimento.Value;
                    RecalcularProximoVencimento(null, null);
                    ReiniciarPassos();
                }
                else
                {
                    Situacao = SituacaoItem.Encerrado;
                    ProximoVencimentoData = null;
                }
                break;

            case TipoVencimento.Calculado:
                Situacao = SituacaoItem.Encerrado;
                ProximoVencimentoData = null;
                break;
        }

        AtualizadoEm = agora;

        var historico = new Historico(
            Id,
            dataConclusao,
            kmNaConclusao,
            valorGasto,
            vencimentoAnterior,
            ProximoVencimentoData,
            observacao,
            agora);

        _historicos.Add(historico);
        return historico;
    }

    // ── Edição ────────────────────────────────────────────────────────────────

    public void Editar(
        string nome,
        short categoriaId,
        DateOnly? dataBase,
        DateOnly? dataVencimento,
        int? intervaloDias,
        int? intervaloKm,
        int? kmBase,
        short? garantiaFabricanteMeses,
        short antecedenciaDias,
        Guid? veiculoId,
        string? observacao,
        DateTimeOffset agora,
        int? kmAtualVeiculo = null,
        int? mediaKmMesVeiculo = null)
    {
        if (Situacao == SituacaoItem.Encerrado)
            throw new DomainException("ITEM_ENCERRADO", "Um item encerrado não pode ser editado.");

        Nome = nome;
        CategoriaId = categoriaId;
        DataBase = dataBase;
        DataVencimento = dataVencimento;
        IntervaloDias = intervaloDias;
        IntervaloKm = intervaloKm;
        KmBase = kmBase;
        GarantiaFabricanteMeses = garantiaFabricanteMeses;
        AntecedenciaDias = antecedenciaDias;
        VeiculoId = veiculoId;
        Observacao = observacao;
        AtualizadoEm = agora;

        RecalcularProximoVencimento(kmAtualVeiculo, mediaKmMesVeiculo);
    }

    // ── Passos ────────────────────────────────────────────────────────────────

    public void AdicionarPasso(string descricao)
    {
        short ordem = (short)(_passos.Count + 1);
        _passos.Add(new Passo(Id, ordem, descricao));
    }

    private void ReiniciarPassos()
    {
        foreach (var p in _passos)
            p.Reiniciar();
    }

    /// <summary>Progresso dos passos: 0.0 a 1.0.</summary>
    public double ProgressoPassos() =>
        _passos.Count == 0 ? 1.0 : (double)_passos.Count(p => p.Concluido) / _passos.Count;
}
