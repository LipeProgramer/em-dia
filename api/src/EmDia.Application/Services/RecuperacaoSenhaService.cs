using System.Net;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmDia.Application.Services;

/// <summary>
/// Extra (DV24) · "Esqueci minha senha": envia um link de uso único (30 minutos) para o e-mail da conta e troca a
/// senha por ele. A resposta do pedido é sempre a mesma, exista ou não a conta, para não revelar quem é cadastrado.
/// </summary>
public class RecuperacaoSenhaService(
    IUsuarioRepository usuarios,
    IRedefinicaoSenhaRepository redefinicoes,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    IPasswordHasher hasher,
    ITokenService tokens,
    IEmailService email,
    IRelogio relogio,
    IOptions<SiteOptions> site,
    IValidator<EsqueciSenhaRequest> validadorPedido,
    IValidator<RedefinirSenhaRequest> validadorRedefinir,
    ILogger<RecuperacaoSenhaService> log)
{
    public const string CodigoLinkInvalido = "LINK_INVALIDO";

    /// <summary>Um pedido por minuto por conta: evita encher a caixa de entrada de alguém de e-mails.</summary>
    private static readonly TimeSpan IntervaloEntrePedidos = TimeSpan.FromMinutes(1);

    public async Task SolicitarAsync(EsqueciSenhaRequest req, CancellationToken ct)
    {
        await validadorPedido.ValidateAndThrowAsync(req, ct);
        var agora = relogio.Agora;

        var usuario = await usuarios.ObterPorEmailAsync(Usuario.NormalizarEmail(req.Email), ct);
        if (usuario is null)
            return;

        var abertas = await redefinicoes.ListarAbertasAsync(usuario.Id, ct);
        if (abertas.Any(r => agora - r.CriadoEm < IntervaloEntrePedidos))
            return;

        // Só o link mais novo vale
        foreach (var antiga in abertas)
            antiga.Usar(agora);

        var valor = tokens.GerarValorRefreshToken();
        await redefinicoes.AdicionarAsync(new RedefinicaoSenha(usuario.Id, tokens.HashRefreshToken(valor), agora), ct);
        await uow.SalvarAsync(ct);

        // Sem esperar o SMTP: a resposta leva o mesmo tempo com ou sem conta
        _ = EnviarAsync(usuario, valor);
    }

    public async Task RedefinirAsync(RedefinirSenhaRequest req, CancellationToken ct)
    {
        await validadorRedefinir.ValidateAndThrowAsync(req, ct);
        var agora = relogio.Agora;

        var redefinicao = await redefinicoes.ObterPorHashAsync(tokens.HashRefreshToken(req.Token.Trim()), ct);
        var usuario = redefinicao is not null && redefinicao.EstaValida(agora)
            ? await usuarios.ObterPorIdAsync(redefinicao.UsuarioId, ct)
            : null;
        if (usuario is null)
            throw new DomainException(CodigoLinkInvalido,
                "Este link expirou ou já foi usado. Peça um novo em \"Esqueci minha senha\".");

        usuario.RedefinirSenha(hasher.Hash(req.NovaSenha));
        redefinicao!.Usar(agora);
        // Quem estava logado com a senha antiga (inclusive um invasor) perde a sessão
        await refreshTokens.RevogarTodosDoUsuarioAsync(usuario.Id, agora, ct);
        await uow.SalvarAsync(ct);
    }

    private async Task EnviarAsync(Usuario usuario, string valor)
    {
        var link = $"{site.Value.Url.TrimEnd('/')}/redefinir-senha?token={Uri.EscapeDataString(valor)}";
        try
        {
            await email.EnviarAsync(usuario.Email, "Em Dia · Redefinir sua senha", Corpo(usuario.Nome, link));
        }
        catch (Exception e)
        {
            log.LogError(e, "Falha ao enviar o e-mail de redefinição de senha.");
        }
    }

    private static string Corpo(string nome, string link)
    {
        var linkHtml = WebUtility.HtmlEncode(link);
        return $"""
            <div style="font-family:sans-serif;max-width:560px;margin:0 auto;padding:20px">
              <h2 style="color:#0f756e;margin:0 0 12px">Em Dia</h2>
              <p>Olá, {WebUtility.HtmlEncode(nome)}!</p>
              <p>Recebemos um pedido para criar uma nova senha. Toque no botão abaixo:</p>
              <p style="margin:24px 0">
                <a href="{linkHtml}" style="background:#0f756e;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:bold">Criar nova senha</a>
              </p>
              <p>O link vale {RedefinicaoSenha.MinutosValidade} minutos e funciona uma vez só.</p>
              <p style="color:#555;font-size:14px">Se não foi você, ignore este e-mail: sua senha continua a mesma.</p>
              <p style="color:#555;font-size:12px;word-break:break-all">Se o botão não abrir, copie este endereço: {linkHtml}</p>
            </div>
            """;
    }
}
