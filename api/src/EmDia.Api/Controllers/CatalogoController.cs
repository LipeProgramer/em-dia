using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>E08–E11 · Catálogo e check-up.</summary>
[Route("api/v1")]
public class CatalogoController(CatalogoService catalogo, CheckupService checkup) : ApiControllerBase
{
    private const short AreaVeiculo = 1;

    /// <summary>E08 · Listar as 6 categorias (áreas) do Radar.</summary>
    /// <response code="200">Id, nome, ícone e ordem de cada área: 1 Veículo · 2 Documentos · 3 Casa · 4 Saúde · 5 Finanças · 6 Garantias.</response>
    [HttpGet("categorias")]
    [ProducesResponseType<IReadOnlyList<CategoriaResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<CategoriaResponse>> Categorias(CancellationToken ct) => catalogo.ListarCategoriasAsync(ct);

    /// <summary>E09 · Listar modelos prontos (só os ativos).</summary>
    /// <remarks>Os ids dos modelos servem para o check-up (modeloId). Ex.: CNH a7000000-0000-0000-0000-000000000003.</remarks>
    /// <param name="categoriaId">Só uma área (1 a 6)</param>
    /// <param name="ct"></param>
    /// <response code="200">Modelos com tipo de vencimento, intervalos sugeridos e passos.</response>
    /// <response code="400">Categoria fora de 1 a 6.</response>
    [HttpGet("modelos")]
    [ProducesResponseType<IReadOnlyList<ModeloResponse>>(StatusCodes.Status200OK), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<IReadOnlyList<ModeloResponse>> Modelos([FromQuery] short? categoriaId, CancellationToken ct) =>
        catalogo.ListarModelosAsync(categoriaId, ct);

    /// <summary>E10 · Perguntas do check-up, por área.</summary>
    /// <response code="200">Código, texto, ajuda e os modelos que cada resposta "sim" ativa.</response>
    [HttpGet("checkup/perguntas")]
    [ProducesResponseType<IReadOnlyList<PerguntaResponse>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<PerguntaResponse>> Perguntas(CancellationToken ct) => catalogo.ListarPerguntasAsync(ct);

    /// <summary>E11 · Fazer o check-up e criar os itens de uma vez (corpo JSON).</summary>
    /// <remarks>Tudo numa transação: se algo falhar, nada é criado. Refazer com os mesmos dados não duplica itens.
    /// Para testar sem JSON, use POST /api/v1/checkup/rapido.</remarks>
    /// <param name="req">Respostas, veículos e itens. O exemplo já é válido.</param>
    /// <param name="ct"></param>
    /// <response code="201">Resumo: itens criados, status por área e destaques.</response>
    /// <response code="400">Pergunta ou modelo inexistente, item sem resposta "sim", ou data faltando.</response>
    /// <response code="409">APELIDO_DUPLICADO: dois veículos com o mesmo apelido.</response>
    /// <response code="422">VEICULO_OBRIGATORIO: item de veículo sem veículo.</response>
    [HttpPost("checkup")]
    [ProducesResponseType<CheckupResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict), ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Checkup(CheckupRequest req, CancellationToken ct)
    {
        var resposta = await checkup.RealizarAsync(UsuarioId, req, ct);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    /// <summary>E11 por parâmetros · Fazer o check-up (sem JSON), com até um veículo e um item.</summary>
    /// <remarks>Ex.: sim = TEM_CNH, modeloId = a7000000-0000-0000-0000-000000000003, dataVencimento = 2027-03-10.
    /// Com veículo: sim = TEM_CARRO, veiculoApelido = Gol, veiculoKmAtual = 50000,
    /// modeloId = a7000000-0000-0000-0000-000000000001 (Troca de óleo), dataBase = 2026-06-01, kmBase = 45000.</remarks>
    /// <param name="p">Respostas, veículo e item</param>
    /// <param name="ct"></param>
    /// <response code="201">Resumo: itens criados, status por área e destaques.</response>
    /// <response code="400">Faltou "sim" ("Campo obrigatório."), modelo sem resposta "sim" ligada a ele, ou data faltando.</response>
    /// <response code="422">VEICULO_OBRIGATORIO: item de veículo sem veiculoApelido.</response>
    [HttpPost("checkup/rapido")]
    [ProducesResponseType<CheckupResponse>(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CheckupPorParametros([FromQuery] CheckupParametros p, CancellationToken ct)
    {
        var itemDaAreaVeiculo = false;
        if (p.ModeloId is { } modeloId)
            itemDaAreaVeiculo = (await catalogo.ListarModelosAsync(AreaVeiculo, ct)).Any(m => m.Id == modeloId);

        var resposta = await checkup.RealizarAsync(UsuarioId, p.ParaRequest(itemDaAreaVeiculo), ct);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }
}
