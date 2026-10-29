using EmDia.Application.DTOs.Notificacoes;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using FluentValidation;

namespace EmDia.Application.Services;

/// <summary>Inscrição dos aparelhos para notificações no celular (Web Push · extra · DV20).</summary>
public class NotificacoesService(
    IInscricaoPushRepository inscricoes,
    IPushService push,
    IUnitOfWork uow,
    IRelogio relogio,
    IValidator<InscricaoPushRequest> validador)
{
    public ChavePublicaResponse ChavePublica() => new(push.ChavePublica);

    /// <summary>Grava o aparelho; se o mesmo endereço já existir, atualiza as chaves e o dono.</summary>
    public async Task InscreverAsync(Guid usuarioId, InscricaoPushRequest req, CancellationToken ct)
    {
        await validador.ValidateAndThrowAsync(req, ct);

        var existente = await inscricoes.ObterPorEndpointAsync(req.Endpoint, ct);
        if (existente is null)
            await inscricoes.AdicionarAsync(new InscricaoPush(usuarioId, req.Endpoint, req.P256dh, req.Auth, relogio.Agora), ct);
        else
            existente.Renovar(usuarioId, req.P256dh, req.Auth, relogio.Agora);

        await uow.SalvarAsync(ct);
    }

    /// <summary>Remove o aparelho (só se for do próprio usuário; senão, não faz nada).</summary>
    public async Task RemoverAsync(Guid usuarioId, string endpoint, CancellationToken ct)
    {
        var existente = await inscricoes.ObterPorEndpointAsync(endpoint, ct);
        if (existente is null || existente.UsuarioId != usuarioId)
            return;

        inscricoes.Remover(existente);
        await uow.SalvarAsync(ct);
    }

    /// <summary>Notificação de teste, para conferir no aparelho que está tudo ligado.</summary>
    public async Task<TesteNotificacaoResponse> TestarAsync(Guid usuarioId, CancellationToken ct) =>
        new(await push.EnviarAsync(usuarioId,
            new NotificacaoPush("Em Dia", "Notificações ligadas! Vamos avisar antes dos seus prazos vencerem.", "/"), ct));
}
