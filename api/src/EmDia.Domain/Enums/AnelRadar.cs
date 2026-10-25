namespace EmDia.Domain.Enums;

/// <summary>
/// Anel do Radar — calculado na leitura (spec §2.6, RN03).
/// </summary>
public enum AnelRadar
{
    /// <summary>Vencido (prazo passou ou km atingida).</summary>
    Centro,

    /// <summary>Vence em até 30 dias.</summary>
    Anel1,

    /// <summary>Vence em 31 a 90 dias.</summary>
    Anel2,

    /// <summary>Vence em mais de 90 dias.</summary>
    Externo
}
