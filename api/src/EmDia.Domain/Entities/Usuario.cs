namespace EmDia.Domain.Entities;

/// <summary>
/// Conta de acesso do usuário (spec §2.3).
/// Guarda apenas o necessário (LGPD): nome, e-mail e hash da senha.
/// </summary>
public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public DateTimeOffset? UltimoCheckupEm { get; private set; }
    public short FalhasLogin { get; private set; }
    public DateTimeOffset? BloqueadoAte { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    // Navegação
    public PreferenciasAviso? Preferencias { get; private set; }

    // EF Core
    protected Usuario() { }

    public Usuario(string nome, string email, string senhaHash, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email.ToLowerInvariant();
        SenhaHash = senhaHash;
        FalhasLogin = 0;
        CriadoEm = agora;
        Preferencias = new PreferenciasAviso(Id, agora);
    }

    public bool PrecisaCheckup => UltimoCheckupEm is null;

    public bool EstaBloqueado(DateTimeOffset agora) =>
        BloqueadoAte.HasValue && agora < BloqueadoAte.Value;

    public void RegistrarFalhaLogin(DateTimeOffset agora, int maxFalhas, int bloqueioMinutos)
    {
        FalhasLogin++;
        if (FalhasLogin >= maxFalhas)
            BloqueadoAte = agora.AddMinutes(bloqueioMinutos);
    }

    public void RegistrarLoginComSucesso()
    {
        FalhasLogin = 0;
        BloqueadoAte = null;
    }

    public void RegistrarCheckup(DateTimeOffset agora) => UltimoCheckupEm = agora;

    public void AlterarNome(string nome) => Nome = nome;
}
