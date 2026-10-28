namespace EmDia.Domain.Exceptions;

/// <summary>
/// Lançada quando um recurso não existe ou pertence a outro usuário.
/// Mapeada para HTTP 404 — não revela que o recurso existe (spec §3.1, RN12).
/// </summary>
public sealed class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string code, string message)
        : base(message)
        => Code = code;
}
