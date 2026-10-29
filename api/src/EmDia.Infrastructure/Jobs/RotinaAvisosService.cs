using EmDia.Application.Interfaces;
using EmDia.Domain;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Services;
using EmDia.Infrastructure.Data;
using EmDia.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmDia.Infrastructure.Jobs;

public record ResultadoRotinaAvisos(int TotalProcessados, int TotalEnviados, int TotalErros);

/// <summary>
/// Rotina diária de avisos por e-mail (RN08/RN09 · RF10/RF11) e, para quem ativou, notificação no celular (DV20).
/// Para cada usuário que aceita e-mail ou tem aparelho inscrito, avalia os itens ativos e envia no máximo um aviso
/// por item e dia. Cada envio é registrado em <c>avisos</c> (único por item, vencimento e número), o que evita
/// duplicidade mesmo se a rotina rodar várias vezes no mesmo dia.
/// </summary>
public class RotinaAvisosService(
    EmDiaDbContext db,
    IEmailService email,
    IPushService push,
    IRelogio relogio,
    ILogger<RotinaAvisosService> logger)
{
    public async Task<ResultadoRotinaAvisos> ExecutarRotinaAsync(CancellationToken ct = default)
    {
        var hoje = relogio.Hoje;
        int processados = 0, enviados = 0, erros = 0;

        var comAparelho = (await db.InscricoesPush.Select(i => i.UsuarioId).Distinct().ToListAsync(ct)).ToHashSet();
        var usuarios = await db.Usuarios
            .Include(u => u.Preferencias)
            .Where(u => u.Preferencias != null && (u.Preferencias.ReceberEmail || comAparelho.Contains(u.Id)))
            .ToListAsync(ct);

        foreach (var usuario in usuarios)
        {
            var prefs = usuario.Preferencias!;
            var veiculos = await db.Veiculos.Where(v => v.UsuarioId == usuario.Id).ToDictionaryAsync(v => v.Id, ct);
            var itens = await db.Itens.Include(i => i.Passos)
                .Where(i => i.UsuarioId == usuario.Id && i.Situacao == SituacaoItem.Ativo && i.ProximoVencimentoData != null)
                .ToListAsync(ct);

            foreach (var item in itens)
            {
                processados++;
                var veiculo = item.VeiculoId is { } vid ? veiculos.GetValueOrDefault(vid) : null;
                var status = item.CalcularStatus(hoje, veiculo);
                var referencia = item.ProximoVencimentoData!.Value; // identifica o ciclo
                var avisos = await db.Avisos.Where(a => a.ItemId == item.Id).ToListAsync(ct);

                var decisao = AvisoDomain.VerificarAviso(avisos, hoje, status, referencia,
                    prefs.MaxReavisos, prefs.IntervaloReavisoDias, RelogioSaoPaulo.DataLocal);
                if (!decisao.Enviar)
                    continue;

                // Uma tentativa anterior com falha para o mesmo número é reaproveitada (nova tentativa)
                var aviso = avisos.FirstOrDefault(a => a.VencimentoReferencia == referencia && a.Numero == decisao.Numero);
                if (aviso is null)
                {
                    var canal = prefs.ReceberEmail ? CanalAviso.Email : CanalAviso.Push;
                    aviso = new Aviso(item.Id, decisao.Tipo, canal, referencia, decisao.Numero,
                        relogio.Agora, sucesso: false);
                    db.Avisos.Add(aviso);
                }

                // O aviso conta como entregue se chegou por pelo menos um dos canais
                var entregue = false;
                string? falha = null;
                if (prefs.ReceberEmail)
                {
                    try
                    {
                        await email.EnviarAsync(usuario.Email, Assunto(item, status, decisao),
                            Corpo(usuario, item, veiculo, hoje), ct);
                        entregue = true;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        falha = ex.Message;
                        logger.LogError(ex, "Falha ao enviar aviso do item {ItemId}", item.Id);
                    }
                }

                if (comAparelho.Contains(usuario.Id))
                {
                    try
                    {
                        if (await push.EnviarAsync(usuario.Id, Notificacao(item, status, decisao, veiculo, hoje), ct) > 0)
                            entregue = true;
                        else
                            falha ??= "Nenhum aparelho recebeu a notificação.";
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        falha ??= ex.Message;
                        logger.LogError(ex, "Falha ao enviar notificação do item {ItemId}", item.Id);
                    }
                }

                aviso.RegistrarResultado(entregue, entregue ? null : falha, relogio.Agora);
                if (entregue) enviados++;
                else erros++;

                await db.SaveChangesAsync(ct);
            }
        }

        logger.LogInformation("Rotina de avisos: {Processados} itens, {Enviados} enviados, {Erros} erros",
            processados, enviados, erros);
        return new ResultadoRotinaAvisos(processados, enviados, erros);
    }

    private static string Assunto(Item item, StatusItem status, DeveAvisar decisao)
    {
        var situacao = status == StatusItem.Vencido ? "precisa de atenção" : "está chegando";
        return decisao.Tipo == TipoAviso.Antecipado
            ? $"Em Dia · {item.Nome} {situacao}"
            : $"Em Dia · Lembrete: {item.Nome} {situacao}";
    }

    /// <summary>Notificação no celular: o mesmo aviso do e-mail, em uma linha; tocar abre o item.</summary>
    private static NotificacaoPush Notificacao(Item item, StatusItem status, DeveAvisar decisao, Veiculo? veiculo, DateOnly hoje)
    {
        var titulo = Assunto(item, status, decisao).Replace("Em Dia · ", "");
        var passo = item.Passos.FirstOrDefault(p => !p.Concluido)?.Descricao;
        var prazo = item.TextoPrazo(hoje, veiculo);
        var corpo = $"{char.ToUpper(prazo[0])}{prazo[1..]}." + (passo is null ? "" : $" Próximo passo: {passo}");
        return new NotificacaoPush(titulo, corpo, $"/itens/{item.Id}");
    }

    /// <summary>Texto curto, positivo e sem culpa (RN15).</summary>
    private static string Corpo(Usuario usuario, Item item, Veiculo? veiculo, DateOnly hoje)
    {
        var nome = System.Net.WebUtility.HtmlEncode(usuario.Nome);
        var itemNome = System.Net.WebUtility.HtmlEncode(item.Nome);
        var prazo = item.TextoPrazo(hoje, veiculo);
        var data = item.DataEfetiva(hoje, veiculo) is { } d ? Formatos.Data(d) : "";
        var passo = item.Passos.FirstOrDefault(p => !p.Concluido)?.Descricao;
        var proximoPasso = passo is null ? "" :
            $"<p>Próximo passo pequeno: <strong>{System.Net.WebUtility.HtmlEncode(passo)}</strong></p>";

        return $"""
            <div style="font-family:sans-serif;max-width:560px;margin:0 auto;padding:20px">
              <h2 style="color:#0f756e;margin:0 0 12px">Em Dia</h2>
              <p>Olá, {nome}!</p>
              <p><strong>{itemNome}</strong>: {prazo} ({data}).</p>
              {proximoPasso}
              <p>Abra o seu Radar para marcar os passos ou concluir quando terminar.</p>
            </div>
            """;
    }
}
