using EmDia.Application.DTOs.Auth;
using EmDia.Application.Services;
using EmDia.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmDia.Api.Controllers;

/// <summary>
/// E01: POST /api/v1/auth/registrar
/// E02: POST /api/v1/auth/login
/// E03: POST /api/v1/auth/refresh
/// E04: POST /api/v1/auth/logout
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly IWebHostEnvironment _env;

    public AuthController(AuthService auth, IWebHostEnvironment env)
    {
        _auth = auth;
        _env  = env;
    }

    /// <summary>
    /// Cookie seguro em produção; sem Secure em Testing/Development para http://localhost.
    /// </summary>
    private CookieOptions CookieOpts => new()
    {
        HttpOnly = true,
        Secure   = _env.EnvironmentName != "Testing" && _env.EnvironmentName != "Development",
        SameSite = SameSiteMode.Strict,
        Path     = "/api/v1/auth",
        MaxAge   = TimeSpan.FromDays(7)
    };

    // ── E01: Registrar ───────────────────────────────────────────────────────
    [HttpPost("registrar")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarRequest req, CancellationToken ct)
    {
        // Cria conta → faz login → devolve token + cookie
        await _auth.RegistrarAsync(req, ct);
        var (resposta, rt) = await _auth.LoginAsync(new LoginRequest(req.Email, req.Senha), ct);

        EmitirCookie(rt);
        return Created($"/api/v1/usuarios/{resposta.Id}", resposta);
    }

    // ── E02: Login ───────────────────────────────────────────────────────────
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req, CancellationToken ct)
    {
        var (resposta, rt) = await _auth.LoginAsync(req, ct);
        EmitirCookie(rt);
        return Ok(resposta);
    }

    // ── E03: Refresh ─────────────────────────────────────────────────────────
    [HttpPost("refresh")]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var tokenCru)
            || string.IsNullOrEmpty(tokenCru))
            return Unauthorized(new { code = "REFRESH_AUSENTE", detail = "Cookie refresh_token ausente." });

        var (resposta, novoRt) = await _auth.RefreshAsync(tokenCru, ct);
        EmitirCookie(novoRt);
        return Ok(resposta);
    }

    // ── E04: Logout ──────────────────────────────────────────────────────────
    [HttpPost("logout")]
    [DisableRateLimiting]  // logout nunca deve ser bloqueado por rate limit
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("refresh_token", out var tokenCru)
            && !string.IsNullOrEmpty(tokenCru))
            await _auth.LogoutAsync(tokenCru, ct);

        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure   = _env.EnvironmentName != "Testing" && _env.EnvironmentName != "Development",
            SameSite = SameSiteMode.Strict,
            Path     = "/api/v1/auth"
        });
        return NoContent();
    }

    // ── Auxiliar ─────────────────────────────────────────────────────────────
    private void EmitirCookie(EmDia.Domain.Entities.RefreshToken rt)
    {
        var valor = rt is RefreshTokenComValorCru rtc ? rtc.ValorCru : rt.TokenHash;
        Response.Cookies.Append("refresh_token", valor, CookieOpts);
    }
}
