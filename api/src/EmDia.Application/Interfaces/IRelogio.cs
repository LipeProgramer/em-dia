namespace EmDia.Application.Interfaces;

/// <summary>
/// Abstração do relógio para facilitar testes (fixar data).
/// "Hoje" sempre no fuso America/Sao_Paulo.
/// </summary>
public interface IRelogio
{
    DateOnly Hoje { get; }
    DateTimeOffset Agora { get; }
}
