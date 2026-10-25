namespace EmDia.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio do Domain é violada.
/// O <see cref="Code"/> permite ao frontend reagir programaticamente (ex.: PASSOS_PENDENTES).
/// Mapeada para HTTP 422 pelo middleware global (spec §3.5).
/// </summary>
public sealed class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
