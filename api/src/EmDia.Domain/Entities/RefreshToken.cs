namespace EmDia.Domain.Entities;

/// <summary>Token de renovação de sessão (spec §4.3). Guarda só o hash SHA-256 do valor.</summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? RevogadoEm { get; private set; }

    protected RefreshToken() { }

    public RefreshToken(Guid usuarioId, string tokenHash, DateTimeOffset agora, int diasValidade)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CriadoEm = agora;
        ExpiraEm = agora.AddDays(diasValidade);
    }

    public bool Revogado => RevogadoEm is not null;

    public bool EstaAtivo(DateTimeOffset agora) => !Revogado && agora < ExpiraEm;

    public void Revogar(DateTimeOffset agora) => RevogadoEm ??= agora;
}
