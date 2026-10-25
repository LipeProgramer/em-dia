using EmDia.Application.DTOs.Itens;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// E19: GET    /api/v1/itens
/// E20: POST   /api/v1/itens
/// E21: GET    /api/v1/itens/{id}
/// E22: PUT    /api/v1/itens/{id}
/// E23: DELETE /api/v1/itens/{id}
/// E24: PATCH  /api/v1/itens/{id}/passos/{passoId}
/// </summary>
public class ItemService
{
    private readonly IItemRepository _itens;
    private readonly IVeiculoRepository _veiculos;
    private readonly IModeloItemRepository _modelos;
    private readonly IRelogio _relogio;

    public ItemService(
        IItemRepository itens,
        IVeiculoRepository veiculos,
        IModeloItemRepository modelos,
        IRelogio relogio)
    {
        _itens    = itens;
        _veiculos = veiculos;
        _modelos  = modelos;
        _relogio  = relogio;
    }

    public async Task<IReadOnlyList<ItemResponse>> ListarAsync(
        Guid usuarioId, short? categoriaId, CancellationToken ct)
    {
        var lista = await _itens.ListarPorUsuarioAsync(usuarioId, ct);
        var hoje  = _relogio.Hoje;

        var query = lista.AsEnumerable();
        if (categoriaId.HasValue)
            query = query.Where(i => i.CategoriaId == categoriaId.Value);

        return query.Select(i => ToResponse(i, hoje)).ToList();
    }

    public async Task<ItemResponse> CriarAsync(Guid usuarioId, CriarItemRequest req, CancellationToken ct)
    {
        var agora = _relogio.Agora;
        var hoje  = _relogio.Hoje;

        // Buscar veículo se necessário
        int? kmAtual = null;
        int? mediaKm = null;
        if (req.VeiculoId.HasValue)
        {
            var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(req.VeiculoId.Value, usuarioId, ct)
                ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");
            kmAtual = veiculo.KmAtual;
            mediaKm = veiculo.MediaKmMes;
        }

        var item = new Item(
            usuarioId,
            req.CategoriaId,
            req.Nome,
            req.TipoVencimento,
            req.DataBase,
            req.DataVencimento,
            req.IntervaloDias,
            req.IntervaloKm,
            req.KmBase,
            req.GarantiaFabricanteMeses,
            req.AntecedenciaDias,
            req.ModeloItemId,
            req.VeiculoId,
            req.Observacao,
            agora,
            kmAtual,
            mediaKm);

        // RN14 — copiar passos do modelo se fornecido
        if (req.ModeloItemId.HasValue)
        {
            var modelo = await _modelos.ObterPorIdAsync(req.ModeloItemId.Value, ct);
            if (modelo is not null)
                foreach (var passo in modelo.Passos.OrderBy(p => p.Ordem))
                    item.AdicionarPasso(passo.Descricao);
        }

        // Passos customizados
        if (req.Passos is { Count: > 0 })
            foreach (var desc in req.Passos)
                item.AdicionarPasso(desc);

        await _itens.AdicionarAsync(item, ct);
        await _itens.SalvarAsync(ct);
        return ToResponse(item, hoje);
    }

    public async Task<ItemResponse> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var item = await _itens.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");
        return ToResponse(item, _relogio.Hoje);
    }

    public async Task<ItemResponse> EditarAsync(Guid id, Guid usuarioId, EditarItemRequest req, CancellationToken ct)
    {
        var item = await _itens.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");

        int? kmAtual = null;
        int? mediaKm = null;
        if (req.VeiculoId.HasValue)
        {
            var veiculo = await _veiculos.ObterPorIdEUsuarioAsync(req.VeiculoId.Value, usuarioId, ct)
                ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");
            kmAtual = veiculo.KmAtual;
            mediaKm = veiculo.MediaKmMes;
        }

        item.Editar(
            req.Nome,
            req.CategoriaId,
            req.DataBase,
            req.DataVencimento,
            req.IntervaloDias,
            req.IntervaloKm,
            req.KmBase,
            req.GarantiaFabricanteMeses,
            req.AntecedenciaDias,
            req.VeiculoId,
            req.Observacao,
            _relogio.Agora,
            kmAtual,
            mediaKm);

        await _itens.SalvarAsync(ct);
        return ToResponse(item, _relogio.Hoje);
    }

    public async Task ExcluirAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var item = await _itens.ObterPorIdEUsuarioAsync(id, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");
        await _itens.ExcluirAsync(item, ct);
    }

    public async Task<ItemResponse> MarcarPassoAsync(
        Guid itemId, Guid passoId, Guid usuarioId, bool concluido, CancellationToken ct)
    {
        var item = await _itens.ObterPorIdEUsuarioAsync(itemId, usuarioId, ct)
            ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Item não encontrado.");

        var passo = item.Passos.FirstOrDefault(p => p.Id == passoId)
            ?? throw new NotFoundException("PASSO_NAO_ENCONTRADO", "Passo não encontrado.");

        passo.Marcar(concluido, _relogio.Agora);
        await _itens.SalvarAsync(ct);
        return ToResponse(item, _relogio.Hoje);
    }

    // ── Mapeamento ────────────────────────────────────────────────────────────

    private static ItemResponse ToResponse(Item item, DateOnly hoje) => new(
        item.Id,
        item.UsuarioId,
        item.CategoriaId,
        item.ModeloItemId,
        item.VeiculoId,
        item.Nome,
        item.TipoVencimento,
        item.DataBase,
        item.DataVencimento,
        item.IntervaloDias,
        item.IntervaloKm,
        item.KmBase,
        item.GarantiaFabricanteMeses,
        item.AntecedenciaDias,
        item.ProximoVencimentoData,
        item.ProximoVencimentoKm,
        item.Situacao,
        item.CalcularStatus(hoje),
        item.Observacao,
        item.CriadoEm,
        item.AtualizadoEm,
        item.Passos
            .OrderBy(p => p.Ordem)
            .Select(p => new PassoResponse(p.Id, p.Ordem, p.Descricao, p.Concluido, p.ConcluidoEm))
            .ToList());
}
