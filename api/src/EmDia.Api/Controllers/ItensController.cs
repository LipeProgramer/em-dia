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

/// <summary>E13–E19 · Itens.</summary>
[Route("api/v1/itens")]
public class ItensController(ItemService itens) : ApiControllerBase
{
    /// <summary>E13 · Listar itens (filtros opcionais: categoriaId, status).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ItemResumoResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IReadOnlyList<ItemResumoResponse>> Listar([FromQuery] short? categoriaId, [FromQuery] string? status,
        CancellationToken ct) => itens.ListarAsync(UsuarioId, categoriaId, status, ct);

    /// <summary>E14 · Detalhe com passos e histórico recente.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ItemDetalheResponse> Obter(Guid id, CancellationToken ct) => itens.ObterAsync(id, UsuarioId, ct);

    /// <summary>E15 · Criar item (a partir de um modelo ou do zero).</summary>
    [HttpPost]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar(SalvarItemRequest req, CancellationToken ct)
    {
        var item = await itens.CriarAsync(UsuarioId, req, ct);
        return Created($"/api/v1/itens/{item.Id}", item);
    }

    /// <summary>E16 · Editar item.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ItemDetalheResponse> Editar(Guid id, SalvarItemRequest req, CancellationToken ct) =>
        itens.EditarAsync(id, UsuarioId, req, ct);

    /// <summary>E17 · Excluir item.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await itens.ExcluirAsync(id, UsuarioId, ct);
        return NoContent();
    }

    /// <summary>E18 · Concluir e renovar.</summary>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType<ConcluirResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ConcluirResponse> Concluir(Guid id, ConcluirRequest req, CancellationToken ct) =>
        itens.ConcluirAsync(id, UsuarioId, req, ct);

    /// <summary>E19 · Marcar ou desmarcar um passo.</summary>
    [HttpPatch("{id:guid}/passos/{passoId:guid}")]
    [ProducesResponseType<MarcarPassoResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<MarcarPassoResponse> MarcarPasso(Guid id, Guid passoId, MarcarPassoRequest req, CancellationToken ct) =>
        itens.MarcarPassoAsync(id, passoId, UsuarioId, req, ct);
}
