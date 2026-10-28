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

/// <summary>E05–E07 · Conta e preferências.</summary>
[Route("api/v1/usuarios/me")]
public class UsuariosController(UsuarioService usuarios, IWebHostEnvironment env) : ApiControllerBase
{
    /// <summary>E05 · Dados da conta e preferências.</summary>
    [HttpGet]
    public Task<UsuarioResponse> Obter(CancellationToken ct) => usuarios.ObterAsync(UsuarioId, ct);

    /// <summary>E06 · Alterar preferências de aviso.</summary>
    [HttpPut("preferencias")]
    [ProducesResponseType<PreferenciasDto>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<PreferenciasDto> AtualizarPreferencias(PreferenciasDto req, CancellationToken ct) =>
        usuarios.AtualizarPreferenciasAsync(UsuarioId, req, ct);

    /// <summary>E07 · Excluir a conta e todos os dados (RF18 · LGPD). Exige a senha.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Excluir(ExcluirContaRequest req, CancellationToken ct)
    {
        await usuarios.ExcluirContaAsync(UsuarioId, req, ct);
        Response.ApagarCookieRefresh(env);
        return NoContent();
    }
}
