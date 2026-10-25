using EmDia.Domain.Enums;

namespace EmDia.Application.DTOs.Catalogo;

public record CategoriaResponse(
    short Id,
    string Nome,
    string Icone,
    short Ordem);

public record PassoModeloResponse(
    short Ordem,
    string Descricao);

public record ModeloResponse(
    Guid Id,
    short CategoriaId,
    string Nome,
    TipoVencimento TipoVencimento,
    int? IntervaloDias,
    int? IntervaloKm,
    short? GarantiaFabricanteMeses,
    short AntecedenciaDias,
    IReadOnlyList<PassoModeloResponse> Passos);
