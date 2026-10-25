using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmDia.Api.Controllers;

/// <summary>
/// E10: GET /api/v1/radar – radar de vencimentos com anéis (RN03/RN04)
/// </summary>
[ApiController]
[Route("api/v1/radar")]
[Authorize]
public class RadarController : ControllerBase
{
    private readonly RadarService _radar;

    public RadarController(RadarService radar) => _radar = radar;

    private Guid UsuarioIdLogado =>
        Guid.Parse(User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim sub ausente."));

    // ── E10: Obter radar ──────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterRadar(CancellationToken ct)
        => Ok(await _radar.ObterAsync(UsuarioIdLogado, ct));
}
