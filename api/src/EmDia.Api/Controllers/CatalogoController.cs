using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>
/// E08: GET /api/v1/catalogo/categorias  (público)
/// E09: GET /api/v1/catalogo/modelos     (público)
/// </summary>
[ApiController]
[Route("api/v1/catalogo")]
public class CatalogoController : ControllerBase
{
    private readonly CatalogoService _catalogo;

    public CatalogoController(CatalogoService catalogo) => _catalogo = catalogo;

    // ── E08: Listar categorias ────────────────────────────────────────────────
    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias(CancellationToken ct)
    {
        var lista = await _catalogo.ListarCategoriasAsync(ct);
        return Ok(lista);
    }

    // ── E09: Listar modelos ───────────────────────────────────────────────────
    [HttpGet("modelos")]
    public async Task<IActionResult> ListarModelos(
        [FromQuery] short? categoriaId, CancellationToken ct)
    {
        var lista = await _catalogo.ListarModelosAsync(categoriaId, ct);
        return Ok(lista);
    }
}
