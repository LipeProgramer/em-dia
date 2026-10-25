using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Radar;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Enums;
using EmDia.Domain.Services;

namespace EmDia.Application.Services;

/// <summary>
/// E10: GET /api/v1/radar — lista itens agrupados por anel (RN03/RN04).
/// </summary>
public class RadarService
{
    private readonly IItemRepository _itens;
    private readonly IVeiculoRepository _veiculos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IRelogio _relogio;

    public RadarService(
        IItemRepository itens,
        IVeiculoRepository veiculos,
        IUsuarioRepository usuarios,
        IRelogio relogio)
    {
        _itens    = itens;
        _veiculos = veiculos;
        _usuarios = usuarios;
        _relogio  = relogio;
    }

    public async Task<RadarResponse> ObterAsync(Guid usuarioId, CancellationToken ct)
    {
        var hoje     = _relogio.Hoje;
        var todos    = await _itens.ListarPorUsuarioAsync(usuarioId, ct);
        var veiculos = await _veiculos.ListarPorUsuarioAsync(usuarioId, ct);
        var usuario  = await _usuarios.ObterPorIdAsync(usuarioId, ct);

        // Dicionário km atual por veículo para cálculo de status
        var kmPorVeiculo = veiculos.ToDictionary(v => v.Id, v => v.KmAtual);

        int? ObterKm(Domain.Entities.Item i) =>
            i.VeiculoId.HasValue && kmPorVeiculo.TryGetValue(i.VeiculoId.Value, out var km) ? km : null;

        // Filtrar encerrados
        var ativos = todos.Where(i => i.Situacao != SituacaoItem.Encerrado).ToList();

        // Modo foco: preferência do usuário
        var modoFoco = usuario?.Preferencias?.ModoFocoPadrao ?? false;

        IEnumerable<Domain.Entities.Item> itensRadar = modoFoco
            ? RadarDomain.AplicarModoFoco(ativos, hoje, ObterKm)
            : RadarDomain.OrdenarPorUrgencia(ativos, hoje, ObterKm);

        // Agrupar por anel
        var grupos = itensRadar.GroupBy(i => i.CalcularAnel(hoje, ObterKm(i)));

        ItemRadarResponse ToItemRadar(Domain.Entities.Item i) => new(
            i.Id,
            i.Nome,
            i.CategoriaId,
            i.CalcularAnel(hoje, ObterKm(i)),
            i.CalcularStatus(hoje, ObterKm(i)),
            i.ProximoVencimentoData,
            i.ProximoVencimentoKm,
            i.DiasRestantes(hoje),
            i.ProgressoPassos(),
            i.Passos.OrderBy(p => p.Ordem)
                .Select(p => new PassoResponse(p.Id, p.Ordem, p.Descricao, p.Concluido, p.ConcluidoEm))
                .ToList());

        List<ItemRadarResponse> Grupo(AnelRadar anel) =>
            grupos.FirstOrDefault(g => g.Key == anel)?.Select(ToItemRadar).ToList()
            ?? new List<ItemRadarResponse>();

        return new RadarResponse(
            Grupo(AnelRadar.Centro),
            Grupo(AnelRadar.Anel1),
            Grupo(AnelRadar.Anel2),
            Grupo(AnelRadar.Externo),
            modoFoco);
    }
}
