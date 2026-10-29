using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces.Repositories;

// Interfaces de acesso a dados (implementadas na Infrastructure com EF Core).
// Toda consulta de dados do usuário filtra pelo dono do registro (RN12).

public interface IUnitOfWork
{
    /// <summary>Grava todas as alterações pendentes em uma única transação.</summary>
    Task SalvarAsync(CancellationToken ct = default);
}

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);

    /// <summary>Exclui o usuário e todos os seus dados (RF18 · LGPD).</summary>
    Task ExcluirComDadosAsync(Usuario usuario, CancellationToken ct = default);
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorHashAsync(string hash, CancellationToken ct = default);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RevogarTodosDoUsuarioAsync(Guid usuarioId, DateTimeOffset agora, CancellationToken ct = default);
}

public interface ICatalogoRepository
{
    Task<IReadOnlyList<Categoria>> ListarCategoriasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ModeloItem>> ListarModelosAtivosAsync(short? categoriaId, CancellationToken ct = default);
    Task<ModeloItem?> ObterModeloAsync(Guid id, CancellationToken ct = default);
}

public interface ICheckupRepository
{
    Task AdicionarAsync(Checkup checkup, CancellationToken ct = default);
}

public interface IVeiculoRepository
{
    Task<IReadOnlyList<Veiculo>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Veiculo?> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<bool> ApelidoExisteAsync(Guid usuarioId, string apelido, Guid? ignorarId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> ContarItensPorVeiculoAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default);
    void Remover(Veiculo veiculo);
}

public interface IItemRepository
{
    /// <summary>Itens do usuário com os passos.</summary>
    Task<IReadOnlyList<Item>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IReadOnlyList<Item>> ListarAtivosPorVeiculoAsync(Guid veiculoId, Guid usuarioId, CancellationToken ct = default);
    Task<Item?> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Item item, CancellationToken ct = default);
    void Remover(Item item);
}

public record FiltroHistorico(Guid UsuarioId, Guid? ItemId, DateOnly? De, DateOnly? Ate, int? Limite = null);

public interface IHistoricoRepository
{
    /// <summary>Conclusões do usuário, da mais recente para a mais antiga.</summary>
    Task<IReadOnlyList<Historico>> ListarAsync(FiltroHistorico filtro, CancellationToken ct = default);
}

public interface IAvisoRepository
{
    Task<IReadOnlyList<Aviso>> ListarPorItemAsync(Guid itemId, CancellationToken ct = default);
    Task AdicionarAsync(Aviso aviso, CancellationToken ct = default);
}

public interface IInscricaoPushRepository
{
    Task<InscricaoPush?> ObterPorEndpointAsync(string endpoint, CancellationToken ct = default);
    Task AdicionarAsync(InscricaoPush inscricao, CancellationToken ct = default);
    void Remover(InscricaoPush inscricao);
}
