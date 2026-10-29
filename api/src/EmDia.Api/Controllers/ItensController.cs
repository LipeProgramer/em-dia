using EmDia.Application.DTOs.Itens;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EmDia.Api.Controllers;

/// <summary>E13–E19 · Itens.</summary>
[Route("api/v1/itens")]
public class ItensController(ItemService itens) : ApiControllerBase
{
    /// <summary>E13 · Listar itens.</summary>
    /// <remarks>Sem filtros, traz todos os itens da conta, do mais urgente para o menos urgente.</remarks>
    /// <param name="categoriaId">Só uma área: 1 Veículo · 2 Documentos · 3 Casa · 4 Saúde · 5 Finanças · 6 Garantias</param>
    /// <param name="status">Só um status</param>
    /// <param name="ct"></param>
    /// <response code="200">Lista resumida (id, nome, status, prazo e progresso dos passos).</response>
    /// <response code="400">Filtro inválido (categoria fora de 1 a 6 ou status desconhecido).</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ItemResumoResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IReadOnlyList<ItemResumoResponse>> Listar([FromQuery] short? categoriaId, [FromQuery] string? status,
        CancellationToken ct) => itens.ListarAsync(UsuarioId, categoriaId, status, ct);

    /// <summary>E14 · Detalhe do item, com passos e histórico recente.</summary>
    /// <remarks>O id vem da resposta do cadastro, do GET /api/v1/itens ou do botão "Copiar" no detalhe do item no site.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="ct"></param>
    /// <response code="200">Item completo: prazo calculado, passos e últimas conclusões.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ItemDetalheResponse> Obter(Guid id, CancellationToken ct) => itens.ObterAsync(id, UsuarioId, ct);

    /// <summary>E15 · Criar item (corpo JSON, usado pelo site e pelo app).</summary>
    /// <remarks>Para testar sem JSON, use POST /api/v1/itens/rapido.</remarks>
    /// <param name="req">Dados do item. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="201">Item criado; o corpo traz o id.</response>
    /// <response code="400">Campo faltando ou inválido; "errors" traz a mensagem de cada campo.</response>
    /// <response code="404">Modelo ou veículo não encontrado.</response>
    /// <response code="422">VEICULO_OBRIGATORIO: item por uso (km) sem veículo.</response>
    [HttpPost]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar(SalvarItemRequest req, CancellationToken ct)
    {
        var item = await itens.CriarAsync(UsuarioId, req, ct);
        return Created($"/api/v1/itens/{item.Id}", item);
    }

    /// <summary>E15 por parâmetros · Criar item preenchendo cada campo (sem JSON).</summary>
    /// <remarks>Mesmas validações e regras do E15. Ex.: nome = CNH, categoriaId = 2, tipoVencimento = DataUnica,
    /// dataVencimento = 2026-11-19.</remarks>
    /// <param name="p">Campos do item</param>
    /// <param name="ct"></param>
    /// <response code="201">Item criado; copie o "id" para usar no GET, PUT, DELETE e concluir.</response>
    /// <response code="400">Faltou um campo obrigatório ("Campo obrigatório.") ou um valor é inválido.</response>
    /// <response code="404">Veículo não encontrado.</response>
    /// <response code="422">VEICULO_OBRIGATORIO: item por uso (km) sem veículo.</response>
    [HttpPost("rapido")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarPorParametros([FromQuery] ItemParametros p, CancellationToken ct)
    {
        var item = await itens.CriarAsync(UsuarioId, p.ParaRequest(), ct);
        return Created($"/api/v1/itens/{item.Id}", item);
    }

    /// <summary>E16 · Editar item (corpo JSON).</summary>
    /// <remarks>Para testar sem JSON, use PUT /api/v1/itens/{id}/rapido.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="req">Todos os campos do item, já com as mudanças.</param>
    /// <param name="ct"></param>
    /// <response code="200">Item atualizado, com o prazo recalculado.</response>
    /// <response code="400">Campo faltando ou inválido.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    /// <response code="422">Regra de negócio (ex.: item encerrado não pode ser editado).</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ItemDetalheResponse> Editar(Guid id, SalvarItemRequest req, CancellationToken ct) =>
        itens.EditarAsync(id, UsuarioId, req, ct);

    /// <summary>E16 por parâmetros · Editar item preenchendo cada campo (sem JSON).</summary>
    /// <remarks>Envie todos os campos como devem ficar; os passos enviados substituem os atuais.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="p">Campos do item</param>
    /// <param name="ct"></param>
    /// <response code="200">Item atualizado, com o prazo recalculado.</response>
    /// <response code="400">Faltou um campo obrigatório ou um valor é inválido.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    /// <response code="422">Regra de negócio (ex.: item encerrado não pode ser editado).</response>
    [HttpPut("{id:guid}/rapido")]
    [ProducesResponseType<ItemDetalheResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ItemDetalheResponse> EditarPorParametros(Guid id, [FromQuery] ItemParametros p, CancellationToken ct) =>
        itens.EditarAsync(id, UsuarioId, p.ParaRequest(), ct);

    /// <summary>E17 · Excluir item (e o histórico dele).</summary>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="ct"></param>
    /// <response code="204">Excluído.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await itens.ExcluirAsync(id, UsuarioId, ct);
        return NoContent();
    }

    /// <summary>E18 · Concluir e renovar (corpo JSON).</summary>
    /// <remarks>Grava no histórico e calcula o próximo prazo. Para testar sem JSON, use POST /api/v1/itens/{id}/concluir/rapido.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="req">Dados da conclusão. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="200">Item renovado, id do histórico e uma mensagem positiva.</response>
    /// <response code="400">Data faltando, futura ou valor inválido.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    /// <response code="422">PASSOS_PENDENTES (há passos não marcados e confirmarPassosPendentes = false), ITEM_ENCERRADO ou KM_MENOR.</response>
    [HttpPost("{id:guid}/concluir")]
    [ProducesResponseType<ConcluirResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ConcluirResponse> Concluir(Guid id, ConcluirRequest req, CancellationToken ct) =>
        itens.ConcluirAsync(id, UsuarioId, req, ct);

    /// <summary>E18 por parâmetros · Concluir e renovar (sem JSON).</summary>
    /// <remarks>Ex.: dataConclusao = 2026-10-15 e confirmarPassosPendentes = true.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="p">Dados da conclusão</param>
    /// <param name="ct"></param>
    /// <response code="200">Item renovado, id do histórico e uma mensagem positiva.</response>
    /// <response code="400">Faltou a data, ela é futura ou um valor é inválido.</response>
    /// <response code="404">Item não existe ou é de outra pessoa.</response>
    /// <response code="422">PASSOS_PENDENTES (há passos não marcados e confirmarPassosPendentes = false), ITEM_ENCERRADO ou KM_MENOR.</response>
    [HttpPost("{id:guid}/concluir/rapido")]
    [ProducesResponseType<ConcluirResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<ConcluirResponse> ConcluirPorParametros(Guid id, [FromQuery] ConcluirParametros p, CancellationToken ct) =>
        itens.ConcluirAsync(id, UsuarioId, p.ParaRequest(), ct);

    /// <summary>E19 · Marcar ou desmarcar um passo (corpo JSON).</summary>
    /// <remarks>Os ids dos passos vêm do GET /api/v1/itens/{id}.</remarks>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="passoId">Id do passo (uuid)</param>
    /// <param name="req">true marca, false desmarca</param>
    /// <param name="ct"></param>
    /// <response code="200">Situação do passo e o novo progresso (0 a 1).</response>
    /// <response code="400">Faltou "concluido".</response>
    /// <response code="404">Item ou passo não encontrado.</response>
    /// <response code="422">Item encerrado não pode ser alterado.</response>
    [HttpPatch("{id:guid}/passos/{passoId:guid}")]
    [ProducesResponseType<MarcarPassoResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<MarcarPassoResponse> MarcarPasso(Guid id, Guid passoId, MarcarPassoRequest req, CancellationToken ct) =>
        itens.MarcarPassoAsync(id, passoId, UsuarioId, req, ct);

    /// <summary>E19 por parâmetros · Marcar ou desmarcar um passo (sem JSON).</summary>
    /// <param name="id">Id do item (uuid)</param>
    /// <param name="passoId">Id do passo (uuid), do GET /api/v1/itens/{id}</param>
    /// <param name="concluido">true marca, false desmarca</param>
    /// <param name="ct"></param>
    /// <response code="200">Situação do passo e o novo progresso (0 a 1).</response>
    /// <response code="400">Faltou "concluido" ("Campo obrigatório.").</response>
    /// <response code="404">Item ou passo não encontrado.</response>
    /// <response code="422">Item encerrado não pode ser alterado.</response>
    [HttpPatch("{id:guid}/passos/{passoId:guid}/rapido")]
    [ProducesResponseType<MarcarPassoResponse>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public Task<MarcarPassoResponse> MarcarPassoPorParametros(Guid id, Guid passoId, [FromQuery, BindRequired] bool concluido,
        CancellationToken ct) =>
        itens.MarcarPassoAsync(id, passoId, UsuarioId, new MarcarPassoRequest(concluido), ct);
}
