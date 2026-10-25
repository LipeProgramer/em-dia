using EmDia.Application.Interfaces;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// Implementação real de IRelogio: retorna a data/hora no fuso America/Sao_Paulo (spec §3.1).
/// </summary>
public class RelogioSaoPaulo : IRelogio
{
    private static readonly TimeZoneInfo FusoSP =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public DateOnly Hoje =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoSP));

    public DateTimeOffset Agora => DateTimeOffset.UtcNow;
}
