using System.Text.Json;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>
/// E11: POST /api/v1/checkup    — executa check-up e cria itens via modelos (RN11/RN14)
/// E12: GET  /api/v1/checkup/atual — retorna último checkup
/// </summary>
public class CheckupService
{
    private readonly ICheckupRepository _checkups;
    private readonly IUsuarioRepository _usuarios;
    private readonly IModeloItemRepository _modelos;
    private readonly IItemRepository _itens;
    private readonly IRelogio _relogio;

    public CheckupService(
        ICheckupRepository checkups,
        IUsuarioRepository usuarios,
        IModeloItemRepository modelos,
        IItemRepository itens,
        IRelogio relogio)
    {
        _checkups = checkups;
        _usuarios = usuarios;
        _modelos  = modelos;
        _itens    = itens;
        _relogio  = relogio;
    }

    /// <summary>E12 — retorna o último checkup ou null.</summary>
    public async Task<CheckupResponse?> ObterAtualAsync(Guid usuarioId, CancellationToken ct)
    {
        var checkup = await _checkups.ObterAtualPorUsuarioAsync(usuarioId, ct);
        return checkup is null ? null : ToResponse(checkup);
    }

    /// <summary>E11 — executa check-up: cria itens a partir dos modelos marcados como true.</summary>
    public async Task<CheckupResponse> ExecutarAsync(
        Guid usuarioId, ExecutarCheckupRequest req, CancellationToken ct)
    {
        var agora = _relogio.Agora;
        var hoje  = _relogio.Hoje;

        var usuario = await _usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Usuário não encontrado.");

        // RN11 — modelos com codigoCheckup respondidos como true
        var todosModelos = await _modelos.ListarAsync(ct);
        var itensExistentes = await _itens.ListarPorUsuarioAsync(usuarioId, ct);

        short itensCriados = 0;

        foreach (var modelo in todosModelos.Where(m => m.Ativo && m.CodigoCheckup is not null))
        {
            if (!req.Respostas.TryGetValue(modelo.CodigoCheckup!, out bool resposta) || !resposta)
                continue;

            // RN15 — não duplica item já existente para o mesmo modelo
            bool jaExiste = itensExistentes.Any(i =>
                i.ModeloItemId == modelo.Id &&
                i.Situacao != Domain.Enums.SituacaoItem.Encerrado);
            if (jaExiste) continue;

            // Tipos que exigem dados específicos do usuário (data de vencimento ou veículo)
            // não podem ser criados automaticamente pelo checkup — pular.
            // DataUnica exige dataVencimento; UsoOuTempo exige veiculoId + kmBase.
            if (modelo.TipoVencimento is Domain.Enums.TipoVencimento.DataUnica
                                      or Domain.Enums.TipoVencimento.UsoOuTempo)
                continue;

            // Para RecorrentePorTempo e Calculado: dataBase = hoje (início da contagem).
            var dataBase = hoje;

            var item = new Item(
                usuarioId,
                modelo.CategoriaId,
                modelo.Nome,
                modelo.TipoVencimento,
                dataBase: dataBase,
                dataVencimento: null,
                modelo.IntervaloDias,
                modelo.IntervaloKm,
                kmBase: null,
                modelo.GarantiaFabricanteMeses,
                modelo.AntecedenciaDias,
                modelo.Id,
                veiculoId: null,
                observacao: null,
                agora);

            // RN14 — copiar passos do modelo
            foreach (var passo in modelo.Passos.OrderBy(p => p.Ordem))
                item.AdicionarPasso(passo.Descricao);

            await _itens.AdicionarAsync(item, ct);
            itensCriados++;
        }

        if (itensCriados > 0)
            await _itens.SalvarAsync(ct);

        // Registrar checkup no usuário
        usuario.RegistrarCheckup(agora);
        await _usuarios.SalvarAsync(ct);

        var checkup = new Checkup(usuarioId, req.Respostas, itensCriados, agora);
        await _checkups.AdicionarAsync(checkup, ct);
        await _checkups.SalvarAsync(ct);

        return ToResponse(checkup);
    }

    private static CheckupResponse ToResponse(Checkup c) => new(
        c.Id,
        c.RealizadoEm,
        c.ItensCriados,
        JsonSerializer.Deserialize<Dictionary<string, bool>>(c.Respostas)
            ?? new Dictionary<string, bool>());
}
