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

/// <summary>E20–E24 · Veículos.</summary>
[Route("api/v1/veiculos")]
public class VeiculosController(VeiculoService veiculos) : ApiControllerBase
{
    /// <summary>E20 · Listar veículos.</summary>
    [HttpGet]
    public Task<IReadOnlyList<VeiculoResponse>> Listar(CancellationToken ct) => veiculos.ListarAsync(UsuarioId, ct);

    /// <summary>E21 · Cadastrar veículo.</summary>
    [HttpPost]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(CriarVeiculoRequest req, CancellationToken ct)
    {
        var veiculo = await veiculos.CriarAsync(UsuarioId, req, ct);
        return Created($"/api/v1/veiculos/{veiculo.Id}", veiculo);
    }

    /// <summary>E22 · Editar apelido, tipo e média de km.</summary>
    [HttpPut("{id:guid}")]
    public Task<VeiculoResponse> Editar(Guid id, EditarVeiculoRequest req, CancellationToken ct) =>
        veiculos.EditarAsync(id, UsuarioId, req, ct);

    /// <summary>E23 · Atualizar a quilometragem atual.</summary>
    [HttpPatch("{id:guid}/km")]
    [ProducesResponseType<AtualizarKmResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<AtualizarKmResponse> AtualizarKm(Guid id, AtualizarKmRequest req, CancellationToken ct) =>
        veiculos.AtualizarKmAsync(id, UsuarioId, req, ct);

    /// <summary>E24 · Excluir veículo (bloqueado se houver itens ligados).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await veiculos.ExcluirAsync(id, UsuarioId, ct);
        return NoContent();
    }
}
