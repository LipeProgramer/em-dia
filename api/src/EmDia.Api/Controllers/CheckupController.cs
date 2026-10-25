using EmDia.Application.DTOs.Checkup;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmDia.Api.Controllers;

/// <summary>
/// E11: POST /api/v1/checkup       – executa check-up (RN11/RN14/RN15)
/// E12: GET  /api/v1/checkup/atual – retorna último check-up
/// </summary>
[ApiController]
[Route("api/v1/checkup")]
[Authorize]
public class CheckupController : ControllerBase
{
    private readonly CheckupService _checkup;

    public CheckupController(CheckupService checkup) => _checkup = checkup;

    private Guid UsuarioIdLogado =>
        Guid.Parse(User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim sub ausente."));

    // ── E11: Executar checkup ─────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Executar(
        [FromBody] ExecutarCheckupRequest req, CancellationToken ct)
        => Ok(await _checkup.ExecutarAsync(UsuarioIdLogado, req, ct));

    // ── E12: Obter checkup atual ──────────────────────────────────────────────
    [HttpGet("atual")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ObterAtual(CancellationToken ct)
    {
        var resultado = await _checkup.ObterAtualAsync(UsuarioIdLogado, ct);
        return resultado is null ? NoContent() : Ok(resultado);
    }
}
