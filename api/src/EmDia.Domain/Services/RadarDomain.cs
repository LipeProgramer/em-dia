using EmDia.Domain.Entities;
using EmDia.Domain.Enums;

namespace EmDia.Domain.Services;

/// <summary>
/// Lógica de domínio do Radar que opera sobre coleções de itens.
/// RN03 (anel), RN04 (modo foco), RN11 (resultado do check-up).
/// </summary>
public static class RadarDomain
{
    // ── RN04 — Modo Foco ──────────────────────────────────────────────────────

    /// <summary>
    /// RN04: retorna os 3 itens mais urgentes.
    /// Vencidos primeiro (mais atrasados antes), depois por menor prazo restante.
    /// </summary>
    public static IEnumerable<Item> AplicarModoFoco(
        IEnumerable<Item> itens,
        DateOnly hoje,
        Func<Item, int?> obterKmAtualVeiculo)
    {
        return itens
            .Select(i => new
            {
                Item = i,
                Status = i.CalcularStatus(hoje, obterKmAtualVeiculo(i)),
                Dias = i.DiasRestantes(hoje)
            })
            .OrderBy(x => x.Status == StatusItem.Vencido ? 0 : 1)
            .ThenBy(x => x.Dias)
            .Take(3)
            .Select(x => x.Item);
    }

    /// <summary>
    /// Ordena itens do Radar: mais urgente primeiro.
    /// Vencidos → Atenção → Em Dia; dentro de cada grupo, menor prazo primeiro.
    /// </summary>
    public static IEnumerable<Item> OrdenarPorUrgencia(
        IEnumerable<Item> itens,
        DateOnly hoje,
        Func<Item, int?> obterKmAtualVeiculo)
    {
        return itens
            .Select(i => new
            {
                Item = i,
                Status = i.CalcularStatus(hoje, obterKmAtualVeiculo(i)),
                Dias = i.DiasRestantes(hoje)
            })
            .OrderBy(x => x.Status switch
            {
                StatusItem.Vencido => 0,
                StatusItem.Atencao => 1,
                StatusItem.EmDia => 2,
                _ => 3
            })
            .ThenBy(x => x.Dias)
            .Select(x => x.Item);
    }

    // ── RN11 — Resultado do check-up ──────────────────────────────────────────

    /// <summary>
    /// RN11: conta itens por status e por categoria (ex.: "4 de 6 áreas em dia").
    /// </summary>
    public static ResultadoCheckup GerarResultado(
        IEnumerable<Item> itens,
        DateOnly hoje,
        Func<Item, int?> obterKmAtualVeiculo)
    {
        var lista = itens.ToList();
        int vencidos = 0, atencao = 0, emDia = 0;

        foreach (var item in lista)
        {
            var status = item.CalcularStatus(hoje, obterKmAtualVeiculo(item));
            switch (status)
            {
                case StatusItem.Vencido: vencidos++; break;
                case StatusItem.Atencao: atencao++; break;
                default: emDia++; break;
            }
        }

        var porCategoria = lista
            .GroupBy(i => i.CategoriaId)
            .Select(g =>
            {
                // Status da área = pior status dos itens dela
                var statuses = g.Select(i => i.CalcularStatus(hoje, obterKmAtualVeiculo(i)));
                var pior = statuses.Contains(StatusItem.Vencido) ? StatusItem.Vencido
                         : statuses.Contains(StatusItem.Atencao) ? StatusItem.Atencao
                         : StatusItem.EmDia;
                return new StatusPorCategoria(g.Key, pior);
            })
            .ToList();

        return new ResultadoCheckup(vencidos, atencao, emDia, porCategoria);
    }
}

public record ResultadoCheckup(
    int Vencidos,
    int Atencao,
    int EmDia,
    IReadOnlyList<StatusPorCategoria> Areas);

public record StatusPorCategoria(short CategoriaId, StatusItem Status);
