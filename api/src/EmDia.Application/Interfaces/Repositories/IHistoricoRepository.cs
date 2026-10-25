using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IHistoricoRepository
{
    Task<IReadOnlyList<Historico>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Historico historico, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
