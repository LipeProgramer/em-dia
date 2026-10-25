using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorHashAsync(string hash, CancellationToken ct = default);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevogarTodosDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
