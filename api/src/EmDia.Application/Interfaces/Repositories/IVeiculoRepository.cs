using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IVeiculoRepository
{
    Task<IReadOnlyList<Veiculo>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Veiculo?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task ExcluirAsync(Veiculo veiculo, CancellationToken ct = default);
}
