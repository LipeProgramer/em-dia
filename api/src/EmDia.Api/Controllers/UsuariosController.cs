using System.Security.Claims;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EmDia.Api.Controllers;

/// <summary>
/// E05: GET    /api/v1/usuarios/me
/// E06: PATCH  /api/v1/usuarios/me
/// E07: DELETE /api/v1/usuarios/me
/// </summary>
[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly AuthService _auth;

    public UsuariosController(AuthService auth) => _auth = auth;

    private Guid UsuarioIdLogado =>
        Guid.Parse(
            User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Claim sub ausente."));

    // ── E05: Obter perfil ────────────────────────────────────────────────────
    [HttpGet("me")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPerfil(CancellationToken ct)
    {
        var resp = await _auth.ObterPerfilAsync(UsuarioIdLogado, ct);
        return Ok(resp);
    }

    // ── E06: Alterar nome ────────────────────────────────────────────────────
    [HttpPatch("me")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarNome(
        [FromBody] AlterarNomeRequest req, CancellationToken ct)
    {
        var resp = await _auth.AlterarNomeAsync(UsuarioIdLogado, req, ct);
        return Ok(resp);
    }

    // ── E07: Excluir conta ───────────────────────────────────────────────────
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ExcluirConta(CancellationToken ct)
    {
        await _auth.ExcluirContaAsync(UsuarioIdLogado, ct);

        // Limpar cookie de refresh
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/api/v1/auth"
        });

        return NoContent();
    }
}
