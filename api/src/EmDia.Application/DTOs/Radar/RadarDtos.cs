using EmDia.Domain.Enums;
using EmDia.Application.DTOs.Itens;

namespace EmDia.Application.DTOs.Radar;

// ── E10: Radar ────────────────────────────────────────────────────────────────
public record ItemRadarResponse(
    Guid Id,
    string Nome,
    short CategoriaId,
    AnelRadar Anel,
    StatusItem Status,
    DateOnly? ProximoVencimentoData,
    int? ProximoVencimentoKm,
    int DiasRestantes,
    double ProgressoPassos,
    IReadOnlyList<PassoResponse> Passos);

public record RadarResponse(
    IReadOnlyList<ItemRadarResponse> Centro,
    IReadOnlyList<ItemRadarResponse> Anel1,
    IReadOnlyList<ItemRadarResponse> Anel2,
    IReadOnlyList<ItemRadarResponse> Externo,
    bool ModoFoco);
