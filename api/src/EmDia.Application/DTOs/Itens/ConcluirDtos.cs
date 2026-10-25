namespace EmDia.Application.DTOs.Itens;

// ── E25: Concluir item ────────────────────────────────────────────────────────
public record ConcluirItemRequest(
    DateOnly DataConclusao,
    int? KmNaConclusao,
    decimal? ValorGasto,
    DateOnly? NovaDataVencimento,
    string? Observacao,
    bool ConfirmarPassosPendentes = false);

// ── E26: Histórico ────────────────────────────────────────────────────────────
public record HistoricoResponse(
    Guid Id,
    Guid ItemId,
    DateOnly ConcluidoEm,
    int? KmNaConclusao,
    decimal? ValorGasto,
    DateOnly? VencimentoAnterior,
    DateOnly? ProximoVencimento,
    string? Observacao,
    DateTimeOffset CriadoEm);
