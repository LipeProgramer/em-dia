namespace EmDia.Domain.Entities;

/// <summary>
/// Preferências de aviso por e-mail (1:1 com Usuario).
/// Criada automaticamente com a conta e com valores padrão (spec §2.3).
/// </summary>
public class PreferenciasAviso
{
    public Guid UsuarioId { get; private set; }
    public bool ReceberEmail { get; private set; } = true;
    public short MaxReavisos { get; private set; } = 2;
    public short IntervaloReavisoDias { get; private set; } = 3;
    public bool ModoFocoPadrao { get; private set; } = false;
    public DateTimeOffset AtualizadoEm { get; private set; }

    // EF Core
    protected PreferenciasAviso() { }

    public PreferenciasAviso(Guid usuarioId, DateTimeOffset agora)
    {
        UsuarioId = usuarioId;
        AtualizadoEm = agora;
    }

    public void Atualizar(
        bool receberEmail,
        short maxReavisos,
        short intervaloReavisoDias,
        bool modoFocoPadrao,
        DateTimeOffset agora)
    {
        // Mesmas faixas dos CHECKs da tabela (spec §2.3)
        if (maxReavisos is < 0 or > 5)
            throw new Exceptions.CampoInvalidoException("maxReavisos", "Escolha entre 0 e 5 reavisos.");
        if (intervaloReavisoDias is < 1 or > 7)
            throw new Exceptions.CampoInvalidoException("intervaloReavisoDias", "Escolha entre 1 e 7 dias.");

        ReceberEmail = receberEmail;
        MaxReavisos = maxReavisos;
        IntervaloReavisoDias = intervaloReavisoDias;
        ModoFocoPadrao = modoFocoPadrao;
        AtualizadoEm = agora;
    }
}
