using System.Security.Claims;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>
/// E13: GET    /api/v1/veiculos
/// E14: POST   /api/v1/veiculos
/// E15: GET    /api/v1/veiculos/{id}
/// E16: PUT    /api/v1/veiculos/{id}
/// E17: DELETE /api/v1/veiculos/{id}
/// </summary>
[ApiController]
[Route("api/v1/veiculos")]
[Authorize]
public class VeiculosController : ControllerBase
{
    private readonly VeiculoService _veiculos;

    public VeiculosController(VeiculoService veiculos) => _veiculos = veiculos;

    private Guid UsuarioIdLogado =>
        Guid.Parse(User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim sub ausente."));

    // ── E13: Listar ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _veiculos.ListarAsync(UsuarioIdLogado, ct));

    // ── E14: Criar ────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarVeiculoRequest req, CancellationToken ct)
    {
        var resp = await _veiculos.CriarAsync(UsuarioIdLogado, req, ct);
        return Created($"/api/v1/veiculos/{resp.Id}", resp);
    }

    // ── E15: Obter ────────────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
        => Ok(await _veiculos.ObterAsync(id, UsuarioIdLogado, ct));

    // ── E16: Editar ───────────────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarVeiculoRequest req, CancellationToken ct)
        => Ok(await _veiculos.EditarAsync(id, UsuarioIdLogado, req, ct));

    // ── E17: Excluir ──────────────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await _veiculos.ExcluirAsync(id, UsuarioIdLogado, ct);
        return NoContent();
    }
}
