using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly EmDiaDbContext _db;

    public ItemRepository(EmDiaDbContext db) => _db = db;

    public async Task<IReadOnlyList<Item>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _db.Itens
            .Include(i => i.Passos)
            .Where(i => i.UsuarioId == usuarioId)
            .OrderBy(i => i.ProximoVencimentoData)
            .ToListAsync(ct);

    public async Task<Item?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _db.Itens
            .Include(i => i.Passos)
            .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId, ct);

    public async Task AdicionarAsync(Item item, CancellationToken ct = default)
        => await _db.Itens.AddAsync(item, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task ExcluirAsync(Item item, CancellationToken ct = default)
    {
        _db.Itens.Remove(item);
        await _db.SaveChangesAsync(ct);
    }
}
