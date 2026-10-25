using EmDia.Domain.Entities;

namespace EmDia.Application.Interfaces;

public interface ITokenService
{
    string GerarAccessToken(Usuario usuario);
    RefreshToken GerarRefreshToken(Guid usuarioId);
}
