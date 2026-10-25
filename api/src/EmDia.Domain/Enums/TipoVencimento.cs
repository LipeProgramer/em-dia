namespace EmDia.Domain.Enums;

/// <summary>
/// Tipo de vencimento do item. Gravado no banco como smallint (spec §2.6).
/// </summary>
public enum TipoVencimento : short
{
    /// <summary>Data fixa e única (ex.: CNH, validade de passaporte).</summary>
    DataUnica = 1,

    /// <summary>Renova em intervalos fixos de dias (ex.: IPVA anual).</summary>
    RecorrentePorTempo = 2,

    /// <summary>Vence por quilometragem OU por tempo, o que vier primeiro.</summary>
    UsoOuTempo = 3,

    /// <summary>Garantia: data de compra + meses do fabricante + 90 dias legais.</summary>
    Calculado = 4
}
