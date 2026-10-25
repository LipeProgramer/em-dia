using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.Interfaces.Repositories;

namespace EmDia.Application.Services;

/// <summary>
/// E08: GET /api/v1/catalogo/categorias
/// E09: GET /api/v1/catalogo/modelos
/// </summary>
public class CatalogoService
{
    private readonly ICategoriaRepository _categorias;
    private readonly IModeloItemRepository _modelos;

    public CatalogoService(ICategoriaRepository categorias, IModeloItemRepository modelos)
    {
        _categorias = categorias;
        _modelos    = modelos;
    }

    public async Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync(CancellationToken ct)
    {
        var lista = await _categorias.ListarAsync(ct);
        return lista
            .OrderBy(c => c.Ordem)
            .Select(c => new CategoriaResponse(c.Id, c.Nome, c.Icone, c.Ordem))
            .ToList();
    }

    public async Task<IReadOnlyList<ModeloResponse>> ListarModelosAsync(
        short? categoriaId, CancellationToken ct)
    {
        var lista = await _modelos.ListarAsync(ct);

        var query = lista.Where(m => m.Ativo);
        if (categoriaId.HasValue)
            query = query.Where(m => m.CategoriaId == categoriaId.Value);

        return query
            .OrderBy(m => m.CategoriaId)
            .ThenBy(m => m.Nome)
            .Select(m => new ModeloResponse(
                m.Id,
                m.CategoriaId,
                m.Nome,
                m.TipoVencimento,
                m.IntervaloDias,
                m.IntervaloKm,
                m.GarantiaFabricanteMeses,
                m.AntecedenciaDias,
                m.Passos
                    .OrderBy(p => p.Ordem)
                    .Select(p => new PassoModeloResponse(p.Ordem, p.Descricao))
                    .ToList()))
            .ToList();
    }
}
