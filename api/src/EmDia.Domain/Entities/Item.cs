using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;

namespace EmDia.Domain.Entities;

/// <summary>
/// Item de vencimento acompanhado pelo usuário. Entidade central do sistema.
/// Contém o comportamento das regras RN01, RN02, RN03, RN05, RN06, RN07, RN14 e RN15 (spec §3.2).
/// O status nunca é gravado — é calculado na leitura (spec §2.1).
/// </summary>
public class Item
{
    public const int MaxPassos = 10;
    public const int MaxDescricaoPasso = 120;

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
    /// Próximo vencimento por data, gravado e recalculado a cada alteração.
    /// No tipo UsoOuTempo guarda a data limite pelo tempo; a data estimada pela km
    /// é calculada na leitura (RN01) por <see cref="DataEfetiva"/>.
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
    public IReadOnlyList<Passo> Passos => _passos.OrderBy(p => p.Ordem).ToList().AsReadOnly();

    private readonly List<Historico> _historicos = new();
    public IReadOnlyList<Historico> Historicos => _historicos.AsReadOnly();

    // EF Core
    protected Item() { }

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
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        ModeloItemId = modeloItemId;
        TipoVencimento = tipoVencimento;
        Situacao = SituacaoItem.Ativo;
        CriadoEm = agora;

        Atribuir(nome, categoriaId, dataBase, dataVencimento, intervaloDias, intervaloKm, kmBase,
            garantiaFabricanteMeses, antecedenciaDias, veiculoId, observacao, agora);
    }

    public bool Encerrado => Situacao == SituacaoItem.Encerrado;

    // ── RN01 — Cálculo do próximo vencimento ──────────────────────────────────

    /// <summary>
    /// RN01: recalcula e grava ProximoVencimentoData e ProximoVencimentoKm.
    /// Data única: a validade informada. Recorrente: data_base + intervalo_dias.
    /// Uso ou tempo: data limite pelo tempo + km alvo. Calculado: compra + garantia do fabricante + 90 dias.
    /// </summary>
    public void RecalcularProximoVencimento()
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
                ProximoVencimentoData = DataBase!.Value.AddDays(IntervaloDias!.Value);
                ProximoVencimentoKm = KmBase!.Value + IntervaloKm!.Value;
                break;

            case TipoVencimento.Calculado:
                ProximoVencimentoData = DataBase!.Value
                    .AddMonths(GarantiaFabricanteMeses!.Value)
                    .AddDays(90);
                ProximoVencimentoKm = null;
                break;
        }
    }

    /// <summary>Km que falta para o vencimento por uso (negativo = passou). Só para UsoOuTempo.</summary>
    public int? KmRestantes(Veiculo? veiculo) =>
        TipoVencimento == TipoVencimento.UsoOuTempo && ProximoVencimentoKm.HasValue && veiculo is not null
            ? ProximoVencimentoKm.Value - veiculo.KmAtual
            : null;

    /// <summary>
    /// RN01 (leitura): data estimada para atingir a km do vencimento,
    /// calculada por (km que falta ÷ média de km por mês) × 30 dias.
    /// </summary>
    public DateOnly? DataEstimadaPelaKm(DateOnly hoje, Veiculo? veiculo)
    {
        var kmRestantes = KmRestantes(veiculo);
        if (kmRestantes is null || veiculo!.MediaKmMes <= 0)
            return null;

        int dias = (int)Math.Ceiling(kmRestantes.Value * 30.0 / veiculo.MediaKmMes);
        return hoje.AddDays(dias);
    }

    /// <summary>
    /// Data que vale para o prazo: no tipo UsoOuTempo, o que vier primeiro entre
    /// a data pelo tempo e a data estimada pela km; nos demais, o próximo vencimento gravado.
    /// </summary>
    public DateOnly? DataEfetiva(DateOnly hoje, Veiculo? veiculo)
    {
        var porKm = DataEstimadaPelaKm(hoje, veiculo);
        if (porKm is null) return ProximoVencimentoData;
        if (ProximoVencimentoData is null) return porKm;
        return porKm < ProximoVencimentoData ? porKm : ProximoVencimentoData;
    }

    /// <summary>Dias até a data efetiva (negativo = vencido há X dias).</summary>
    public int? DiasRestantes(DateOnly hoje, Veiculo? veiculo) =>
        DataEfetiva(hoje, veiculo) is { } data ? data.DayNumber - hoje.DayNumber : null;

    /// <summary>True quando, no tipo UsoOuTempo, a km atingiu a do vencimento antes do tempo.</summary>
    public bool KmVemPrimeiro(DateOnly hoje, Veiculo? veiculo)
    {
        var porKm = DataEstimadaPelaKm(hoje, veiculo);
        return porKm is not null && (ProximoVencimentoData is null || porKm < ProximoVencimentoData);
    }

    // ── RN02 — Status calculado na leitura ────────────────────────────────────

    /// <summary>
    /// RN02: Vencido = a data passou ou a km atual atingiu a km do vencimento;
    /// Atenção = faltam até antecedencia_dias; Em dia = demais casos;
    /// Encerrado = item de data única ou garantia já concluído.
    /// </summary>
    public StatusItem CalcularStatus(DateOnly hoje, Veiculo? veiculo)
    {
        if (Encerrado)
            return StatusItem.Encerrado;

        if (KmRestantes(veiculo) is <= 0)
            return StatusItem.Vencido;

        var dias = DiasRestantes(hoje, veiculo);
        if (dias is null)
            return StatusItem.EmDia;
        if (dias < 0)
            return StatusItem.Vencido;
        if (dias <= AntecedenciaDias)
            return StatusItem.Atencao;
        return StatusItem.EmDia;
    }

    // ── RN03 — Anel do Radar ──────────────────────────────────────────────────

    /// <summary>RN03: Centro = vencido; Anel1 = até 30 dias; Anel2 = 31 a 90; Externo = mais de 90.</summary>
    public AnelRadar CalcularAnel(DateOnly hoje, Veiculo? veiculo)
    {
        if (CalcularStatus(hoje, veiculo) is StatusItem.Vencido or StatusItem.Encerrado)
            return AnelRadar.Centro;

        return DiasRestantes(hoje, veiculo) switch
        {
            null => AnelRadar.Externo,
            <= 30 => AnelRadar.Anel1,
            <= 90 => AnelRadar.Anel2,
            _ => AnelRadar.Externo
        };
    }

    /// <summary>Texto curto do prazo, em linguagem simples (ex.: "venceu há 5 dias", "faltam 500 km").</summary>
    public string TextoPrazo(DateOnly hoje, Veiculo? veiculo)
    {
        if (Encerrado)
            return "concluído";

        var kmRestantes = KmRestantes(veiculo);
        if (kmRestantes is <= 0)
            return kmRestantes == 0
                ? "chegou na km da troca"
                : $"passou {Formatos.Km(-kmRestantes.Value)} km";

        if (KmVemPrimeiro(hoje, veiculo) && kmRestantes is > 0)
            return $"faltam {Formatos.Km(kmRestantes.Value)} km";

        return DiasRestantes(hoje, veiculo) switch
        {
            null => "sem prazo",
            0 => "vence hoje",
            1 => "vence amanhã",
            -1 => "venceu ontem",
            < 0 and var d => $"venceu há {-d} dias",
            var d => $"faltam {d} dias"
        };
    }

    // ── RN05/RN06/RN07/RN15 — Concluir ────────────────────────────────────────

    /// <summary>
    /// RN05: registra o histórico e renova o item.
    /// Recorrente e uso ou tempo: data_base = data da conclusão (e km_base = km informada),
    /// recalcula e desmarca os passos. Data única: se vier nova validade, atualiza; senão, encerra.
    /// Garantia: encerra. RN06: passos pendentes exigem confirmação. RN07: atualiza a km do veículo.
    /// </summary>
    public ResultadoConclusao Concluir(
        DateOnly dataConclusao,
        int? km,
        decimal? valorGasto,
        string? observacao,
        bool confirmarPassosPendentes,
        DateOnly? novaDataVencimento,
        Veiculo? veiculo,
        DateTimeOffset agora,
        DateOnly hoje)
    {
        if (Encerrado)
            throw new DomainException(CodigosErro.ItemEncerrado, "Este item já foi concluído e encerrado.");

        if (dataConclusao > hoje)
            throw new CampoInvalidoException("dataConclusao", "A data não pode estar no futuro.");

        // RN06 — passos pendentes
        int pendentes = _passos.Count(p => !p.Concluido);
        if (pendentes > 0 && !confirmarPassosPendentes)
            throw new DomainException(CodigosErro.PassosPendentes, pendentes == 1
                ? "Este item ainda tem 1 passo sem marcar. Quer concluir mesmo assim?"
                : $"Este item ainda tem {pendentes} passos sem marcar. Quer concluir mesmo assim?");

        if (TipoVencimento == TipoVencimento.UsoOuTempo)
        {
            if (km is null)
                throw new CampoInvalidoException("km", "Informe a quilometragem da conclusão.");
            if (km < KmBase)
                throw new CampoInvalidoException("km", $"A km não pode ser menor que {Formatos.Km(KmBase!.Value)}.");
            if (veiculo is not null && km < veiculo.KmAtual)
                throw new DomainException(CodigosErro.KmMenor,
                    $"A km não pode ser menor que {Formatos.Km(veiculo.KmAtual)}.");
        }

        if (TipoVencimento == TipoVencimento.DataUnica && novaDataVencimento is { } nova && nova <= hoje)
            throw new CampoInvalidoException("novaDataVencimento", "A nova validade precisa ser uma data futura.");

        DateOnly? vencimentoAnterior = ProximoVencimentoData;

        switch (TipoVencimento)
        {
            case TipoVencimento.RecorrentePorTempo:
                DataBase = dataConclusao;
                RecalcularProximoVencimento();
                DesmarcarPassos();
                break;

            case TipoVencimento.UsoOuTempo:
                DataBase = dataConclusao;
                KmBase = km;
                RecalcularProximoVencimento();
                DesmarcarPassos();
                // RN07: km maior que a atual atualiza o veículo
                if (veiculo is not null && km > veiculo.KmAtual)
                    veiculo.AtualizarKm(km!.Value, agora);
                break;

            case TipoVencimento.DataUnica when novaDataVencimento.HasValue:
                DataVencimento = novaDataVencimento.Value;
                RecalcularProximoVencimento();
                DesmarcarPassos();
                break;

            default: // Data única sem nova validade e garantia (Calculado)
                Situacao = SituacaoItem.Encerrado;
                break;
        }

        AtualizadoEm = agora;

        var historico = new Historico(
            Id,
            dataConclusao,
            km,
            valorGasto,
            vencimentoAnterior,
            Encerrado ? null : ProximoVencimentoData,
            observacao,
            agora);

        _historicos.Add(historico);
        return new ResultadoConclusao(historico, MensagemConclusao());
    }

    /// <summary>RN15: mensagem positiva, sem culpa, devolvida após concluir.</summary>
    private string MensagemConclusao()
    {
        if (Encerrado)
            return "Pronto! Item concluído e guardado no seu histórico.";

        var data = Formatos.Data(ProximoVencimentoData!.Value);
        return TipoVencimento == TipoVencimento.UsoOuTempo
            ? $"Boa! Próxima vez: {Formatos.Km(ProximoVencimentoKm!.Value)} km ou {data}, o que vier primeiro."
            : $"Boa! Próximo prazo: {data}.";
    }

    // ── Edição ────────────────────────────────────────────────────────────────

    public void Editar(
        string nome,
        short categoriaId,
        TipoVencimento tipoVencimento,
        DateOnly? dataBase,
        DateOnly? dataVencimento,
        int? intervaloDias,
        int? intervaloKm,
        int? kmBase,
        short? garantiaFabricanteMeses,
        short antecedenciaDias,
        Guid? veiculoId,
        string? observacao,
        DateTimeOffset agora)
    {
        if (Encerrado)
            throw new DomainException(CodigosErro.ItemEncerrado, "Um item encerrado não pode ser editado.");

        ValidarCamposPorTipo(tipoVencimento, dataBase, dataVencimento, intervaloDias, intervaloKm, kmBase,
            garantiaFabricanteMeses, veiculoId);
        TipoVencimento = tipoVencimento;
        Atribuir(nome, categoriaId, dataBase, dataVencimento, intervaloDias, intervaloKm, kmBase,
            garantiaFabricanteMeses, antecedenciaDias, veiculoId, observacao, agora);
    }

    private void Atribuir(
        string nome, short categoriaId, DateOnly? dataBase, DateOnly? dataVencimento,
        int? intervaloDias, int? intervaloKm, int? kmBase, short? garantiaFabricanteMeses,
        short antecedenciaDias, Guid? veiculoId, string? observacao, DateTimeOffset agora)
    {
        ValidarCamposPorTipo(TipoVencimento, dataBase, dataVencimento, intervaloDias, intervaloKm, kmBase,
            garantiaFabricanteMeses, veiculoId);

        Nome = nome.Trim();
        CategoriaId = categoriaId;
        DataBase = dataBase;
        DataVencimento = dataVencimento;
        IntervaloDias = intervaloDias;
        IntervaloKm = intervaloKm;
        KmBase = kmBase;
        GarantiaFabricanteMeses = garantiaFabricanteMeses;
        AntecedenciaDias = antecedenciaDias;
        VeiculoId = veiculoId;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        AtualizadoEm = agora;

        RecalcularProximoVencimento();
    }

    /// <summary>Campos exigidos por tipo de vencimento (espelha os CHECKs da tabela itens — spec §2.5).</summary>
    public static void ValidarCamposPorTipo(
        TipoVencimento tipo, DateOnly? dataBase, DateOnly? dataVencimento, int? intervaloDias,
        int? intervaloKm, int? kmBase, short? garantiaFabricanteMeses, Guid? veiculoId)
    {
        switch (tipo)
        {
            case TipoVencimento.DataUnica when dataVencimento is null:
                throw new CampoInvalidoException("dataVencimento", "Informe a data de vencimento.");
            case TipoVencimento.RecorrentePorTempo when dataBase is null:
                throw new CampoInvalidoException("dataBase", "Informe a data da última vez.");
            case TipoVencimento.RecorrentePorTempo when intervaloDias is null:
                throw new CampoInvalidoException("intervaloDias", "Informe o intervalo em dias.");
            case TipoVencimento.UsoOuTempo when veiculoId is null:
                throw new DomainException(CodigosErro.VeiculoObrigatorio, "Escolha o veículo deste item.");
            case TipoVencimento.UsoOuTempo when dataBase is null:
                throw new CampoInvalidoException("dataBase", "Informe a data da última vez.");
            case TipoVencimento.UsoOuTempo when kmBase is null:
                throw new CampoInvalidoException("kmBase", "Informe a km da última vez.");
            case TipoVencimento.UsoOuTempo when intervaloKm is null:
                throw new CampoInvalidoException("intervaloKm", "Informe o intervalo em km.");
            case TipoVencimento.UsoOuTempo when intervaloDias is null:
                throw new CampoInvalidoException("intervaloDias", "Informe o intervalo em dias.");
            case TipoVencimento.Calculado when dataBase is null:
                throw new CampoInvalidoException("dataBase", "Informe a data da compra.");
            case TipoVencimento.Calculado when garantiaFabricanteMeses is null:
                throw new CampoInvalidoException("garantiaFabricanteMeses", "Informe a garantia do fabricante em meses.");
        }
    }

    // ── Passos ────────────────────────────────────────────────────────────────

    public void AdicionarPasso(string descricao)
    {
        if (_passos.Count >= MaxPassos)
            throw new CampoInvalidoException("passos", $"Use até {MaxPassos} passos.");
        short ordem = (short)(_passos.Count == 0 ? 1 : _passos.Max(p => p.Ordem) + 1);
        _passos.Add(new Passo(Id, ordem, descricao));
    }

    /// <summary>Recria a lista de passos (E16, quando os passos são enviados).</summary>
    public void SubstituirPassos(IEnumerable<string> descricoes)
    {
        _passos.Clear();
        foreach (var d in descricoes)
            AdicionarPasso(d);
    }

    public Passo MarcarPasso(Guid passoId, bool concluido, DateTimeOffset agora)
    {
        var passo = _passos.FirstOrDefault(p => p.Id == passoId)
            ?? throw new NotFoundException("PASSO_NAO_ENCONTRADO", "Não encontramos este passo.");
        if (Encerrado)
            throw new DomainException(CodigosErro.ItemEncerrado, "Um item encerrado não pode ser alterado.");
        passo.Marcar(concluido, agora);
        AtualizadoEm = agora;
        return passo;
    }

    private void DesmarcarPassos()
    {
        foreach (var p in _passos)
            p.Reiniciar();
    }

    /// <summary>Progresso dos passos: 0 a 1, com duas casas (0 quando não há passos).</summary>
    public double ProgressoPassos() =>
        _passos.Count == 0 ? 0 : Math.Round((double)_passos.Count(p => p.Concluido) / _passos.Count, 2);
}

/// <summary>Resultado da conclusão: histórico criado e mensagem positiva (RN15).</summary>
public sealed record ResultadoConclusao(Historico Historico, string Mensagem);
