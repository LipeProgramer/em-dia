using EmDia.Application.Interfaces;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// Implementação real de IRelogio: "hoje" é calculado no fuso America/Sao_Paulo (spec §3.1);
/// momentos são gravados em UTC.
/// </summary>
public class RelogioSaoPaulo : IRelogio
{
    private static readonly TimeZoneInfo FusoSP =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateOnly Hoje => DataLocal(DateTimeOffset.UtcNow);

    public DateTimeOffset Agora => DateTimeOffset.UtcNow;

    /// <summary>Data local (America/Sao_Paulo) de um momento qualquer.</summary>
    public static DateOnly DataLocal(DateTimeOffset momento) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(momento, FusoSP).DateTime);
}
