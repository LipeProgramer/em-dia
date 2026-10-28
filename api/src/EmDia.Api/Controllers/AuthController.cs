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

/// <summary>E01–E04 · Autenticação.</summary>
[Route("api/v1/auth")]
public class AuthController(AuthService auth, IWebHostEnvironment env) : ApiControllerBase
{
    /// <summary>E01 · Criar conta.</summary>
    [HttpPost("registrar"), AllowAnonymous]
    [ProducesResponseType<RegistrarResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(RegistrarRequest req, CancellationToken ct)
    {
        var resposta = await auth.RegistrarAsync(req, ct);
        return Created("/api/v1/usuarios/me", resposta);
    }

    /// <summary>E02 · Entrar (limite de 5 tentativas por minuto por IP).</summary>
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting(RateLimitExtensions.PoliticaLogin)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var sessao = await auth.LoginAsync(req, ct);
        Response.EmitirCookieRefresh(sessao.RefreshToken, sessao.RefreshExpiraEm, env);
        return sessao.Resposta;
    }

    /// <summary>E03 · Renovar o token de acesso usando o cookie de refresh.</summary>
    [HttpPost("refresh"), AllowAnonymous]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshResponse>> Refresh(CancellationToken ct)
    {
        var sessao = await auth.RefreshAsync(Request.Cookies[CookieRefreshExtensions.NomeCookieRefresh], ct);
        Response.EmitirCookieRefresh(sessao.RefreshToken, sessao.RefreshExpiraEm, env);
        return sessao.Resposta;
    }

    /// <summary>E04 · Sair: revoga o refresh atual e apaga o cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(Request.Cookies[CookieRefreshExtensions.NomeCookieRefresh], ct);
        Response.ApagarCookieRefresh(env);
        return NoContent();
    }
}
