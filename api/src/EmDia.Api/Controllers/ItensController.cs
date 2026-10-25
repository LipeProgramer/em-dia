using System.Security.Claims;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>
/// E19: GET    /api/v1/itens
/// E20: POST   /api/v1/itens
/// E21: GET    /api/v1/itens/{id}
/// E22: PUT    /api/v1/itens/{id}
/// E23: DELETE /api/v1/itens/{id}
/// E24: PATCH  /api/v1/itens/{id}/passos/{passoId}
/// E25: POST   /api/v1/itens/{id}/concluir
/// E26: GET    /api/v1/itens/{id}/historico
/// </summary>
[ApiController]
[Route("api/v1/itens")]
[Authorize]
public class ItensController : ControllerBase
{
    private readonly ItemService _itens;
    private readonly ConcluirService _concluir;

    public ItensController(ItemService itens, ConcluirService concluir)
    {
        _itens    = itens;
        _concluir = concluir;
    }

    private Guid UsuarioIdLogado =>
        Guid.Parse(User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim sub ausente."));

    // ── E19: Listar ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] short? categoriaId, CancellationToken ct)
        => Ok(await _itens.ListarAsync(UsuarioIdLogado, categoriaId, ct));

    // ── E20: Criar ────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarItemRequest req, CancellationToken ct)
    {
        var resp = await _itens.CriarAsync(UsuarioIdLogado, req, ct);
        return Created($"/api/v1/itens/{resp.Id}", resp);
    }

    // ── E21: Obter ────────────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
        => Ok(await _itens.ObterAsync(id, UsuarioIdLogado, ct));

    // ── E22: Editar ───────────────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id, [FromBody] EditarItemRequest req, CancellationToken ct)
        => Ok(await _itens.EditarAsync(id, UsuarioIdLogado, req, ct));

    // ── E23: Excluir ──────────────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await _itens.ExcluirAsync(id, UsuarioIdLogado, ct);
        return NoContent();
    }

    // ── E24: Marcar passo ─────────────────────────────────────────────────────
    [HttpPatch("{id:guid}/passos/{passoId:guid}")]
    public async Task<IActionResult> MarcarPasso(
        Guid id, Guid passoId, [FromBody] MarcarPassoRequest req, CancellationToken ct)
        => Ok(await _itens.MarcarPassoAsync(id, passoId, UsuarioIdLogado, req.Concluido, ct));

    // ── E25: Concluir item ────────────────────────────────────────────────────
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Concluir(
        Guid id, [FromBody] ConcluirItemRequest req, CancellationToken ct)
        => Ok(await _concluir.ConcluirAsync(id, UsuarioIdLogado, req, ct));

    // ── E26: Histórico do item ────────────────────────────────────────────────
    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Historico(Guid id, CancellationToken ct)
        => Ok(await _concluir.ListarHistoricoAsync(id, UsuarioIdLogado, ct));
}
