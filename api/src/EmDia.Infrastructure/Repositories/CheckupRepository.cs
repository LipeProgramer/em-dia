using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class CheckupRepository : ICheckupRepository
{
    private readonly EmDiaDbContext _db;

    public CheckupRepository(EmDiaDbContext db) => _db = db;

    public async Task<Checkup?> ObterAtualPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _db.Checkups
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.RealizadoEm)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Checkup>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _db.Checkups
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.RealizadoEm)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Checkup checkup, CancellationToken ct = default)
        => await _db.Checkups.AddAsync(checkup, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
