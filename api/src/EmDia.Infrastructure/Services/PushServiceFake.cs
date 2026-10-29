using EmDia.Application.Interfaces;

namespace EmDia.Infrastructure.Services;

/// <summary>Implementação fake do IPushService para testes: só registra as notificações.</summary>
public class PushServiceFake : IPushService
{
    public List<(Guid UsuarioId, NotificacaoPush Notificacao)> Enviadas { get; } = [];

    public string ChavePublica => "BFakeChavePublicaDeTestes";

    public Task<int> EnviarAsync(Guid usuarioId, NotificacaoPush notificacao, CancellationToken ct = default)
    {
        Enviadas.Add((usuarioId, notificacao));
        return Task.FromResult(1);
    }
}
