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

/// <summary>E12 · Radar e modo foco.</summary>
[Route("api/v1/radar")]
public class RadarController(RadarService radar) : ApiControllerBase
{
    /// <summary>E12 · Resumo e itens por anel. Sem "foco", usa a preferência do usuário.</summary>
    [HttpGet]
    public Task<RadarResponse> Obter([FromQuery] bool? foco, CancellationToken ct) => radar.ObterAsync(UsuarioId, foco, ct);
}
