using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class HistoricoRepository : IHistoricoRepository
{
    private readonly EmDiaDbContext _db;

    public HistoricoRepository(EmDiaDbContext db) => _db = db;

    public async Task<IReadOnlyList<Historico>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var itemIds = await _db.Itens
            .Where(i => i.UsuarioId == usuarioId)
            .Select(i => i.Id)
            .ToListAsync(ct);
        return await _db.Historicos
            .Where(h => itemIds.Contains(h.ItemId))
            .OrderByDescending(h => h.ConcluidoEm)
            .ToListAsync(ct);
    }

    public async Task AdicionarAsync(Historico historico, CancellationToken ct = default)
        => await _db.Historicos.AddAsync(historico, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
