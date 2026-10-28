using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces;

/// <summary>Emissão de tokens (spec §4.2 e §4.3).</summary>
public interface ITokenService
{
    /// <summary>Token de acesso JWT (HS256, 15 minutos).</summary>
    (string Token, DateTimeOffset ExpiraEm) GerarAccessToken(Usuario usuario);

    /// <summary>Valor aleatório de 64 bytes para o token de renovação.</summary>
    string GerarValorRefreshToken();

    /// <summary>Hash SHA-256 do token de renovação (o que fica no banco).</summary>
    string HashRefreshToken(string valor);
}
