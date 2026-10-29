namespace EmDia.Application.Interfaces;

/// <summary>Notificação curta mostrada no celular; <paramref name="Url"/> é a tela aberta ao tocar.</summary>
public record NotificacaoPush(string Titulo, string Corpo, string Url);

/// <summary>Envio de notificações Web Push para os aparelhos inscritos do usuário (extra · DV20).</summary>
public interface IPushService
{
    /// <summary>Chave pública VAPID (base64url), usada pelo navegador ao se inscrever.</summary>
    string ChavePublica { get; }

    /// <summary>Envia para todos os aparelhos do usuário; devolve quantos receberam.</summary>
    Task<int> EnviarAsync(Guid usuarioId, NotificacaoPush notificacao, CancellationToken ct = default);
}
