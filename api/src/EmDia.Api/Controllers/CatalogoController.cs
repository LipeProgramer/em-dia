using System.Security.Claims;
using EmDia.Api.Extensions;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Historico;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmDia.Api.Controllers;

/// <summary>E08–E11 · Catálogo e check-up.</summary>
[Route("api/v1")]
public class CatalogoController(CatalogoService catalogo, CheckupService checkup) : ApiControllerBase
{
    /// <summary>E08 · Listar as 6 categorias do Radar.</summary>
    [HttpGet("categorias")]
    public Task<IReadOnlyList<CategoriaResponse>> Categorias(CancellationToken ct) => catalogo.ListarCategoriasAsync(ct);

    /// <summary>E09 · Listar modelos prontos (só ativos).</summary>
    [HttpGet("modelos")]
    [ProducesResponseType<IReadOnlyList<ModeloResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IReadOnlyList<ModeloResponse>> Modelos([FromQuery] short? categoriaId, CancellationToken ct) =>
        catalogo.ListarModelosAsync(categoriaId, ct);

    /// <summary>E10 · Perguntas do check-up.</summary>
    [HttpGet("checkup/perguntas")]
    public Task<IReadOnlyList<PerguntaResponse>> Perguntas(CancellationToken ct) => catalogo.ListarPerguntasAsync(ct);

    /// <summary>E11 · Fazer o check-up e criar os itens de uma vez.</summary>
    [HttpPost("checkup")]
    [ProducesResponseType<CheckupResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Checkup(CheckupRequest req, CancellationToken ct)
    {
        var resposta = await checkup.RealizarAsync(UsuarioId, req, ct);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }
}
