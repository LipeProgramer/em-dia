namespace EmDia.Domain.Entities;

/// <summary>Conta de acesso. Só guarda o necessário (LGPD): nome, e-mail e hash da senha.</summary>
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

    public PreferenciasAviso? Preferencias { get; private set; }

    protected Usuario() { }

    public Usuario(string nome, string email, string senhaHash, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = NormalizarEmail(email);
        SenhaHash = senhaHash;
        FalhasLogin = 0;
        CriadoEm = agora;
        Preferencias = new PreferenciasAviso(Id, agora);
    }

    /// <summary>E-mail é gravado em minúsculas (spec §2.3).</summary>
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>True quando o usuário nunca fez o check-up (spec E02).</summary>
    public bool PrecisaCheckup => UltimoCheckupEm is null;

    public bool EstaBloqueado(DateTimeOffset agora) =>
        BloqueadoAte.HasValue && agora < BloqueadoAte.Value;

    /// <summary>Soma uma falha; após <paramref name="maxFalhas"/> falhas seguidas, bloqueia a conta (spec §4.1).</summary>
    public void RegistrarFalhaLogin(DateTimeOffset agora, int maxFalhas, int bloqueioMinutos)
    {
        FalhasLogin++;
        if (FalhasLogin >= maxFalhas)
        {
            BloqueadoAte = agora.AddMinutes(bloqueioMinutos);
            FalhasLogin = 0; // a contagem recomeça depois do bloqueio
        }
    }

    public void RegistrarLoginComSucesso()
    {
        FalhasLogin = 0;
        BloqueadoAte = null;
    }

    public void RegistrarCheckup(DateTimeOffset agora) => UltimoCheckupEm = agora;

    /// <summary>Nova senha pelo link de redefinição (DV24): também desfaz o bloqueio por tentativas erradas.</summary>
    public void RedefinirSenha(string novoHash)
    {
        SenhaHash = novoHash;
        RegistrarLoginComSucesso();
    }
}
