using EmDia.Domain.Entities;
using EmDia.Domain.Enums;

namespace EmDia.Domain.Services;

/// <summary>
/// Regras do Radar que operam sobre coleções de itens:
/// ordenação por urgência, RN04 (modo foco) e RN11 (resultado do check-up).
/// </summary>
public static class RadarDomain
{
    public const int ItensModoFoco = 3;

    /// <summary>
    /// Ordena do mais urgente para o menos urgente: vencidos primeiro (os mais atrasados antes),
    /// depois pelo menor prazo restante.
    /// </summary>
    public static IEnumerable<Item> OrdenarPorUrgencia(
        IEnumerable<Item> itens, DateOnly hoje, Func<Item, Veiculo?> veiculoDo)
    {
        return itens
            .Select(i => new
            {
                Item = i,
                Vencido = i.CalcularStatus(hoje, veiculoDo(i)) == StatusItem.Vencido,
                Dias = i.DiasRestantes(hoje, veiculoDo(i)) ?? int.MaxValue
            })
            .OrderBy(x => x.Vencido ? 0 : 1)
            .ThenBy(x => x.Dias)
            .Select(x => x.Item);
    }

    /// <summary>RN04: os 3 itens mais urgentes.</summary>
    public static IEnumerable<Item> AplicarModoFoco(
        IEnumerable<Item> itens, DateOnly hoje, Func<Item, Veiculo?> veiculoDo) =>
        OrdenarPorUrgencia(itens, hoje, veiculoDo).Take(ItensModoFoco);

    /// <summary>
    /// RN11: conta itens por status e por categoria (ex.: "4 de 6 áreas em dia").
    /// A situação de uma área é o pior status dos itens dela; área sem itens conta como em dia.
    /// </summary>
    public static ResultadoCheckup GerarResultado(
        IEnumerable<Item> itens, IEnumerable<short> categorias, DateOnly hoje, Func<Item, Veiculo?> veiculoDo)
    {
        var ativos = itens.Where(i => !i.Encerrado)
            .Select(i => (Item: i, Status: i.CalcularStatus(hoje, veiculoDo(i))))
            .ToList();

        var areas = categorias
            .Select(c =>
            {
                var statuses = ativos.Where(x => x.Item.CategoriaId == c).Select(x => x.Status).ToList();
                var pior = statuses.Contains(StatusItem.Vencido) ? StatusItem.Vencido
                         : statuses.Contains(StatusItem.Atencao) ? StatusItem.Atencao
                         : StatusItem.EmDia;
                return new StatusPorCategoria(c, pior, statuses.Count);
            })
            .ToList();

        return new ResultadoCheckup(
            ativos.Count(x => x.Status == StatusItem.Vencido),
            ativos.Count(x => x.Status == StatusItem.Atencao),
            ativos.Count(x => x.Status == StatusItem.EmDia),
            areas);
    }
}

public record ResultadoCheckup(int Vencidos, int Atencao, int EmDia, IReadOnlyList<StatusPorCategoria> Areas)
{
    public int AreasEmDia => Areas.Count(a => a.Status == StatusItem.EmDia);
}

public record StatusPorCategoria(short CategoriaId, StatusItem Status, int QuantidadeItens);
