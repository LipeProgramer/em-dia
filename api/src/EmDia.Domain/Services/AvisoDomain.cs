using EmDia.Domain.Entities;
using EmDia.Domain.Enums;

namespace EmDia.Domain.Services;

/// <summary>
/// Lógica de domínio para avisos por e-mail.
/// RN08: aviso antecipado quando o item entra em Atenção ou Vencido.
/// RN09: reavisos com limite e intervalo configuráveis; ciclo reinicia com novo vencimento.
/// </summary>
public static class AvisoDomain
{
    /// <summary>
    /// RN08/RN09: determina se um aviso (ou reaviso) deve ser enviado para o item hoje.
    /// </summary>
    /// <param name="avisosSendados">Avisos já enviados para o vencimento atual.</param>
    /// <param name="hoje">Data de hoje.</param>
    /// <param name="status">Status calculado do item.</param>
    /// <param name="vencimentoReferencia">Próximo vencimento do item.</param>
    /// <param name="maxReavisos">Máximo de reavisos configurado pelo usuário (0 a 5).</param>
    /// <param name="intervaloReavisoDias">Intervalo entre reavisos em dias (1 a 7).</param>
    public static DeveAvisar VerificarAviso(
        IEnumerable<Aviso> avisosSendados,
        DateOnly hoje,
        StatusItem status,
        DateOnly vencimentoReferencia,
        short maxReavisos,
        short intervaloReavisoDias)
    {
        // Só avisa se estiver em Atenção ou Vencido
        if (status is not (StatusItem.Atencao or StatusItem.Vencido))
            return DeveAvisar.Nao();

        var avisosDoClico = avisosSendados
            .Where(a => a.VencimentoReferencia == vencimentoReferencia && a.Sucesso)
            .OrderBy(a => a.Numero)
            .ToList();

        // Aviso antecipado (numero 0) — só envia uma vez por ciclo
        if (!avisosDoClico.Any())
            return DeveAvisar.Sim(TipoAviso.Antecipado, 0);

        // Reavisos
        short ultimoNumero = avisosDoClico.Last().Numero;
        if (ultimoNumero >= maxReavisos)
            return DeveAvisar.Nao(); // limite atingido

        // Verifica se já passou o intervalo desde o último aviso
        var ultimoAviso = avisosDoClico.Last();
        var proximoReavisoEm = ultimoAviso.EnviadoEm.UtcDateTime.Date.AddDays(intervaloReavisoDias);
        if (hoje.ToDateTime(TimeOnly.MinValue) < proximoReavisoEm)
            return DeveAvisar.Nao();

        short proximoNumero = (short)(ultimoNumero + 1);
        return DeveAvisar.Sim(TipoAviso.Reaviso, proximoNumero);
    }
}

public record DeveAvisar(bool Enviar, TipoAviso Tipo, short Numero)
{
    public static DeveAvisar Nao() => new(false, TipoAviso.Antecipado, 0);
    public static DeveAvisar Sim(TipoAviso tipo, short numero) => new(true, tipo, numero);
}
