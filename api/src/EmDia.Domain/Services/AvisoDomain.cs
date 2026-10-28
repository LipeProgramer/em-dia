using EmDia.Domain.Entities;
using EmDia.Domain.Enums;

namespace EmDia.Domain.Services;

/// <summary>
/// RN08: aviso antecipado quando o item entra em Atenção ou Vencido e ainda não foi avisado naquele ciclo.
/// RN09: reaviso a cada intervalo_reaviso_dias, até max_reavisos por ciclo.
/// O ciclo é identificado pelo vencimento de referência e recomeça a cada novo vencimento.
/// </summary>
public static class AvisoDomain
{
    /// <param name="avisos">Avisos já registrados para o item (de qualquer ciclo).</param>
    /// <param name="dataEnvio">Converte o momento de envio na data local (America/Sao_Paulo).</param>
    public static DeveAvisar VerificarAviso(
        IEnumerable<Aviso> avisos,
        DateOnly hoje,
        StatusItem status,
        DateOnly vencimentoReferencia,
        short maxReavisos,
        short intervaloReavisoDias,
        Func<DateTimeOffset, DateOnly> dataEnvio)
    {
        if (status is not (StatusItem.Atencao or StatusItem.Vencido))
            return DeveAvisar.Nao();

        var doCiclo = avisos
            .Where(a => a.VencimentoReferencia == vencimentoReferencia && a.Sucesso)
            .OrderBy(a => a.Numero)
            .ToList();

        if (doCiclo.Count == 0)
            return DeveAvisar.Sim(TipoAviso.Antecipado, 0);

        var ultimo = doCiclo[^1];
        if (ultimo.Numero >= maxReavisos)
            return DeveAvisar.Nao();

        if (hoje < dataEnvio(ultimo.EnviadoEm).AddDays(intervaloReavisoDias))
            return DeveAvisar.Nao();

        return DeveAvisar.Sim(TipoAviso.Reaviso, (short)(ultimo.Numero + 1));
    }
}

public record DeveAvisar(bool Enviar, TipoAviso Tipo, short Numero)
{
    public static DeveAvisar Nao() => new(false, TipoAviso.Antecipado, 0);
    public static DeveAvisar Sim(TipoAviso tipo, short numero) => new(true, tipo, numero);
}
