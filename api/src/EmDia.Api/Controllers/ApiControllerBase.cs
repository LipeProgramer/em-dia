using System.Security.Claims;
using EmDia.Api.Extensions;
using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Historico;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmDia.Api.Controllers;

// Controllers: recebem a requisição, leem o usuário do token, chamam o service e devolvem o status HTTP.
// Não contêm regra de negócio (spec §1.2). Toda rota exige autenticação (fallback policy), exceto as marcadas
// com [AllowAnonymous]. O id do usuário vem sempre do token (claim "sub"), nunca do corpo (spec §4.4).

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid UsuarioId =>
        Guid.Parse(User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException());
}
