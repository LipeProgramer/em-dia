using EmDia.Application.Interfaces;

namespace EmDia.Infrastructure.Security;

/// <summary>
/// Hashing de senha com BCrypt, custo 12 (spec §4.3).
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senha)
        => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);

    public bool Verificar(string senha, string hash)
        => BCrypt.Net.BCrypt.Verify(senha, hash);
}
