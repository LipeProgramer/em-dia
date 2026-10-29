using EmDia.Application.DTOs.Auth;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EmDia.Application.Services;

/// <summary>Cadastro, login, renovação e saída (E01–E04 · spec §4).</summary>
public class AuthService(
    IUsuarioRepository usuarios,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    IPasswordHasher hasher,
    ITokenService tokens,
    IRelogio relogio,
    IOptions<SegurancaOptions> seguranca,
    IOptions<JwtOptions> jwt,
    IValidator<RegistrarRequest> validadorRegistro,
    IValidator<LoginRequest> validadorLogin)
{
    /// <summary>Mensagem genérica: não diz se o e-mail existe nem se a conta está bloqueada (spec §4.5).</summary>
    public const string MensagemLoginInvalido = "E-mail ou senha inválidos.";

    /// <summary>E01: cria a conta e as preferências padrão de aviso.</summary>
    public async Task<RegistrarResponse> RegistrarAsync(RegistrarRequest req, CancellationToken ct)
    {
        await validadorRegistro.ValidateAndThrowAsync(req, ct);

        if (await usuarios.EmailExisteAsync(Usuario.NormalizarEmail(req.Email), ct))
            throw new ConflictException(CodigosErro.EmailDuplicado, "Já existe uma conta com este e-mail.");

        var usuario = new Usuario(req.Nome, req.Email, hasher.Hash(req.Senha), relogio.Agora);
        await usuarios.AdicionarAsync(usuario, ct);
        await uow.SalvarAsync(ct);

        return new RegistrarResponse(usuario.Id, usuario.Nome, usuario.Email);
    }

    /// <summary>E02: autentica; 5 falhas seguidas bloqueiam a conta por 15 minutos.</summary>
    public async Task<SessaoEmitida<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        await validadorLogin.ValidateAndThrowAsync(req, ct);
        var agora = relogio.Agora;

        var usuario = await usuarios.ObterPorEmailAsync(Usuario.NormalizarEmail(req.Email), ct);
        if (usuario is null || usuario.EstaBloqueado(agora))
            throw new UnauthorizedException("CREDENCIAIS_INVALIDAS", MensagemLoginInvalido);

        if (!hasher.Verificar(req.Senha, usuario.SenhaHash))
        {
            usuario.RegistrarFalhaLogin(agora, seguranca.Value.MaxFalhasLogin, seguranca.Value.BloqueioMinutos);
            await uow.SalvarAsync(ct);
            throw new UnauthorizedException("CREDENCIAIS_INVALIDAS", MensagemLoginInvalido);
        }

        usuario.RegistrarLoginComSucesso();
        var (accessToken, expiraEm) = tokens.GerarAccessToken(usuario);
        var (refresh, refreshExpira) = await EmitirRefreshAsync(usuario.Id, ct);
        await uow.SalvarAsync(ct);

        var resposta = new LoginResponse(accessToken, expiraEm,
            new UsuarioLogado(usuario.Id, usuario.Nome, usuario.PrecisaCheckup));
        return new SessaoEmitida<LoginResponse>(resposta, refresh, refreshExpira);
    }

    /// <summary>
    /// E03: rotação do refresh token. Reuso de um token já trocado revoga a sessão daquele aparelho (a família do
    /// token), e não as dos outros aparelhos; um reenvio até 60 s depois da troca é tolerado (DV25 · spec §4.3).
    /// </summary>
    public async Task<SessaoEmitida<RefreshResponse>> RefreshAsync(string? valor, CancellationToken ct)
    {
        var agora = relogio.Agora;
        if (string.IsNullOrWhiteSpace(valor))
            throw new UnauthorizedException("REFRESH_INVALIDO", "Sua sessão expirou. Entre de novo.");

        var atual = await refreshTokens.ObterPorHashAsync(tokens.HashRefreshToken(valor), ct)
            ?? throw new UnauthorizedException("REFRESH_INVALIDO", "Sua sessão expirou. Entre de novo.");

        // Duas abas renovando juntas ou resposta perdida no celular: o token acabou de ser trocado, não é roubo
        var reenvioTolerado = atual.Revogado && atual.ReusoTolerado(agora)
            && await refreshTokens.FamiliaAtivaAsync(atual.FamiliaId, agora, ct);
        if (atual.Revogado && !reenvioTolerado)
        {
            await refreshTokens.RevogarFamiliaAsync(atual.FamiliaId, agora, ct);
            await uow.SalvarAsync(ct);
            throw new UnauthorizedException("REFRESH_REUTILIZADO", "Sua sessão expirou. Entre de novo.");
        }

        if (!reenvioTolerado && !atual.EstaAtivo(agora))
            throw new UnauthorizedException("REFRESH_INVALIDO", "Sua sessão expirou. Entre de novo.");

        var usuario = await usuarios.ObterPorIdAsync(atual.UsuarioId, ct)
            ?? throw new UnauthorizedException("REFRESH_INVALIDO", "Sua sessão expirou. Entre de novo.");

        atual.Substituir(agora);
        var (accessToken, expiraEm) = tokens.GerarAccessToken(usuario);
        var (refresh, refreshExpira) = await EmitirRefreshAsync(usuario.Id, ct, atual.FamiliaId);
        await uow.SalvarAsync(ct);

        return new SessaoEmitida<RefreshResponse>(new RefreshResponse(accessToken, expiraEm), refresh, refreshExpira);
    }

    /// <summary>E04: revoga o refresh token atual.</summary>
    public async Task LogoutAsync(string? valor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;

        var atual = await refreshTokens.ObterPorHashAsync(tokens.HashRefreshToken(valor), ct);
        if (atual is null) return;

        atual.Revogar(relogio.Agora);
        await uow.SalvarAsync(ct);
    }

    /// <param name="familiaId">Sessão do aparelho (renovação); sem ela (login), abre uma sessão nova.</param>
    private async Task<(string Valor, DateTimeOffset ExpiraEm)> EmitirRefreshAsync(Guid usuarioId, CancellationToken ct,
        Guid? familiaId = null)
    {
        var valor = tokens.GerarValorRefreshToken();
        var rt = new RefreshToken(usuarioId, tokens.HashRefreshToken(valor), relogio.Agora, jwt.Value.RefreshTokenDays, familiaId);
        await refreshTokens.AdicionarAsync(rt, ct);
        return (valor, rt.ExpiraEm);
    }
}
