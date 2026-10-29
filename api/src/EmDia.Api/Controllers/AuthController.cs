using EmDia.Api.Extensions;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace EmDia.Api.Controllers;

/// <summary>
/// E01–E04 · Autenticação. No navegador, o token de renovação vai só no cookie HttpOnly (spec §4.3).
/// O app nativo (cabeçalho <c>X-Cliente: mobile</c>) não tem cookies de navegador: recebe o token de renovação
/// no corpo, guarda no armazenamento seguro do aparelho e o reenvia no cabeçalho <c>X-Refresh-Token</c> (DV17).
/// Nas rotas ".../rapido" (DV22), a senha vai no cabeçalho <c>senha</c>, nunca no endereço: endereços ficam
/// gravados em logs e no histórico do navegador.
/// </summary>
[Route("api/v1/auth")]
public class AuthController(AuthService auth, IWebHostEnvironment env) : ApiControllerBase
{
    private bool ClienteMobile =>
        string.Equals(Request.Headers["X-Cliente"], "mobile", StringComparison.OrdinalIgnoreCase);

    private string? RefreshRecebido =>
        Request.Cookies[CookieRefreshExtensions.NomeCookieRefresh] ?? (ClienteMobile ? Request.Headers["X-Refresh-Token"].ToString() : null);

    /// <summary>E01 · Criar conta (corpo JSON).</summary>
    /// <remarks>Para testar sem JSON, use POST /api/v1/auth/registrar/rapido.</remarks>
    /// <param name="req">Nome, e-mail e senha. O exemplo já é válido (troque o e-mail se ele já existir).</param>
    /// <param name="ct"></param>
    /// <response code="201">Conta criada. Depois, entre pelo POST /api/v1/auth/login.</response>
    /// <response code="400">Campo inválido: nome de 2 a 100 caracteres, e-mail válido, senha de 8 a 64 com letra e número.</response>
    /// <response code="409">EMAIL_DUPLICADO: já existe uma conta com este e-mail.</response>
    [HttpPost("registrar"), AllowAnonymous]
    [ProducesResponseType<RegistrarResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(RegistrarRequest req, CancellationToken ct)
    {
        var resposta = await auth.RegistrarAsync(req, ct);
        return Created("/api/v1/usuarios/me", resposta);
    }

    /// <summary>E01 por parâmetros · Criar conta (sem JSON).</summary>
    /// <remarks>Ex.: nome = Demo Banca, email = demo.banca@emdia.dev, senha = Demo2026a.</remarks>
    /// <param name="nome">Seu nome (2 a 100 caracteres)</param>
    /// <param name="email">E-mail, usado para entrar</param>
    /// <param name="senha">Senha de 8 a 64 caracteres, com letra e número (vai no cabeçalho, não no endereço)</param>
    /// <param name="ct"></param>
    /// <response code="201">Conta criada. Depois, entre pelo POST /api/v1/auth/login/rapido.</response>
    /// <response code="400">Faltou um campo ("Campo obrigatório.") ou ele é inválido.</response>
    /// <response code="409">EMAIL_DUPLICADO: já existe uma conta com este e-mail.</response>
    [HttpPost("registrar/rapido"), AllowAnonymous]
    [ProducesResponseType<RegistrarResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<IActionResult> RegistrarPorParametros([FromQuery, BindRequired] string nome, [FromQuery, BindRequired] string email,
        [FromHeader(Name = "senha"), BindRequired] string senha, CancellationToken ct) =>
        Registrar(new RegistrarRequest(nome, email, senha), ct);

    /// <summary>E02 · Entrar (corpo JSON).</summary>
    /// <remarks>Limite de 5 tentativas por minuto por IP. Copie o "accessToken" da resposta e cole em Authorize (cadeado),
    /// sem a palavra Bearer. O token vale 15 minutos.</remarks>
    /// <param name="req">E-mail e senha. O exemplo é a conta do exemplo do cadastro.</param>
    /// <param name="ct"></param>
    /// <response code="200">Token de acesso, validade e dados básicos da pessoa.</response>
    /// <response code="400">Faltou o e-mail ou a senha.</response>
    /// <response code="401">CREDENCIAIS_INVALIDAS: e-mail ou senha incorretos.</response>
    /// <response code="429">MUITAS_TENTATIVAS: mais de 5 tentativas em 1 minuto.</response>
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

    /// <summary>E02 por parâmetros · Entrar (sem JSON).</summary>
    /// <remarks>Mesmo limite do E02 (5 por minuto por IP). Copie o "accessToken" e cole em Authorize (cadeado).</remarks>
    /// <param name="email">E-mail da conta</param>
    /// <param name="senha">Senha da conta (vai no cabeçalho, não no endereço)</param>
    /// <param name="ct"></param>
    /// <response code="200">Token de acesso, validade e dados básicos da pessoa.</response>
    /// <response code="400">Faltou o e-mail ou a senha ("Campo obrigatório.").</response>
    /// <response code="401">CREDENCIAIS_INVALIDAS: e-mail ou senha incorretos.</response>
    /// <response code="429">MUITAS_TENTATIVAS: mais de 5 tentativas em 1 minuto.</response>
    [HttpPost("login/rapido"), AllowAnonymous, EnableRateLimiting(RateLimitExtensions.PoliticaLogin)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest), ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public Task<IActionResult> LoginPorParametros([FromQuery, BindRequired] string email,
        [FromHeader(Name = "senha"), BindRequired] string senha, CancellationToken ct) =>
        Login(new LoginRequest(email, senha), ct);

    /// <summary>E03 · Renovar o token de acesso.</summary>
    /// <remarks>Usa o cookie de renovação gravado no login (no Swagger, funciona depois de um login na mesma aba).
    /// Cada renovação troca o cookie; reusar um token antigo encerra todas as sessões.</remarks>
    /// <response code="200">Novo token de acesso e validade.</response>
    /// <response code="401">REFRESH_INVALIDO ou REFRESH_REUTILIZADO: entre de novo.</response>
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

    /// <summary>E04 · Sair.</summary>
    /// <remarks>Revoga o token de renovação atual e apaga o cookie. O token de acesso some sozinho em até 15 minutos.</remarks>
    /// <response code="204">Sessão encerrada.</response>
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
