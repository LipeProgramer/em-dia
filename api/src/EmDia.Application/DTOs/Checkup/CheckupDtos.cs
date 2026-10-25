namespace EmDia.Application.DTOs.Checkup;

// ── E11: Executar check-up ────────────────────────────────────────────────────
/// <summary>
/// Respostas do check-up: chave = CodigoCheckup do modelo, valor = true/false.
/// Ex: { "TEM_CARRO": true, "TEM_CNH": true, "TEM_CASA": false }
/// </summary>
public record ExecutarCheckupRequest(Dictionary<string, bool> Respostas);

// ── E11/E12: Resposta ────────────────────────────────────────────────────────
public record CheckupResponse(
    Guid Id,
    DateTimeOffset RealizadoEm,
    short ItensCriados,
    Dictionary<string, bool> Respostas);
