using EmDia.Application.DTOs.Historico;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>E25 · Histórico de conclusões.</summary>
[Route("api/v1/historico")]
public class HistoricoController(HistoricoService historico) : ApiControllerBase
{
    /// <summary>E25 · Histórico de conclusões (filtros opcionais).</summary>
    /// <param name="itemId">Só as conclusões de um item (uuid)</param>
    /// <param name="de">A partir desta data (aaaa-mm-dd)</param>
    /// <param name="ate">Até esta data (aaaa-mm-dd)</param>
    /// <param name="ct"></param>
    /// <response code="200">Cada conclusão com data, km, valor gasto, observação e o vencimento antes e depois.</response>
    /// <response code="400">Período inválido ("de" depois de "ate").</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HistoricoResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IReadOnlyList<HistoricoResponse>> Listar([FromQuery] Guid? itemId, [FromQuery] DateOnly? de,
        [FromQuery] DateOnly? ate, CancellationToken ct) => historico.ListarAsync(UsuarioId, itemId, de, ate, ct);
}
