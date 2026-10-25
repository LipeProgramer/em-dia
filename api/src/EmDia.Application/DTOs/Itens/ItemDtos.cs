using EmDia.Domain.Enums;

namespace EmDia.Application.DTOs.Itens;

public record PassoResponse(
    Guid Id,
    short Ordem,
    string Descricao,
    bool Concluido,
    DateTimeOffset? ConcluidoEm);

public record ItemResponse(
    Guid Id,
    Guid UsuarioId,
    short CategoriaId,
    Guid? ModeloItemId,
    Guid? VeiculoId,
    string Nome,
    TipoVencimento TipoVencimento,
    DateOnly? DataBase,
    DateOnly? DataVencimento,
    int? IntervaloDias,
    int? IntervaloKm,
    int? KmBase,
    short? GarantiaFabricanteMeses,
    short AntecedenciaDias,
    DateOnly? ProximoVencimentoData,
    int? ProximoVencimentoKm,
    SituacaoItem Situacao,
    StatusItem Status,
    string? Observacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm,
    IReadOnlyList<PassoResponse> Passos);

public record CriarItemRequest(
    string Nome,
    short CategoriaId,
    TipoVencimento TipoVencimento,
    DateOnly? DataBase,
    DateOnly? DataVencimento,
    int? IntervaloDias,
    int? IntervaloKm,
    int? KmBase,
    short? GarantiaFabricanteMeses,
    short AntecedenciaDias,
    Guid? VeiculoId,
    Guid? ModeloItemId,
    string? Observacao,
    IReadOnlyList<string>? Passos);

public record EditarItemRequest(
    string Nome,
    short CategoriaId,
    DateOnly? DataBase,
    DateOnly? DataVencimento,
    int? IntervaloDias,
    int? IntervaloKm,
    int? KmBase,
    short? GarantiaFabricanteMeses,
    short AntecedenciaDias,
    Guid? VeiculoId,
    string? Observacao);

public record MarcarPassoRequest(bool Concluido);
