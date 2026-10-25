using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly EmDiaDbContext _db;

    public VeiculoRepository(EmDiaDbContext db) => _db = db;

    public async Task<IReadOnlyList<Veiculo>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _db.Veiculos
            .Where(v => v.UsuarioId == usuarioId)
            .OrderBy(v => v.Apelido)
            .ToListAsync(ct);

    public async Task<Veiculo?> ObterPorIdEUsuarioAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _db.Veiculos
            .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == usuarioId, ct);

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default)
        => await _db.Veiculos.AddAsync(veiculo, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task ExcluirAsync(Veiculo veiculo, CancellationToken ct = default)
    {
        _db.Veiculos.Remove(veiculo);
        await _db.SaveChangesAsync(ct);
    }
}
