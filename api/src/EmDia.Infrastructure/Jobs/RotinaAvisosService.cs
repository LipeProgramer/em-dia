using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Services;
using EmDia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmDia.Infrastructure.Jobs;

public record ResultadoRotinaAvisos(int TotalProcessados, int TotalEnviados, int TotalErros);

/// <summary>
/// Executa a rotina diária de avisos antecipados e reavisos por e-mail (RN08 / RN09).
/// </summary>
public class RotinaAvisosService
{
    private readonly EmDiaDbContext _db;
    private readonly IAvisoRepository _avisoRepo;
    private readonly IEmailService _email;
    private readonly IRelogio _relogio;
    private readonly ILogger<RotinaAvisosService> _logger;

    public RotinaAvisosService(
        EmDiaDbContext db,
        IAvisoRepository avisoRepo,
        IEmailService email,
        IRelogio relogio,
        ILogger<RotinaAvisosService> logger)
    {
        _db = db;
        _avisoRepo = avisoRepo;
        _email = email;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<ResultadoRotinaAvisos> ExecutarRotinaAsync(CancellationToken ct = default)
    {
        var hoje = _relogio.Hoje;
        var agora = _relogio.Agora;

        _logger.LogInformation("Iniciando rotina de avisos para data de referência: {Hoje}", hoje);

        // 1. Busca usuários com preferências ativas para receber e-mail
        var usuarios = await _db.Usuarios
            .Include(u => u.Preferencias)
            .Where(u => u.Preferencias != null && u.Preferencias.ReceberEmail)
            .ToListAsync(ct);

        int totalProcessados = 0;
        int totalEnviados = 0;
        int totalErros = 0;

        foreach (var usuario in usuarios)
        {
            var usuarioId = usuario.Id;
            var maxReavisos = usuario.Preferencias?.MaxReavisos ?? 2;
            var intervaloReaviso = usuario.Preferencias?.IntervaloReavisoDias ?? 3;

            // Busca veículos do usuário para mapear km atual
            var veiculos = await _db.Veiculos
                .Where(v => v.UsuarioId == usuarioId)
                .ToDictionaryAsync(v => v.Id, v => v.KmAtual, ct);

            // Busca itens ativos do usuário
            var itens = await _db.Itens
                .Include(i => i.Passos)
                .Where(i => i.UsuarioId == usuarioId && i.Situacao != SituacaoItem.Encerrado)
                .ToListAsync(ct);

            foreach (var item in itens)
            {
                totalProcessados++;

                // Determina km atual se tiver veículo associado
                int? kmAtual = item.VeiculoId.HasValue && veiculos.TryGetValue(item.VeiculoId.Value, out var km)
                    ? km
                    : null;

                // Calcula status do item hoje
                var status = item.CalcularStatus(hoje, kmAtual);

                // Só avisa se estiver em Atenção ou Vencido (RN08)
                if (status is not (StatusItem.Atencao or StatusItem.Vencido))
                    continue;

                // Vencimento de referência (data)
                var vencimentoRef = item.ProximoVencimentoData ?? hoje;

                // Busca avisos já enviados para esse item
                var avisosEnviados = await _avisoRepo.ListarPorItemAsync(item.Id, ct);

                // Verifica se deve avisar hoje conforme regras de domínio RN08 e RN09
                var decisao = AvisoDomain.VerificarAviso(
                    avisosEnviados,
                    hoje,
                    status,
                    vencimentoRef,
                    maxReavisos,
                    intervaloReaviso);

                if (!decisao.Enviar)
                    continue;

                // Evita duplicata no mesmo dia e número (CT40)
                var jaEnviouHoje = await _avisoRepo.JaEnviouHojeAsync(
                    item.Id,
                    vencimentoRef,
                    decisao.Numero,
                    ct);

                if (jaEnviouHoje)
                {
                    _logger.LogInformation(
                        "Item {ItemId} já recebeu o aviso número {Numero} hoje. Pulando duplicata.",
                        item.Id,
                        decisao.Numero);
                    continue;
                }

                // Monta e-mail amigável para adultos com TDAH
                var tipoLabel = decisao.Tipo == TipoAviso.Antecipado ? "Aviso Antecipado" : $"Lembrete #{decisao.Numero}";
                var statusTexto = status == StatusItem.Vencido ? "Vencido" : "Próximo do vencimento";
                var assunto = $"[Em Dia] {tipoLabel}: {item.Nome} ({statusTexto})";

                var corpoHtml = $@"
<div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;"">
    <h2 style=""color: #0f756e;"">Em Dia &bull; Radar de Vencimentos</h2>
    <p>Olá, <strong>{usuario.Nome}</strong>!</p>
    <p>Este é um aviso automático sobre o seu item cadastrado:</p>
    <div style=""background: #f8fafc; padding: 15px; border-radius: 6px; margin: 15px 0;"">
        <h3 style=""margin: 0 0 8px 0; color: #1e293b;"">{item.Nome}</h3>
        <p style=""margin: 4px 0;""><strong>Status:</strong> {statusTexto}</p>
        <p style=""margin: 4px 0;""><strong>Data de Vencimento:</strong> {vencimentoRef:dd/MM/yyyy}</p>
    </div>
    <p>Para conferir os passos ou marcar como concluído, acesse seu radar no aplicativo.</p>
</div>";

                try
                {
                    await _email.EnviarAsync(usuario.Email, assunto, corpoHtml, ct);

                    var aviso = new Aviso(
                        item.Id,
                        decisao.Tipo,
                        CanalAviso.Email,
                        vencimentoRef,
                        decisao.Numero,
                        agora,
                        sucesso: true);

                    await _avisoRepo.AdicionarAsync(aviso, ct);
                    await _avisoRepo.SalvarAsync(ct);

                    totalEnviados++;
                    _logger.LogInformation(
                        "Aviso {Tipo} #{Numero} enviado com sucesso para {Email} (Item: {ItemNome})",
                        decisao.Tipo,
                        decisao.Numero,
                        usuario.Email,
                        item.Nome);
                }
                catch (Exception ex)
                {
                    totalErros++;
                    _logger.LogError(ex, "Erro ao enviar e-mail de aviso para {Email}", usuario.Email);

                    var avisoFalho = new Aviso(
                        item.Id,
                        decisao.Tipo,
                        CanalAviso.Email,
                        vencimentoRef,
                        decisao.Numero,
                        agora,
                        sucesso: false,
                        erro: ex.Message);

                    await _avisoRepo.AdicionarAsync(avisoFalho, ct);
                    await _avisoRepo.SalvarAsync(ct);
                }
            }
        }

        _logger.LogInformation(
            "Rotina de avisos concluída. Processados: {Processados}, Enviados: {Enviados}, Erros: {Erros}",
            totalProcessados,
            totalEnviados,
            totalErros);

        return new ResultadoRotinaAvisos(totalProcessados, totalEnviados, totalErros);
    }
}
