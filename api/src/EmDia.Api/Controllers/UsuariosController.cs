using EmDia.Api.Extensions;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EmDia.Api.Controllers;

/// <summary>E05–E07 · Conta e preferências.</summary>
[Route("api/v1/usuarios/me")]
public class UsuariosController(UsuarioService usuarios, IWebHostEnvironment env) : ApiControllerBase
{
    /// <summary>E05 · Dados da conta e preferências.</summary>
    /// <remarks>A conta vem do token: não há id no endereço, então ninguém consulta a conta de outra pessoa.</remarks>
    /// <response code="200">Nome, e-mail, último check-up, se precisa fazer o check-up e as preferências.</response>
    [HttpGet]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    public Task<UsuarioResponse> Obter(CancellationToken ct) => usuarios.ObterAsync(UsuarioId, ct);

    /// <summary>E06 · Alterar preferências de aviso (corpo JSON).</summary>
    /// <remarks>Para testar sem JSON, use PUT /api/v1/usuarios/me/preferencias/rapido.</remarks>
    /// <param name="req">Todas as preferências. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="200">Preferências gravadas.</response>
    /// <response code="400">Valor fora do intervalo (reavisos de 0 a 5; intervalo de 1 a 7 dias).</response>
    [HttpPut("preferencias")]
    [ProducesResponseType<PreferenciasDto>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<PreferenciasDto> AtualizarPreferencias(PreferenciasDto req, CancellationToken ct) =>
        usuarios.AtualizarPreferenciasAsync(UsuarioId, req, ct);

    /// <summary>E06 por parâmetros · Alterar preferências de aviso (sem JSON).</summary>
    /// <param name="receberEmail">Receber os avisos de vencimento por e-mail</param>
    /// <param name="maxReavisos">Quantas vezes reavisar um item vencido (0 a 5)</param>
    /// <param name="intervaloReavisoDias">Dias entre um reaviso e outro (1 a 7)</param>
    /// <param name="modoFocoPadrao">Abrir o Radar já no modo foco (só o que é urgente)</param>
    /// <param name="ct"></param>
    /// <response code="200">Preferências gravadas.</response>
    /// <response code="400">Faltou um campo ("Campo obrigatório.") ou o valor está fora do intervalo.</response>
    [HttpPut("preferencias/rapido")]
    [ProducesResponseType<PreferenciasDto>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<PreferenciasDto> AtualizarPreferenciasPorParametros([FromQuery, BindRequired] bool receberEmail,
        [FromQuery, BindRequired] short maxReavisos, [FromQuery, BindRequired] short intervaloReavisoDias,
        [FromQuery, BindRequired] bool modoFocoPadrao, CancellationToken ct) =>
        usuarios.AtualizarPreferenciasAsync(UsuarioId,
            new PreferenciasDto(receberEmail, maxReavisos, intervaloReavisoDias, modoFocoPadrao), ct);

    /// <summary>E07 · Excluir a conta e todos os dados (RF18 · LGPD), com a senha no corpo JSON.</summary>
    /// <remarks>Apaga itens, veículos, histórico, check-ups e sessões. Não tem como desfazer.</remarks>
    /// <param name="req">Senha atual, para confirmar.</param>
    /// <param name="ct"></param>
    /// <response code="204">Conta e dados apagados; o cookie de sessão é removido.</response>
    /// <response code="400">Faltou a senha.</response>
    /// <response code="422">SENHA_INCORRETA: a senha de confirmação está errada.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Excluir(ExcluirContaRequest req, CancellationToken ct)
    {
        await usuarios.ExcluirContaAsync(UsuarioId, req, ct);
        Response.ApagarCookieRefresh(env);
        return NoContent();
    }

    /// <summary>E07 por parâmetro · Excluir a conta e todos os dados (sem JSON).</summary>
    /// <remarks>Apaga itens, veículos, histórico, check-ups e sessões. Não tem como desfazer.</remarks>
    /// <param name="senha">Senha atual, para confirmar (vai no cabeçalho, não no endereço)</param>
    /// <param name="ct"></param>
    /// <response code="204">Conta e dados apagados.</response>
    /// <response code="400">Faltou a senha ("Campo obrigatório.").</response>
    /// <response code="422">SENHA_INCORRETA: a senha de confirmação está errada.</response>
    [HttpDelete("rapido")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> ExcluirPorParametro([FromHeader(Name = "senha"), BindRequired] string senha, CancellationToken ct) =>
        Excluir(new ExcluirContaRequest(senha), ct);
}
