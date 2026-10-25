using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

public interface IAvisoRepository
{
    Task<bool> JaEnviouHojeAsync(Guid itemId, DateOnly vencimentoRef, short numero, CancellationToken ct = default);
    Task<IReadOnlyList<Aviso>> ListarPorItemAsync(Guid itemId, CancellationToken ct = default);
    Task AdicionarAsync(Aviso aviso, CancellationToken ct = default);
    Task SalvarAsync(CancellationToken ct = default);
}
