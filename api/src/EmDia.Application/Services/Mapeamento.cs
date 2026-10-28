using EmDia.Application.DTOs.Itens;
using EmDia.Domain.Entities;

namespace EmDia.Application.Services;

/// <summary>Converte entidades em DTOs de saída. Status, anel e prazo são sempre calculados na leitura (RN02/RN03).</summary>
internal static class Mapeamento
{
    public static ItemResumoResponse Resumo(Item i, DateOnly hoje, Veiculo? veiculo) => new(
        i.Id,
        i.Nome,
        i.CategoriaId,
        i.TipoVencimento,
        i.CalcularStatus(hoje, veiculo),
        i.CalcularAnel(hoje, veiculo),
        i.Encerrado ? null : i.DiasRestantes(hoje, veiculo),
        i.Encerrado ? null : i.KmRestantes(veiculo),
        i.Encerrado ? null : i.DataEfetiva(hoje, veiculo),
        i.TextoPrazo(hoje, veiculo),
        i.ProgressoPassos());

    public static ItemDetalheResponse Detalhe(Item i, DateOnly hoje, Veiculo? veiculo, IEnumerable<Historico> historico) => new(
        i.Id,
        i.Nome,
        i.CategoriaId,
        i.ModeloItemId,
        i.TipoVencimento,
        i.Situacao,
        i.CalcularStatus(hoje, veiculo),
        i.CalcularAnel(hoje, veiculo),
        i.TextoPrazo(hoje, veiculo),
        i.Encerrado ? null : i.DiasRestantes(hoje, veiculo),
        i.Encerrado ? null : i.KmRestantes(veiculo),
        i.Encerrado ? null : i.DataEfetiva(hoje, veiculo),
        i.ProximoVencimentoData,
        i.ProximoVencimentoKm,
        i.DataBase,
        i.DataVencimento,
        i.IntervaloDias,
        i.IntervaloKm,
        i.KmBase,
        i.GarantiaFabricanteMeses,
        i.AntecedenciaDias,
        i.Observacao,
        veiculo is null ? null : new VeiculoResumo(veiculo.Id, veiculo.Apelido),
        i.Passos.Select(p => new PassoResponse(p.Id, p.Ordem, p.Descricao, p.Concluido)).ToList(),
        i.ProgressoPassos(),
        historico.Select(h => new HistoricoResumo(h.Id, h.ConcluidoEm, h.KmNaConclusao, h.ValorGasto)).ToList());
}
