namespace EmDia.Domain.Entities;

/// <summary>
/// Token de renovação de sessão (spec §2.3, §4.3).
/// Guarda apenas o hash SHA-256 do token — nunca o valor em texto claro.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? RevogadoEm { get; private set; }

    public bool EstaAtivo => RevogadoEm is null && DateTimeOffset.UtcNow < ExpiraEm;

    // EF Core
    protected RefreshToken() { }

    public RefreshToken(Guid usuarioId, string tokenHash, DateTimeOffset agora, int diasValidade)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CriadoEm = agora;
        ExpiraEm = agora.AddDays(diasValidade);
    }

    public void Revogar(DateTimeOffset agora) => RevogadoEm = agora;
}
