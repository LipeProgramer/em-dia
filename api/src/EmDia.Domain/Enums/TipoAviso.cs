namespace EmDia.Domain.Enums;

/// <summary>Tipo do aviso enviado por e-mail (spec §2.6).</summary>
public enum TipoAviso : short
{
    Antecipado = 1,
    Reaviso = 2
}

/// <summary>Canal pelo qual o aviso é enviado (spec §2.6).</summary>
public enum CanalAviso : short
{
    Email = 1
}
