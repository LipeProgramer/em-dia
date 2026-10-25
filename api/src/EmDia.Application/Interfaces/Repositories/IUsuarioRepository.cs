using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
    Task ExcluirAsync(Guid id, CancellationToken ct = default);
}
