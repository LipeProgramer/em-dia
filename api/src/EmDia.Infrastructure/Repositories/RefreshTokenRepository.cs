using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly EmDiaDbContext _db;

    public RefreshTokenRepository(EmDiaDbContext db) => _db = db;

    public async Task<RefreshToken?> ObterPorHashAsync(string hash, CancellationToken ct = default)
        => await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken ct = default)
        => await _db.RefreshTokens.AddAsync(refreshToken, ct);

    public async Task RevogarTodosDoUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UsuarioId == usuarioId && rt.RevogadoEm == null)
            .ToListAsync(ct);
        foreach (var rt in tokens)
            rt.Revogar(agora);
    }

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
