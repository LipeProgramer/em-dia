namespace EmDia.Domain.Enums;

/// <summary>
/// Situação persistida do item (gravada no banco). Distinta do StatusItem calculado.
/// </summary>
public enum SituacaoItem : short
{
    Ativo = 1,
    Encerrado = 2
}
