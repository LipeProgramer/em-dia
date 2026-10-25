using EmDia.Domain.Enums;

namespace EmDia.Application.DTOs.Veiculos;

public record CriarVeiculoRequest(
    string Apelido,
    TipoVeiculo Tipo,
    int KmAtual,
    int MediaKmMes);

public record EditarVeiculoRequest(
    string Apelido,
    TipoVeiculo Tipo,
    int MediaKmMes);

public record VeiculoResponse(
    Guid Id,
    string Apelido,
    TipoVeiculo Tipo,
    int KmAtual,
    int MediaKmMes,
    DateTimeOffset KmAtualizadoEm,
    DateTimeOffset CriadoEm);
