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

/// <summary>E25 · Histórico de conclusões.</summary>
[Route("api/v1/historico")]
public class HistoricoController(HistoricoService historico) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HistoricoResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IReadOnlyList<HistoricoResponse>> Listar([FromQuery] Guid? itemId, [FromQuery] DateOnly? de,
        [FromQuery] DateOnly? ate, CancellationToken ct) => historico.ListarAsync(UsuarioId, itemId, de, ate, ct);
}
