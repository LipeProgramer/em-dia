using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

public class AvisoRepository : IAvisoRepository
{
    private readonly EmDiaDbContext _db;

    public AvisoRepository(EmDiaDbContext db) => _db = db;

    public async Task<bool> JaEnviouHojeAsync(Guid itemId, DateOnly vencimentoRef, short numero, CancellationToken ct = default)
        => await _db.Avisos.AnyAsync(
            a => a.ItemId == itemId
              && a.VencimentoReferencia == vencimentoRef
              && a.Numero == numero
              && a.Sucesso,
            ct);

    public async Task AdicionarAsync(Aviso aviso, CancellationToken ct = default)
        => await _db.Avisos.AddAsync(aviso, ct);

    public async Task SalvarAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
