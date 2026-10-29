using EmDia.Application.DTOs.Notificacoes;
using EmDia.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Controllers;

/// <summary>Extra (DV20) · Notificações no celular: inscrever e remover o aparelho e enviar um teste.</summary>
[Route("api/v1/push")]
public class NotificacoesController(NotificacoesService notificacoes) : ApiControllerBase
{
    /// <summary>Chave pública VAPID usada pelo navegador para se inscrever.</summary>
    [HttpGet("chave-publica")]
    public ChavePublicaResponse ChavePublica() => notificacoes.ChavePublica();

    /// <summary>Inscreve este aparelho para receber os avisos (só serviços de push conhecidos).</summary>
    [HttpPost("inscricoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent), ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inscrever(InscricaoPushRequest req, CancellationToken ct)
    {
        await notificacoes.InscreverAsync(UsuarioId, req, ct);
        return NoContent();
    }

    /// <summary>Para de enviar para este aparelho.</summary>
    [HttpDelete("inscricoes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(RemoverInscricaoRequest req, CancellationToken ct)
    {
        await notificacoes.RemoverAsync(UsuarioId, req.Endpoint, ct);
        return NoContent();
    }

    /// <summary>Envia uma notificação de teste para os aparelhos do usuário.</summary>
    [HttpPost("teste")]
    public Task<TesteNotificacaoResponse> Testar(CancellationToken ct) => notificacoes.TestarAsync(UsuarioId, ct);
}
