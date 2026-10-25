using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class ModeloItemRepository : IModeloItemRepository
{
    private readonly EmDiaDbContext _db;

    public ModeloItemRepository(EmDiaDbContext db) => _db = db;

    public async Task<IReadOnlyList<ModeloItem>> ListarAsync(CancellationToken ct = default)
        => await _db.ModelosItem
            .Include(m => m.Passos)
            .OrderBy(m => m.Nome)
            .ToListAsync(ct);

    public async Task<ModeloItem?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _db.ModelosItem
            .Include(m => m.Passos)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
}
