using System.Security.Cryptography;
using System.Text;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// Orquestra autenticação: registro, login, refresh, logout, exclusão de conta.
/// Toda regra de negócio de acesso fica aqui (não nos controllers).
/// </summary>
public class AuthService
{
    // Constantes de segurança (spec §4.3)
    private const int MaxFalhasLogin = 5;
    private const int BloqueioMinutos = 15;
    private const int DiasRefreshToken = 7;

    private readonly IUsuarioRepository _usuarios;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;
    private readonly IRelogio _relogio;

    public AuthService(
        IUsuarioRepository usuarios,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher,
        ITokenService tokenService,
        IRelogio relogio)
    {
        _usuarios = usuarios;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
        _tokenService = tokenService;
        _relogio = relogio;
    }

    // ── E01: Registrar ───────────────────────────────────────────────────────
    public async Task<(LoginResponse Resposta, RefreshToken RefreshToken)> RegistrarAsync(
        RegistrarRequest req, CancellationToken ct = default)
    {
        var existe = await _usuarios.ObterPorEmailAsync(req.Email, ct);
        if (existe is not null)
            throw new ConflictException("EMAIL_DUPLICADO", "Já existe uma conta com este e-mail.");

        var hash = _hasher.Hash(req.Senha);
        var usuario = new Usuario(req.Nome, req.Email, hash, _relogio.Agora);

        await _usuarios.AdicionarAsync(usuario, ct);
        await _usuarios.SalvarAsync(ct);

        var accessToken = _tokenService.GerarAccessToken(usuario);
        var refreshToken = _tokenService.GerarRefreshToken(usuario.Id, _relogio.Agora);

        await _refreshTokens.AdicionarAsync(refreshToken, ct);
        await _refreshTokens.SalvarAsync(ct);

        var resposta = new LoginResponse(usuario.Id, usuario.Nome, usuario.Email, accessToken, usuario.PrecisaCheckup);
        return (resposta, refreshToken);
    }

    // ── E02: Login ───────────────────────────────────────────────────────────
    public async Task<(LoginResponse Resposta, RefreshToken RefreshToken)> LoginAsync(
        LoginRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterPorEmailAsync(req.Email, ct);

        // Sempre retornar 401 genérico — não revelar se e-mail existe ou não
        if (usuario is null)
            throw new UnauthorizedException("CREDENCIAIS_INVALIDAS", "E-mail ou senha incorretos.");

        if (usuario.EstaBloqueado(_relogio.Agora))
            throw new UnauthorizedException("CONTA_BLOQUEADA",
                $"Conta bloqueada por excesso de tentativas. Tente novamente mais tarde.");

        if (!_hasher.Verificar(req.Senha, usuario.SenhaHash))
        {
            usuario.RegistrarFalhaLogin(_relogio.Agora, MaxFalhasLogin, BloqueioMinutos);
            await _usuarios.SalvarAsync(ct);
            throw new UnauthorizedException("CREDENCIAIS_INVALIDAS", "E-mail ou senha incorretos.");
        }

        usuario.RegistrarLoginComSucesso();
        await _usuarios.SalvarAsync(ct);

        var accessToken = _tokenService.GerarAccessToken(usuario);
        var refreshToken = _tokenService.GerarRefreshToken(usuario.Id, _relogio.Agora);

        await _refreshTokens.AdicionarAsync(refreshToken, ct);
        await _refreshTokens.SalvarAsync(ct);

        var resposta = new LoginResponse(usuario.Id, usuario.Nome, usuario.Email, accessToken, usuario.PrecisaCheckup);
        return (resposta, refreshToken);
    }

    // ── E03: Refresh ─────────────────────────────────────────────────────────
    public async Task<(RefreshResponse Resposta, RefreshToken NovoRefreshToken)> RefreshAsync(
        string tokenCru, CancellationToken ct = default)
    {
        var hash = ComputarSHA256(tokenCru);
        var rt = await _refreshTokens.ObterPorHashAsync(hash, ct);

        if (rt is null || !rt.EstaAtivo)
            throw new UnauthorizedException("REFRESH_INVALIDO", "Token de renovação inválido ou expirado.");

        var usuario = await _usuarios.ObterPorIdAsync(rt.UsuarioId, ct);
        if (usuario is null)
            throw new UnauthorizedException("REFRESH_INVALIDO", "Token de renovação inválido.");

        // Revogar token antigo
        rt.Revogar(_relogio.Agora);
        await _refreshTokens.SalvarAsync(ct);

        // Emitir novos tokens (rotação)
        var novoAccessToken = _tokenService.GerarAccessToken(usuario);
        var novoRefreshToken = _tokenService.GerarRefreshToken(usuario.Id, _relogio.Agora);

        await _refreshTokens.AdicionarAsync(novoRefreshToken, ct);
        await _refreshTokens.SalvarAsync(ct);

        return (new RefreshResponse(novoAccessToken), novoRefreshToken);
    }

    // ── E04: Logout ──────────────────────────────────────────────────────────
    public async Task LogoutAsync(string tokenCru, CancellationToken ct = default)
    {
        var hash = ComputarSHA256(tokenCru);
        var rt = await _refreshTokens.ObterPorHashAsync(hash, ct);

        if (rt is not null && rt.EstaAtivo)
        {
            rt.Revogar(_relogio.Agora);
            await _refreshTokens.SalvarAsync(ct);
        }
        // Se o token não existe ou já foi revogado, retornar 204 silenciosamente
    }

    // ── E05: Obter perfil ────────────────────────────────────────────────────
    public async Task<UsuarioResponse> ObterPerfilAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.CriadoEm, usuario.PrecisaCheckup);
    }

    // ── E06: Alterar nome ────────────────────────────────────────────────────
    public async Task<UsuarioResponse> AlterarNomeAsync(
        Guid usuarioId, AlterarNomeRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        usuario.AlterarNome(req.Nome);
        await _usuarios.SalvarAsync(ct);

        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.CriadoEm, usuario.PrecisaCheckup);
    }

    // ── E07: Excluir conta ───────────────────────────────────────────────────
    public async Task ExcluirContaAsync(Guid usuarioId, CancellationToken ct = default)
    {
        // Cascata no banco destrói tudo (RF18/LGPD)
        await _usuarios.ExcluirAsync(usuarioId, ct);
    }

    // ── Auxiliar ─────────────────────────────────────────────────────────────
    public static string ComputarSHA256(string valor)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
