using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IModeloItemRepository
{
    Task<IReadOnlyList<ModeloItem>> ListarAsync(CancellationToken ct = default);
    Task<ModeloItem?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
