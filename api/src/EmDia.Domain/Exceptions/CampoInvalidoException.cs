namespace EmDia.Domain.Exceptions;

/// <summary>
/// Dado de entrada inválido que só pode ser verificado com outros registros ou com a data de hoje
/// (ex.: km menor que a base). Vira 400 com o erro no campo, no mesmo formato do FluentValidation.
/// </summary>
public sealed class CampoInvalidoException : Exception
{
    public string Campo { get; }

    public CampoInvalidoException(string campo, string message) : base(message)
        => Campo = campo;
}
