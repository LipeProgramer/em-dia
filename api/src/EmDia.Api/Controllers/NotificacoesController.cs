using EmDia.Application.DTOs.Notificacoes;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>
/// Extra (DV20) · Notificações no celular: inscrever e remover o aparelho e enviar um teste.
/// A inscrição é feita pelo navegador (Perfil → Notificações no celular): só ele gera o endereço e as chaves do
/// aparelho, por isso essas rotas não têm versão por parâmetros.
/// </summary>
[Route("api/v1/push")]
public class NotificacoesController(NotificacoesService notificacoes) : ApiControllerBase
{
    /// <summary>Chave pública VAPID usada pelo navegador para se inscrever.</summary>
    /// <response code="200">Chave em base64url.</response>
    [HttpGet("chave-publica")]
    [ProducesResponseType<ChavePublicaResponse>(StatusCodes.Status200OK)]
    public ChavePublicaResponse ChavePublica() => notificacoes.ChavePublica();

    /// <summary>Inscreve este aparelho para receber os avisos.</summary>
    /// <remarks>Corpo = PushSubscription.toJSON() do navegador. Só aceita endereços HTTPS dos serviços de push
    /// do Google, Mozilla, Microsoft e Apple.</remarks>
    /// <param name="req">Endereço e chaves do aparelho</param>
    /// <param name="ct"></param>
    /// <response code="204">Aparelho inscrito (ou atualizado, se já estava).</response>
    /// <response code="400">Endereço fora dos serviços aceitos ou chave faltando.</response>
    [HttpPost("inscricoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inscrever(InscricaoPushRequest req, CancellationToken ct)
    {
        await notificacoes.InscreverAsync(UsuarioId, req, ct);
        return NoContent();
    }

    /// <summary>Para de enviar para este aparelho.</summary>
    /// <param name="req">Endereço do aparelho</param>
    /// <param name="ct"></param>
    /// <response code="204">Aparelho removido (ou já não estava inscrito).</response>
    [HttpDelete("inscricoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(RemoverInscricaoRequest req, CancellationToken ct)
    {
        await notificacoes.RemoverAsync(UsuarioId, req.Endpoint, ct);
        return NoContent();
    }

    /// <summary>Envia uma notificação de teste para os aparelhos da pessoa.</summary>
    /// <response code="200">Quantos aparelhos receberam (0 = nenhum aparelho inscrito).</response>
    [HttpPost("teste")]
    [ProducesResponseType<TesteNotificacaoResponse>(StatusCodes.Status200OK)]
    public Task<TesteNotificacaoResponse> Testar(CancellationToken ct) => notificacoes.TestarAsync(UsuarioId, ct);
}
