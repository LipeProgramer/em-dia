using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmDia.Application.Interfaces;
using EmDia.Application.Services;
using EmDia.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EmDia.Infrastructure.Security;

/// <summary>
/// Gera access tokens JWT (15 min) e refresh tokens opacos (7 dias) — spec §4.3.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly string _chave;
    private readonly string _emissor;
    private readonly string _audiencia;
    private const int AccessTokenMinutos = 15;
    private const int RefreshTokenDias = 7;

    public JwtTokenService(IConfiguration config)
    {
        _chave = config["Jwt:Chave"]
            ?? throw new InvalidOperationException("Jwt:Chave não configurada.");
        _emissor = config["Jwt:Emissor"] ?? "emdia-api";
        _audiencia = config["Jwt:Audiencia"] ?? "emdia-web";
    }

    public string GerarAccessToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var chaveBytes = Encoding.UTF8.GetBytes(_chave);
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(chaveBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _emissor,
            audience: _audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenMinutos),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GerarRefreshToken(Guid usuarioId, DateTimeOffset agora)
    {
        // Token opaco de 32 bytes aleatórios
        var tokenCru = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = AuthService.ComputarSHA256(tokenCru);

        // Armazena o hash; retorna o objeto com TokenHash populado
        // O valor cru é extraído pelo controller e enviado no cookie
        return new RefreshTokenComValorCru(usuarioId, hash, tokenCru, agora, RefreshTokenDias);
    }
}

/// <summary>
/// Extensão de RefreshToken que carrega o valor cru para ser enviado ao cliente.
/// Nunca persiste o valor cru — apenas o hash é salvo.
/// </summary>
public class RefreshTokenComValorCru : RefreshToken
{
    public string ValorCru { get; }

    public RefreshTokenComValorCru(
        Guid usuarioId, string hash, string valorCru,
        DateTimeOffset agora, int diasValidade)
        : base(usuarioId, hash, agora, diasValidade)
    {
        ValorCru = valorCru;
    }
}
