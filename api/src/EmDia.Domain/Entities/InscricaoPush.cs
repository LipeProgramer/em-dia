namespace EmDia.Domain.Entities;

/// <summary>
/// Aparelho inscrito para receber notificações (Web Push). O navegador gera o endereço de entrega e as chaves;
/// a API só guarda os dados para enviar os avisos (extra · DV20).
/// </summary>
public class InscricaoPush
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256dh { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }

    protected InscricaoPush() { }

    public InscricaoPush(Guid usuarioId, string endpoint, string p256dh, string auth, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CriadoEm = agora;
    }

    /// <summary>O mesmo aparelho pode se inscrever de novo (novas chaves) ou trocar de conta.</summary>
    public void Renovar(Guid usuarioId, string p256dh, string auth, DateTimeOffset agora)
    {
        UsuarioId = usuarioId;
        P256dh = p256dh;
        Auth = auth;
        CriadoEm = agora;
    }
}
