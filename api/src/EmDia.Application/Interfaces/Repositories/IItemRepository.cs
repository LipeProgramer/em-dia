using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IItemRepository
{
    Task<IReadOnlyList<Item>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Item?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Item item, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task ExcluirAsync(Item item, CancellationToken ct = default);
}
