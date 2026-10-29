namespace EmDia.Domain.Entities;

/// <summary>
/// Token de renovação de sessão (spec §4.3). Guarda só o hash SHA-256 do valor.
/// Cada login abre uma <see cref="FamiliaId"/> (a sessão de um aparelho); as rotações herdam a família (DV25).
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Tolerância para reenviar um token recém-trocado (duas abas renovando juntas, resposta perdida no celular):
    /// dentro dela não é tratado como roubo (DV25).
    /// </summary>
    public static readonly TimeSpan ToleranciaReuso = TimeSpan.FromSeconds(60);

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid FamiliaId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset ExpiraEm { get; private set; }
    public DateTimeOffset? RevogadoEm { get; private set; }

    /// <summary>Quando foi trocado por um token novo na renovação (rotação), e não revogado por logout ou reuso.</summary>
    public DateTimeOffset? SubstituidoEm { get; private set; }

    protected RefreshToken() { }

    /// <param name="familiaId">Sessão a que pertence; sem ela (login), o token abre uma sessão nova.</param>
    public RefreshToken(Guid usuarioId, string tokenHash, DateTimeOffset agora, int diasValidade, Guid? familiaId = null)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        FamiliaId = familiaId ?? Id;
        TokenHash = tokenHash;
        CriadoEm = agora;
        ExpiraEm = agora.AddDays(diasValidade);
    }

    public bool Revogado => RevogadoEm is not null;

    public bool EstaAtivo(DateTimeOffset agora) => !Revogado && agora < ExpiraEm;

    public void Revogar(DateTimeOffset agora) => RevogadoEm ??= agora;

    /// <summary>Rotação: o token deixa de valer porque um novo foi emitido no lugar dele.</summary>
    public void Substituir(DateTimeOffset agora)
    {
        if (Revogado) return;
        RevogadoEm = agora;
        SubstituidoEm = agora;
    }

    /// <summary>Reenvio de um token trocado há pouco: corrida entre abas ou resposta perdida, não roubo (DV25).</summary>
    public bool ReusoTolerado(DateTimeOffset agora) =>
        SubstituidoEm is { } trocadoEm && agora - trocadoEm <= ToleranciaReuso && agora < ExpiraEm;
}
