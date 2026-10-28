using EmDia.Api.Extensions;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmDia.Api.Controllers;

/// <summary>
/// E01–E04 · Autenticação. No navegador, o token de renovação vai só no cookie HttpOnly (spec §4.3).
/// O app nativo (cabeçalho <c>X-Cliente: mobile</c>) não tem cookies de navegador: recebe o token de renovação
/// no corpo, guarda no armazenamento seguro do aparelho e o reenvia no cabeçalho <c>X-Refresh-Token</c> (DV17).
/// </summary>
[Route("api/v1/auth")]
public class AuthController(AuthService auth, IWebHostEnvironment env) : ApiControllerBase
{
    private bool ClienteMobile =>
        string.Equals(Request.Headers["X-Cliente"], "mobile", StringComparison.OrdinalIgnoreCase);

    private string? RefreshRecebido =>
        Request.Cookies[CookieRefreshExtensions.NomeCookieRefresh] ?? (ClienteMobile ? Request.Headers["X-Refresh-Token"].ToString() : null);

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
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var sessao = await auth.LoginAsync(req, ct);
        if (ClienteMobile)
            return Ok(new LoginMobileResponse(sessao.Resposta.AccessToken, sessao.Resposta.ExpiraEm, sessao.Resposta.Usuario, sessao.RefreshToken));

        Response.EmitirCookieRefresh(sessao.RefreshToken, sessao.RefreshExpiraEm, env);
        return Ok(sessao.Resposta);
    }

    /// <summary>E03 · Renovar o token de acesso usando o cookie de refresh.</summary>
    [HttpPost("refresh"), AllowAnonymous]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var sessao = await auth.RefreshAsync(RefreshRecebido, ct);
        if (ClienteMobile)
            return Ok(new RefreshMobileResponse(sessao.Resposta.AccessToken, sessao.Resposta.ExpiraEm, sessao.RefreshToken));

        Response.EmitirCookieRefresh(sessao.RefreshToken, sessao.RefreshExpiraEm, env);
        return Ok(sessao.Resposta);
    }

    /// <summary>E04 · Sair: revoga o refresh atual e apaga o cookie.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(RefreshRecebido, ct);
        Response.ApagarCookieRefresh(env);
        return NoContent();
    }
}

/// <summary>Login do app nativo: inclui o token de renovação (guardado no SecureStore do aparelho).</summary>
public record LoginMobileResponse(string AccessToken, DateTimeOffset ExpiraEm, UsuarioLogado Usuario, string RefreshToken);

/// <summary>Renovação do app nativo: novo token de acesso e novo token de renovação (rotação).</summary>
public record RefreshMobileResponse(string AccessToken, DateTimeOffset ExpiraEm, string RefreshToken);
