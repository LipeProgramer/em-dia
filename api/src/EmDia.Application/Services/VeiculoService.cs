using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// E13: GET  /api/v1/veiculos
/// E14: POST /api/v1/veiculos
/// E15: GET  /api/v1/veiculos/{id}
/// E16: PUT  /api/v1/veiculos/{id}
/// E17: DELETE /api/v1/veiculos/{id}
/// </summary>
public class VeiculoService
{
    private readonly IVeiculoRepository _veiculos;
    private readonly IRelogio _relogio;

    public VeiculoService(IVeiculoRepository veiculos, IRelogio relogio)
    {
        _veiculos = veiculos;
        _relogio  = relogio;
    }

    public async Task<IReadOnlyList<VeiculoResponse>> ListarAsync(Guid usuarioId, CancellationToken ct)
    {
        var lista = await _veiculos.ListarPorUsuarioAsync(usuarioId, ct);
        return lista.Select(ToResponse).ToList();
    }

    public async Task<VeiculoResponse> CriarAsync(Guid usuarioId, CriarVeiculoRequest req, CancellationToken ct)
    {
        var agora = _relogio.Agora;
        var veiculo = new Veiculo(usuarioId, req.Apelido, req.Tipo, req.KmAtual, req.MediaKmMes, agora);
        await _veiculos.AdicionarAsync(veiculo, ct);
        await _veiculos.SalvarAsync(ct);
        return ToResponse(veiculo);
    }

    public async Task<VeiculoResponse> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");
        return ToResponse(veiculo);
    }

    public async Task<VeiculoResponse> EditarAsync(Guid id, Guid usuarioId, EditarVeiculoRequest req, CancellationToken ct)
    {
        var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");

        veiculo.Editar(req.Apelido, req.Tipo, req.MediaKmMes);
        await _veiculos.SalvarAsync(ct);
        return ToResponse(veiculo);
    }

    public async Task ExcluirAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");

        await _veiculos.ExcluirAsync(veiculo, ct);
    }

    private static VeiculoResponse ToResponse(Veiculo v) => new(
        v.Id, v.Apelido, v.Tipo, v.KmAtual, v.MediaKmMes, v.KmAtualizadoEm, v.CriadoEm);
}
