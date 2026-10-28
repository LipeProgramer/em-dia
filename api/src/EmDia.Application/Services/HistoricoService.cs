using EmDia.Application.DTOs.Historico;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>E25: conclusões do usuário, da mais recente para a mais antiga.</summary>
public class HistoricoService(IHistoricoRepository historicos, IItemRepository itens)
{
    public async Task<IReadOnlyList<HistoricoResponse>> ListarAsync(
        Guid usuarioId, Guid? itemId, DateOnly? de, DateOnly? ate, CancellationToken ct)
    {
        if (de is not null && ate is not null && de > ate)
            throw new CampoInvalidoException("de", "A data inicial deve ser anterior ou igual à final.");

        if (itemId is { } id && !await itens.ExisteAsync(id, usuarioId, ct))
            throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Não encontramos este item.");

        return (await historicos.ListarAsync(new FiltroHistorico(usuarioId, itemId, de, ate), ct))
            .Select(h => new HistoricoResponse(h.Id, h.ItemId, h.Item.Nome, h.ConcluidoEm, h.KmNaConclusao,
                h.ValorGasto, h.Observacao, h.VencimentoAnterior, h.ProximoVencimento))
            .ToList();
    }
}
