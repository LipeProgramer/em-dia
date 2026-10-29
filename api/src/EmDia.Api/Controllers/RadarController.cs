using EmDia.Application.DTOs.Itens;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>E12 · Radar e modo foco.</summary>
[Route("api/v1/radar")]
public class RadarController(RadarService radar) : ApiControllerBase
{
    /// <summary>E12 · Radar: resumo e itens por anel (vencido, atenção, em dia).</summary>
    /// <param name="foco">true mostra só o que é urgente (vencidos e atenção); vazio usa a preferência da pessoa</param>
    /// <param name="ct"></param>
    /// <response code="200">Contagem por status, se o modo foco está ligado e os itens com o anel de cada um.</response>
    [HttpGet]
    [ProducesResponseType<RadarResponse>(StatusCodes.Status200OK)]
    public Task<RadarResponse> Obter([FromQuery] bool? foco, CancellationToken ct) => radar.ObterAsync(UsuarioId, foco, ct);
}
