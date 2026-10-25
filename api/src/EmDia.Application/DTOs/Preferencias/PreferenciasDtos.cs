namespace EmDia.Application.DTOs.Preferencias;

// ── E27/E28: Preferências de aviso ───────────────────────────────────────────
public record PreferenciasResponse(
    bool ReceberEmail,
    short MaxReavisos,
    short IntervaloReavisoDias,
    bool ModoFocoPadrao,
    DateTimeOffset AtualizadoEm);

public record AtualizarPreferenciasRequest(
    bool ReceberEmail,
    short MaxReavisos,
    short IntervaloReavisoDias,
    bool ModoFocoPadrao);
