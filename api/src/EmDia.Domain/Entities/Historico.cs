namespace EmDia.Domain.Entities;

/// <summary>
/// Registro de uma conclusão de item (spec §2.3).
/// Uma linha por conclusão.
/// </summary>
public class Historico
{
    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }

    /// <summary>Item concluído (navegação usada pelo histórico — E25).</summary>
    public Item Item { get; private set; } = null!;
    public DateOnly ConcluidoEm { get; private set; }
    public int? KmNaConclusao { get; private set; }
    public decimal? ValorGasto { get; private set; }
    public DateOnly? VencimentoAnterior { get; private set; }
    public DateOnly? ProximoVencimento { get; private set; }
    public string? Observacao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    // EF Core
    protected Historico() { }

    public Historico(
        Guid itemId,
        DateOnly concluidoEm,
        int? kmNaConclusao,
        decimal? valorGasto,
        DateOnly? vencimentoAnterior,
        DateOnly? proximoVencimento,
        string? observacao,
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        ItemId = itemId;
        ConcluidoEm = concluidoEm;
        KmNaConclusao = kmNaConclusao;
        ValorGasto = valorGasto;
        VencimentoAnterior = vencimentoAnterior;
        ProximoVencimento = proximoVencimento;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        CriadoEm = agora;
    }
}
