using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly EmDiaDbContext _db;

    public CategoriaRepository(EmDiaDbContext db) => _db = db;

    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default)
        => await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(ct);

    public async Task<Categoria?> ObterPorIdAsync(short id, CancellationToken ct = default)
        => await _db.Categorias.FindAsync([id], ct);
}
