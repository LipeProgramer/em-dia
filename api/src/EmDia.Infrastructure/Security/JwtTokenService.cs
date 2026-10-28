using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmDia.Application;
using EmDia.Application.Interfaces;
using EmDia.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmDia.Infrastructure.Security;

/// <summary>
/// Token de acesso JWT HS256 com sub, email, name, jti, iat, exp, iss e aud (spec §4.2)
/// e token de renovação aleatório de 64 bytes guardado como hash SHA-256 (spec §4.3).
/// </summary>
public class JwtTokenService(IOptions<JwtOptions> opcoes) : ITokenService
{
    private readonly JwtOptions _jwt = opcoes.Value;

    public (string Token, DateTimeOffset ExpiraEm) GerarAccessToken(Usuario usuario)
    {
        // A validade do JWT é conferida com o relógio do sistema pelo JwtBearer, então a emissão usa o mesmo relógio
        var agora = DateTimeOffset.UtcNow;
        var expiraEm = agora.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, agora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: agora.UtcDateTime,
            expires: expiraEm.UtcDateTime,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }

    public string GerarValorRefreshToken() =>
        Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string valor) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}
