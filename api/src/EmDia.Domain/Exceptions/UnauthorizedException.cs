namespace EmDia.Domain.Exceptions;

/// <summary>
/// 401 Unauthorized — credenciais inválidas, token expirado, conta bloqueada.
/// </summary>
public class UnauthorizedException : Exception
{
    public string Code { get; }

    public UnauthorizedException(string code, string message) : base(message)
        => Code = code;
}
