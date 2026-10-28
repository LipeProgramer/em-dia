namespace EmDia.Domain.Interfaces;

/// <summary>
/// Abstrai a data/hora atual para que os testes possam fixar "hoje" em 15/10/2026
/// sem depender do relógio real da máquina (spec §6.1).
/// </summary>
public interface IRelogio
{
    /// <summary>Data de hoje no fuso America/Sao_Paulo.</summary>
    DateOnly Hoje { get; }

    /// <summary>Momento atual em UTC.</summary>
    DateTimeOffset Agora { get; }
}
