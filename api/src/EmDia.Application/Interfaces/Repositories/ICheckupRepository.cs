using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface ICheckupRepository
{
    Task<Checkup?> ObterAtualPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<Checkup>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Checkup checkup, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
