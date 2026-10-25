using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly EmDiaDbContext _db;

    public UsuarioRepository(EmDiaDbContext db) => _db = db;

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Preferencias)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        => await _db.Usuarios
            .Include(u => u.Preferencias)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => await _db.Usuarios.AddAsync(usuario, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task ExcluirAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (usuario != null)
            _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync(ct);
    }
}
