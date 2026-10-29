namespace EmDia.Domain.Entities;

/// <summary>
/// Pedido de "esqueci minha senha" (extra · DV24). O link enviado por e-mail vale 30 minutos e uma vez só.
/// Como no token de renovação, só o hash SHA-256 do código fica no banco.
/// </summary>
public class RedefinicaoSenha
{
    public const int MinutosValidade = 30;

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? UsadoEm { get; private set; }

    protected RedefinicaoSenha() { }

    public RedefinicaoSenha(Guid usuarioId, string tokenHash, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        CriadoEm = agora;
        ExpiraEm = agora.AddMinutes(MinutosValidade);
    }

    public bool EstaValida(DateTimeOffset agora) => UsadoEm is null && agora < ExpiraEm;

    /// <summary>Gasta o link (usado ou substituído por um pedido mais novo).</summary>
    public void Usar(DateTimeOffset agora) => UsadoEm ??= agora;
}
