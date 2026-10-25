using EmDia.Application.DTOs.Itens;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// E25: POST /api/v1/itens/{id}/concluir – conclui item (RN05/RN06)
/// E26: GET  /api/v1/itens/{id}/historico – lista histórico do item
/// </summary>
public class ConcluirService
{
    private readonly IItemRepository _itens;
    private readonly IVeiculoRepository _veiculos;
    private readonly IHistoricoRepository _historicos;
    private readonly IRelogio _relogio;
    private readonly ItemService _itemService;

    public ConcluirService(
        IItemRepository itens,
        IVeiculoRepository veiculos,
        IHistoricoRepository historicos,
        IRelogio relogio,
        ItemService itemService)
    {
        _itens       = itens;
        _veiculos    = veiculos;
        _historicos  = historicos;
        _relogio     = relogio;
        _itemService = itemService;
    }

    /// <summary>E25 – conclui item e retorna o item atualizado.</summary>
    public async Task<ItemResponse> ConcluirAsync(
        Guid itemId, Guid usuarioId, ConcluirItemRequest req, CancellationToken ct)
    {
        var agora = _relogio.Agora;
        var hoje  = _relogio.Hoje;

        var item = await _itens.ObterPorIdEUsuarioAsync(itemId, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");

        int? kmAtualAntes = null;
        int? mediaKmMes   = null;

        if (item.VeiculoId.HasValue)
        {
            var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(item.VeiculoId.Value, usuarioId, ct);
            if (veiculo is not null)
            {
                kmAtualAntes = veiculo.KmAtual;
                mediaKmMes   = veiculo.MediaKmMes;
            }
        }

        // Chama domínio — retorna o Historico criado (RN05/RN06)
        var historico = item.Concluir(
            req.DataConclusao,
            req.KmNaConclusao,
            req.ValorGasto,
            req.Observacao,
            req.ConfirmarPassosPendentes,
            req.NovaDataVencimento,
            agora,
            hoje,
            kmAtualAntes,
            mediaKmMes);

        await _historicos.AdicionarAsync(historico, ct);
        await _itens.SalvarAsync(ct);

        return await _itemService.ObterAsync(itemId, usuarioId, ct);
    }

    /// <summary>E26 – lista histórico do item.</summary>
    public async Task<IReadOnlyList<HistoricoResponse>> ListarHistoricoAsync(
        Guid itemId, Guid usuarioId, CancellationToken ct)
    {
        // Verifica ownership
        _ = await _itens.ObterPorIdEUsuarioAsync(itemId, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");

        var lista = await _historicos.ListarPorItemEUsuarioAsync(itemId, usuarioId, ct);

        return lista.Select(h => new HistoricoResponse(
            h.Id,
            h.ItemId,
            h.ConcluidoEm,
            h.KmNaConclusao,
            h.ValorGasto,
            h.VencimentoAnterior,
            h.ProximoVencimento,
            h.Observacao,
            h.CriadoEm)).ToList();
    }
}
