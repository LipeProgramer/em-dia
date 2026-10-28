using EmDia.Application.DTOs.Itens;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using FluentValidation;

namespace EmDia.Application.Services;

/// <summary>Itens, conclusão e passos (E13–E19).</summary>
public class ItemService(
    IItemRepository itens,
    IVeiculoRepository veiculos,
    ICatalogoRepository catalogo,
    IHistoricoRepository historicos,
    IUnitOfWork uow,
    IRelogio relogio,
    IValidator<SalvarItemRequest> validadorItem,
    IValidator<ConcluirRequest> validadorConcluir,
    IValidator<MarcarPassoRequest> validadorPasso)
{
    private const int HistoricoRecente = 5;

    /// <summary>E13: lista os itens do usuário, com filtros opcionais por categoria e status.</summary>
    public async Task<IReadOnlyList<ItemResumoResponse>> ListarAsync(
        Guid usuarioId, short? categoriaId, string? status, CancellationToken ct)
    {
        if (categoriaId is < 1 or > 6)
            throw new CampoInvalidoException("categoriaId", "Escolha uma categoria entre 1 e 6.");

        StatusItem? filtroStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusItem>(status, ignoreCase: true, out var s) || int.TryParse(status, out _))
                throw new CampoInvalidoException("status", "Use Vencido, Atencao, EmDia ou Encerrado.");
            filtroStatus = s;
        }

        var hoje = relogio.Hoje;
        var porId = await VeiculosDoUsuarioAsync(usuarioId, ct);

        return (await itens.ListarPorUsuarioAsync(usuarioId, ct))
            .Where(i => categoriaId is null || i.CategoriaId == categoriaId)
            .Select(i => Mapeamento.Resumo(i, hoje, VeiculoDo(i, porId)))
            .Where(r => filtroStatus is null || r.Status == filtroStatus)
            .OrderBy(r => r.Status == StatusItem.Encerrado)
            .ThenBy(r => r.DiasRestantes ?? int.MaxValue)
            .ToList();
    }

    /// <summary>E14: detalhe do item com passos e histórico recente.</summary>
    public async Task<ItemDetalheResponse> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var item = await ObterItemAsync(id, usuarioId, ct);
        return await DetalheAsync(item, usuarioId, ct);
    }

    /// <summary>E15: cria um item a partir de um modelo ou do zero (RN01, RN02, RN14).</summary>
    public async Task<ItemDetalheResponse> CriarAsync(Guid usuarioId, SalvarItemRequest req, CancellationToken ct)
    {
        await validadorItem.ValidateAndThrowAsync(req, ct);
        await GarantirVeiculoAsync(req, usuarioId, ct);

        ModeloItem? modelo = null;
        if (req.ModeloId is { } modeloId)
            modelo = await catalogo.ObterModeloAsync(modeloId, ct)
                ?? throw new NotFoundException("MODELO_NAO_ENCONTRADO", "Não encontramos este modelo.");

        var item = new Item(usuarioId, req.CategoriaId, req.Nome, req.TipoVencimento, req.DataBase, req.DataVencimento,
            req.IntervaloDias, req.IntervaloKm, req.KmBase, req.GarantiaFabricanteMeses, req.AntecedenciaDias,
            modelo?.Id, req.VeiculoId, req.Observacao, relogio.Agora);

        // RN14: passos do modelo são copiados; se o usuário enviou passos (editados), valem os dele
        var passos = req.Passos ?? modelo?.Passos.OrderBy(p => p.Ordem).Select(p => p.Descricao).ToList() ?? [];
        foreach (var p in passos)
            item.AdicionarPasso(p);

        await itens.AdicionarAsync(item, ct);
        await uow.SalvarAsync(ct);
        return await DetalheAsync(item, usuarioId, ct);
    }

    /// <summary>E16: edita o item e recalcula o próximo vencimento (RN01). Item encerrado não pode ser editado.</summary>
    public async Task<ItemDetalheResponse> EditarAsync(Guid id, Guid usuarioId, SalvarItemRequest req, CancellationToken ct)
    {
        await validadorItem.ValidateAndThrowAsync(req, ct);
        var item = await ObterItemAsync(id, usuarioId, ct);
        await GarantirVeiculoAsync(req, usuarioId, ct);

        item.Editar(req.Nome, req.CategoriaId, req.TipoVencimento, req.DataBase, req.DataVencimento, req.IntervaloDias,
            req.IntervaloKm, req.KmBase, req.GarantiaFabricanteMeses, req.AntecedenciaDias, req.VeiculoId,
            req.Observacao, relogio.Agora);

        if (req.Passos is not null)
            item.SubstituirPassos(req.Passos);

        await uow.SalvarAsync(ct);
        return await DetalheAsync(item, usuarioId, ct);
    }

    /// <summary>E17: exclui o item; passos, histórico e avisos vão junto (CASCADE).</summary>
    public async Task ExcluirAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var item = await ObterItemAsync(id, usuarioId, ct);
        itens.Remover(item);
        await uow.SalvarAsync(ct);
    }

    /// <summary>E18: conclui e renova (RN05, RN06, RN07, RN15). Tudo em uma transação.</summary>
    public async Task<ConcluirResponse> ConcluirAsync(Guid id, Guid usuarioId, ConcluirRequest req, CancellationToken ct)
    {
        await validadorConcluir.ValidateAndThrowAsync(req, ct);
        var item = await ObterItemAsync(id, usuarioId, ct);
        var veiculo = item.VeiculoId is { } vid ? await veiculos.ObterAsync(vid, usuarioId, ct) : null;

        var resultado = item.Concluir(req.DataConclusao!.Value, req.Km, req.ValorGasto, req.Observacao,
            req.ConfirmarPassosPendentes, req.NovaDataVencimento, veiculo, relogio.Agora, relogio.Hoje);

        await uow.SalvarAsync(ct);
        return new ConcluirResponse(await DetalheAsync(item, usuarioId, ct), resultado.Historico.Id, resultado.Mensagem);
    }

    /// <summary>E19: marca ou desmarca um passo do item.</summary>
    public async Task<MarcarPassoResponse> MarcarPassoAsync(
        Guid id, Guid passoId, Guid usuarioId, MarcarPassoRequest req, CancellationToken ct)
    {
        await validadorPasso.ValidateAndThrowAsync(req, ct);
        var item = await ObterItemAsync(id, usuarioId, ct);

        var passo = item.MarcarPasso(passoId, req.Concluido!.Value, relogio.Agora);
        await uow.SalvarAsync(ct);

        return new MarcarPassoResponse(passo.Id, passo.Concluido, item.ProgressoPassos());
    }

    // ── Auxiliares ───────────────────────────────────────────────────────────

    private async Task<Item> ObterItemAsync(Guid id, Guid usuarioId, CancellationToken ct) =>
        await itens.ObterAsync(id, usuarioId, ct)
        ?? throw new NotFoundException("ITEM_NAO_ENCONTRADO", "Não encontramos este item.");

    /// <summary>RN12: veículo de outra pessoa responde 404, como se não existisse.</summary>
    private async Task GarantirVeiculoAsync(SalvarItemRequest req, Guid usuarioId, CancellationToken ct)
    {
        if (req.VeiculoId is { } vid && await veiculos.ObterAsync(vid, usuarioId, ct) is null)
            throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Não encontramos este veículo.");
    }

    private async Task<ItemDetalheResponse> DetalheAsync(Item item, Guid usuarioId, CancellationToken ct)
    {
        var veiculo = item.VeiculoId is { } vid ? await veiculos.ObterAsync(vid, usuarioId, ct) : null;
        var recentes = await historicos.ListarAsync(
            new FiltroHistorico(usuarioId, item.Id, null, null, HistoricoRecente), ct);
        return Mapeamento.Detalhe(item, relogio.Hoje, veiculo, recentes);
    }

    private async Task<Dictionary<Guid, Veiculo>> VeiculosDoUsuarioAsync(Guid usuarioId, CancellationToken ct) =>
        (await veiculos.ListarPorUsuarioAsync(usuarioId, ct)).ToDictionary(v => v.Id);

    private static Veiculo? VeiculoDo(Item i, Dictionary<Guid, Veiculo> porId) =>
        i.VeiculoId is { } id ? porId.GetValueOrDefault(id) : null;
}
