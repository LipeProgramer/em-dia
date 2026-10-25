namespace EmDia.Domain.Enums;

/// <summary>
/// Status calculado na leitura — nunca gravado no banco (spec §2.6, RN02).
/// </summary>
public enum StatusItem
{
    Vencido,
    Atencao,
    EmDia,
    Encerrado
}
