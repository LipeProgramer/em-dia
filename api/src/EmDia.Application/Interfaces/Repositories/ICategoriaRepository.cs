using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default);
    Task<Categoria?> ObterPorIdAsync(short id, CancellationToken ct = default);
}
