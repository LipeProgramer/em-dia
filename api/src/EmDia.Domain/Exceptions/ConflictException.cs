namespace EmDia.Domain.Exceptions;

/// <summary>
/// Conflito com dados existentes (e-mail duplicado, apelido repetido, veículo com itens).
/// Mapeada para HTTP 409 (spec §3.5).
/// </summary>
public sealed class ConflictException : Exception
{
    public string Code { get; }

    public ConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
