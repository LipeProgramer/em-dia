using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EmDia.Api.Controllers;

/// <summary>E20–E24 · Veículos.</summary>
[Route("api/v1/veiculos")]
public class VeiculosController(VeiculoService veiculos) : ApiControllerBase
{
    /// <summary>E20 · Listar veículos.</summary>
    /// <response code="200">Veículos da conta, com a km atual e quantos itens cada um tem.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<VeiculoResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<VeiculoResponse>> Listar(CancellationToken ct) => veiculos.ListarAsync(UsuarioId, ct);

    /// <summary>E21 · Cadastrar veículo (corpo JSON).</summary>
    /// <remarks>Para testar sem JSON, use POST /api/v1/veiculos/rapido.</remarks>
    /// <param name="req">Dados do veículo. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="201">Veículo criado; o corpo traz o id.</response>
    /// <response code="400">Campo faltando ou inválido.</response>
    /// <response code="409">APELIDO_DUPLICADO: já existe um veículo com este apelido.</response>
    [HttpPost]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(CriarVeiculoRequest req, CancellationToken ct)
    {
        var veiculo = await veiculos.CriarAsync(UsuarioId, req, ct);
        return Created($"/api/v1/veiculos/{veiculo.Id}", veiculo);
    }

    /// <summary>E21 por parâmetros · Cadastrar veículo (sem JSON).</summary>
    /// <remarks>Ex.: apelido = Gol, tipo = Carro, kmAtual = 50000, mediaKmMes = 1500.</remarks>
    /// <param name="p">Dados do veículo</param>
    /// <param name="kmAtual">Km atual do painel (0 ou mais)</param>
    /// <param name="ct"></param>
    /// <response code="201">Veículo criado; copie o "id" para editar, atualizar a km ou excluir.</response>
    /// <response code="400">Faltou um campo obrigatório ("Campo obrigatório.") ou um valor é inválido.</response>
    /// <response code="409">APELIDO_DUPLICADO: já existe um veículo com este apelido.</response>
    [HttpPost("rapido")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarPorParametros([FromQuery] VeiculoParametros p, [FromQuery, BindRequired] int kmAtual,
        CancellationToken ct)
    {
        var veiculo = await veiculos.CriarAsync(UsuarioId, new CriarVeiculoRequest(p.Apelido ?? "", p.Tipo, kmAtual, p.MediaKmMes), ct);
        return Created($"/api/v1/veiculos/{veiculo.Id}", veiculo);
    }

    /// <summary>E22 · Editar apelido, tipo e média de km (corpo JSON).</summary>
    /// <remarks>A km atual muda pelo PATCH /api/v1/veiculos/{id}/km. Para testar sem JSON, use PUT /api/v1/veiculos/{id}/rapido.</remarks>
    /// <param name="id">Id do veículo (uuid)</param>
    /// <param name="req">Novos dados. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="200">Veículo atualizado.</response>
    /// <response code="400">Campo faltando ou inválido.</response>
    /// <response code="404">Veículo não existe ou é de outra pessoa.</response>
    /// <response code="409">APELIDO_DUPLICADO: já existe outro veículo com este apelido.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<VeiculoResponse> Editar(Guid id, EditarVeiculoRequest req, CancellationToken ct) =>
        veiculos.EditarAsync(id, UsuarioId, req, ct);

    /// <summary>E22 por parâmetros · Editar apelido, tipo e média de km (sem JSON).</summary>
    /// <param name="id">Id do veículo (uuid)</param>
    /// <param name="p">Novos dados</param>
    /// <param name="ct"></param>
    /// <response code="200">Veículo atualizado.</response>
    /// <response code="400">Faltou um campo obrigatório ou um valor é inválido.</response>
    /// <response code="404">Veículo não existe ou é de outra pessoa.</response>
    /// <response code="409">APELIDO_DUPLICADO: já existe outro veículo com este apelido.</response>
    [HttpPut("{id:guid}/rapido")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<VeiculoResponse> EditarPorParametros(Guid id, [FromQuery] VeiculoParametros p, CancellationToken ct) =>
        veiculos.EditarAsync(id, UsuarioId, new EditarVeiculoRequest(p.Apelido ?? "", p.Tipo, p.MediaKmMes), ct);

    /// <summary>E23 · Atualizar a quilometragem atual (corpo JSON).</summary>
    /// <remarks>Recalcula os itens por uso (ex.: troca de óleo) e devolve os que mudaram de status.</remarks>
    /// <param name="id">Id do veículo (uuid)</param>
    /// <param name="req">Nova km. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="200">Km gravada e itens afetados.</response>
    /// <response code="400">Km faltando ou negativa.</response>
    /// <response code="404">Veículo não existe ou é de outra pessoa.</response>
    /// <response code="422">KM_MENOR: a km não pode diminuir.</response>
    [HttpPatch("{id:guid}/km")]
    [ProducesResponseType<AtualizarKmResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<AtualizarKmResponse> AtualizarKm(Guid id, AtualizarKmRequest req, CancellationToken ct) =>
        veiculos.AtualizarKmAsync(id, UsuarioId, req, ct);

    /// <summary>E23 por parâmetros · Atualizar a quilometragem atual (sem JSON).</summary>
    /// <param name="id">Id do veículo (uuid)</param>
    /// <param name="kmAtual">Km atual do painel; não pode ser menor que a anterior</param>
    /// <param name="ct"></param>
    /// <response code="200">Km gravada e itens afetados.</response>
    /// <response code="400">Faltou a km ("Campo obrigatório.") ou ela é negativa.</response>
    /// <response code="404">Veículo não existe ou é de outra pessoa.</response>
    /// <response code="422">KM_MENOR: a km não pode diminuir.</response>
    [HttpPatch("{id:guid}/km/rapido")]
    [ProducesResponseType<AtualizarKmResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<AtualizarKmResponse> AtualizarKmPorParametros(Guid id, [FromQuery, BindRequired] int kmAtual, CancellationToken ct) =>
        veiculos.AtualizarKmAsync(id, UsuarioId, new AtualizarKmRequest(kmAtual), ct);

    /// <summary>E24 · Excluir veículo.</summary>
    /// <remarks>Bloqueado enquanto houver itens ligados a ele: exclua ou mude esses itens antes.</remarks>
    /// <param name="id">Id do veículo (uuid)</param>
    /// <param name="ct"></param>
    /// <response code="204">Excluído.</response>
    /// <response code="404">Veículo não existe ou é de outra pessoa.</response>
    /// <response code="409">VEICULO_COM_ITENS: há itens ligados a este veículo.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await veiculos.ExcluirAsync(id, UsuarioId, ct);
        return NoContent();
    }
}
