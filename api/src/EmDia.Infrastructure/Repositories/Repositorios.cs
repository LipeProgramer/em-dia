using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmDia.Infrastructure.Repositories;

// Acesso aos dados com EF Core (consultas sempre parametrizadas).
// Toda consulta de dados do usuário filtra pelo dono do registro (RN12).

public class UnitOfWork(EmDiaDbContext db) : IUnitOfWork
{
    public Task SalvarAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class UsuarioRepository(EmDiaDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Usuarios.Include(u => u.Preferencias).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        db.Usuarios.Include(u => u.Preferencias).FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExisteAsync(string email, CancellationToken ct = default) =>
        db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default) =>
        await db.Usuarios.AddAsync(usuario, ct);

    /// <summary>
    /// Remove primeiro os itens (que apontam para veículos com RESTRICT) e depois o usuário;
    /// o restante (veículos, tokens, check-ups, preferências, passos, histórico, avisos) vai em cascata.
    /// </summary>
    public async Task ExcluirComDadosAsync(Usuario usuario, CancellationToken ct = default)
    {
        // Tudo no mesmo SaveChanges: o EF apaga os itens antes do usuário, numa única transação.
        db.Itens.RemoveRange(await db.Itens.Where(i => i.UsuarioId == usuario.Id).ToListAsync(ct));
        db.Usuarios.Remove(usuario);
    }
}

public class RefreshTokenRepository(EmDiaDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> ObterPorHashAsync(string hash, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(refreshToken, ct);

    public async Task RevogarTodosDoUsuarioAsync(Guid usuarioId, DateTimeOffset agora, CancellationToken ct = default)
    {
        var ativos = await db.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && r.RevogadoEm == null)
            .ToListAsync(ct);
        foreach (var r in ativos)
            r.Revogar(agora);
    }
}

public class CatalogoRepository(EmDiaDbContext db) : ICatalogoRepository
{
    public async Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(CancellationToken ct = default) =>
        await db.Categorias.AsNoTracking().OrderBy(c => c.Ordem).ToListAsync(ct);

    public async Task<IReadOnlyList<ModeloItem>> ListarModelosAtivosAsync(short? categoriaId, CancellationToken ct = default) =>
        await db.ModelosItem.AsNoTracking()
            .Include(m => m.Passos)
            .Where(m => m.Ativo && (categoriaId == null || m.CategoriaId == categoriaId))
            .OrderBy(m => m.CategoriaId).ThenBy(m => m.Nome)
            .ToListAsync(ct);

    public Task<ModeloItem?> ObterModeloAsync(Guid id, CancellationToken ct = default) =>
        db.ModelosItem.AsNoTracking().Include(m => m.Passos).FirstOrDefaultAsync(m => m.Id == id && m.Ativo, ct);
}

public class CheckupRepository(EmDiaDbContext db) : ICheckupRepository
{
    public async Task AdicionarAsync(Checkup checkup, CancellationToken ct = default) =>
        await db.Checkups.AddAsync(checkup, ct);
}

public class VeiculoRepository(EmDiaDbContext db) : IVeiculoRepository
{
    public async Task<IReadOnlyList<Veiculo>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.Veiculos.Where(v => v.UsuarioId == usuarioId).OrderBy(v => v.Apelido).ToListAsync(ct);

    public Task<Veiculo?> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct = default) =>
        db.Veiculos.FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == usuarioId, ct);

    public Task<bool> ApelidoExisteAsync(Guid usuarioId, string apelido, Guid? ignorarId, CancellationToken ct = default) =>
        db.Veiculos.AnyAsync(v => v.UsuarioId == usuarioId
                                  && v.Apelido.ToLower() == apelido.ToLower()
                                  && (ignorarId == null || v.Id != ignorarId), ct);

    public Task<Dictionary<Guid, int>> ContarItensPorVeiculoAsync(Guid usuarioId, CancellationToken ct = default) =>
        db.Itens
            .Where(i => i.UsuarioId == usuarioId && i.VeiculoId != null)
            .GroupBy(i => i.VeiculoId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default) =>
        await db.Veiculos.AddAsync(veiculo, ct);

    public void Remover(Veiculo veiculo) => db.Veiculos.Remove(veiculo);
}

public class ItemRepository(EmDiaDbContext db) : IItemRepository
{
    public async Task<IReadOnlyList<Item>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.Itens.Include(i => i.Passos).Where(i => i.UsuarioId == usuarioId).ToListAsync(ct);

    public async Task<IReadOnlyList<Item>> ListarAtivosPorVeiculoAsync(Guid veiculoId, Guid usuarioId, CancellationToken ct = default) =>
        await db.Itens
            .Where(i => i.UsuarioId == usuarioId && i.VeiculoId == veiculoId && i.Situacao == Domain.Enums.SituacaoItem.Ativo)
            .ToListAsync(ct);

    public Task<Item?> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct = default) =>
        db.Itens.Include(i => i.Passos).FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId, ct);

    public Task<bool> ExisteAsync(Guid id, Guid usuarioId, CancellationToken ct = default) =>
        db.Itens.AnyAsync(i => i.Id == id && i.UsuarioId == usuarioId, ct);

    public async Task AdicionarAsync(Item item, CancellationToken ct = default) =>
        await db.Itens.AddAsync(item, ct);

    public void Remover(Item item) => db.Itens.Remove(item);
}

public class HistoricoRepository(EmDiaDbContext db) : IHistoricoRepository
{
    public async Task<IReadOnlyList<Historico>> ListarAsync(FiltroHistorico f, CancellationToken ct = default)
    {
        var consulta = db.Historicos.AsNoTracking()
            .Include(h => h.Item)
            .Where(h => h.Item.UsuarioId == f.UsuarioId);

        if (f.ItemId is { } itemId) consulta = consulta.Where(h => h.ItemId == itemId);
        if (f.De is { } de) consulta = consulta.Where(h => h.ConcluidoEm >= de);
        if (f.Ate is { } ate) consulta = consulta.Where(h => h.ConcluidoEm <= ate);

        consulta = consulta.OrderByDescending(h => h.ConcluidoEm).ThenByDescending(h => h.CriadoEm);
        if (f.Limite is { } limite) consulta = consulta.Take(limite);

        return await consulta.ToListAsync(ct);
    }
}

public class AvisoRepository(EmDiaDbContext db) : IAvisoRepository
{
    public async Task<IReadOnlyList<Aviso>> ListarPorItemAsync(Guid itemId, CancellationToken ct = default) =>
        await db.Avisos.Where(a => a.ItemId == itemId).OrderBy(a => a.Numero).ToListAsync(ct);

    public async Task AdicionarAsync(Aviso aviso, CancellationToken ct = default) =>
        await db.Avisos.AddAsync(aviso, ct);
}

public class RedefinicaoSenhaRepository(EmDiaDbContext db) : IRedefinicaoSenhaRepository
{
    public Task<RedefinicaoSenha?> ObterPorHashAsync(string hash, CancellationToken ct = default) =>
        db.RedefinicoesSenha.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

    public async Task<IReadOnlyList<RedefinicaoSenha>> ListarAbertasAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.RedefinicoesSenha.Where(r => r.UsuarioId == usuarioId && r.UsadoEm == null).ToListAsync(ct);

    public async Task AdicionarAsync(RedefinicaoSenha redefinicao, CancellationToken ct = default) =>
        await db.RedefinicoesSenha.AddAsync(redefinicao, ct);
}

public class InscricaoPushRepository(EmDiaDbContext db) : IInscricaoPushRepository
{
    public Task<InscricaoPush?> ObterPorEndpointAsync(string endpoint, CancellationToken ct = default) =>
        db.InscricoesPush.FirstOrDefaultAsync(i => i.Endpoint == endpoint, ct);

    public async Task AdicionarAsync(InscricaoPush inscricao, CancellationToken ct = default) =>
        await db.InscricoesPush.AddAsync(inscricao, ct);

    public void Remover(InscricaoPush inscricao) => db.InscricoesPush.Remove(inscricao);
}
