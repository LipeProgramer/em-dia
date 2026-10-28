using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.Interfaces;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Enums;
using EmDia.Domain.Exceptions;
using EmDia.Domain.Services;

namespace EmDia.Application.Services;

/// <summary>E12: monta o Radar (RN02, RN03 e, no modo foco, RN04).</summary>
public class RadarService(
    IItemRepository itens,
    IVeiculoRepository veiculos,
    IUsuarioRepository usuarios,
    IRelogio relogio)
{
    /// <param name="foco">Quando nulo, usa a preferência do usuário (modoFocoPadrao).</param>
    public async Task<RadarResponse> ObterAsync(Guid usuarioId, bool? foco, CancellationToken ct)
    {
        var hoje = relogio.Hoje;
        var usuario = await usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new NotFoundException("USUARIO_NAO_ENCONTRADO", "Não encontramos sua conta.");
        var modoFoco = foco ?? usuario.Preferencias?.ModoFocoPadrao ?? false;

        var porId = (await veiculos.ListarPorUsuarioAsync(usuarioId, ct)).ToDictionary(v => v.Id);
        Veiculo? VeiculoDo(Item i) => i.VeiculoId is { } id ? porId.GetValueOrDefault(id) : null;

        // Só itens ativos do usuário, do mais urgente para o menos urgente
        var ativos = (await itens.ListarPorUsuarioAsync(usuarioId, ct)).Where(i => !i.Encerrado).ToList();
        var ordenados = RadarDomain.OrdenarPorUrgencia(ativos, hoje, VeiculoDo);
        if (modoFoco)
            ordenados = ordenados.Take(RadarDomain.ItensModoFoco);

        var statuses = ativos.Select(i => i.CalcularStatus(hoje, VeiculoDo(i))).ToList();
        var resumo = new ResumoStatus(
            statuses.Count(s => s == StatusItem.Vencido),
            statuses.Count(s => s == StatusItem.Atencao),
            statuses.Count(s => s == StatusItem.EmDia));

        return new RadarResponse(resumo, modoFoco,
            ordenados.Select(i => Mapeamento.Resumo(i, hoje, VeiculoDo(i))).ToList());
    }
}
