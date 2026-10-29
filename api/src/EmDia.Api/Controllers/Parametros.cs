using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EmDia.Api.Controllers;

// Extra (DV22) · Campos das rotas ".../rapido" como parâmetros (sem JSON), para testar pelo Swagger sem erro de
// digitação. Cada classe vira o mesmo request do contrato E01–E25, então as validações e regras são as mesmas.
// Os campos com [BindRequired] aparecem com * no Swagger; se faltarem, a API responde 400 "Campo obrigatório.".

/// <summary>Campos do item (E15 · criar e E16 · editar).</summary>
public class ItemParametros
{
    /// <summary>Nome do item. Ex.: IPVA</summary>
    [FromQuery(Name = "nome"), BindRequired] public string? Nome { get; set; }

    /// <summary>Área: 1 Veículo · 2 Documentos · 3 Casa · 4 Saúde · 5 Finanças · 6 Garantias</summary>
    [FromQuery(Name = "categoriaId"), BindRequired] public short CategoriaId { get; set; }

    /// <summary>Como o prazo é calculado. DataUnica → dataVencimento; RecorrentePorTempo → dataBase e intervaloDias;
    /// UsoOuTempo → veiculoId, dataBase, kmBase, intervaloKm e intervaloDias; Calculado → dataBase e garantiaFabricanteMeses</summary>
    [FromQuery(Name = "tipoVencimento"), BindRequired] public TipoVencimento TipoVencimento { get; set; }

    /// <summary>DataUnica: data em que vence (aaaa-mm-dd)</summary>
    [FromQuery(Name = "dataVencimento")] public DateOnly? DataVencimento { get; set; }

    /// <summary>Data da última vez (ou da compra, em Calculado); não pode ser futura (aaaa-mm-dd)</summary>
    [FromQuery(Name = "dataBase")] public DateOnly? DataBase { get; set; }

    /// <summary>Repetir a cada quantos dias</summary>
    [FromQuery(Name = "intervaloDias")] public int? IntervaloDias { get; set; }

    /// <summary>UsoOuTempo: repetir a cada quantos km</summary>
    [FromQuery(Name = "intervaloKm")] public int? IntervaloKm { get; set; }

    /// <summary>UsoOuTempo: km do veículo na última vez</summary>
    [FromQuery(Name = "kmBase")] public int? KmBase { get; set; }

    /// <summary>Calculado: meses de garantia do fabricante</summary>
    [FromQuery(Name = "garantiaFabricanteMeses")] public short? GarantiaFabricanteMeses { get; set; }

    /// <summary>UsoOuTempo: id do veículo (GET /api/v1/veiculos)</summary>
    [FromQuery(Name = "veiculoId")] public Guid? VeiculoId { get; set; }

    /// <summary>Avisar quantos dias antes (0 a 365; padrão 30)</summary>
    [FromQuery(Name = "antecedenciaDias")] public short? AntecedenciaDias { get; set; }

    /// <summary>Observação livre (até 500 caracteres)</summary>
    [FromQuery(Name = "observacao")] public string? Observacao { get; set; }

    /// <summary>Passos pequenos, um por linha (até 10)</summary>
    [FromQuery(Name = "passos")] public string[]? Passos { get; set; }

    public SalvarItemRequest ParaRequest() => new(Nome ?? "", CategoriaId, null, TipoVencimento, DataBase, DataVencimento,
        IntervaloDias, IntervaloKm, KmBase, GarantiaFabricanteMeses, VeiculoId, AntecedenciaDias ?? 30, Observacao,
        Passos?.Where(p => !string.IsNullOrWhiteSpace(p)).ToList());
}

/// <summary>Campos da conclusão (E18).</summary>
public class ConcluirParametros
{
    /// <summary>Quando foi feito (aaaa-mm-dd); não pode ser futura</summary>
    [FromQuery(Name = "dataConclusao"), BindRequired] public DateOnly DataConclusao { get; set; }

    /// <summary>true conclui mesmo com passos pendentes; false responde 422 PASSOS_PENDENTES se houver algum</summary>
    [FromQuery(Name = "confirmarPassosPendentes")] public bool ConfirmarPassosPendentes { get; set; }

    /// <summary>UsoOuTempo: km do veículo na conclusão</summary>
    [FromQuery(Name = "km")] public int? Km { get; set; }

    /// <summary>Quanto foi gasto (ex.: 250.90)</summary>
    [FromQuery(Name = "valorGasto")] public decimal? ValorGasto { get; set; }

    /// <summary>Observação livre (até 500 caracteres)</summary>
    [FromQuery(Name = "observacao")] public string? Observacao { get; set; }

    /// <summary>DataUnica: nova validade do documento renovado (aaaa-mm-dd)</summary>
    [FromQuery(Name = "novaDataVencimento")] public DateOnly? NovaDataVencimento { get; set; }

    public ConcluirRequest ParaRequest() =>
        new(DataConclusao, Km, ValorGasto, Observacao, ConfirmarPassosPendentes, NovaDataVencimento);
}

/// <summary>Campos do veículo (E21 · cadastrar e E22 · editar).</summary>
public class VeiculoParametros
{
    /// <summary>Nome curto do veículo (1 a 50 caracteres). Ex.: Gol</summary>
    [FromQuery(Name = "apelido"), BindRequired] public string? Apelido { get; set; }

    /// <summary>Carro ou Moto</summary>
    [FromQuery(Name = "tipo"), BindRequired] public TipoVeiculo Tipo { get; set; }

    /// <summary>Quantos km roda por mês, em média (1 a 20.000)</summary>
    [FromQuery(Name = "mediaKmMes"), BindRequired] public int MediaKmMes { get; set; }
}

/// <summary>Campos do check-up (E11), com no máximo um veículo e um item.</summary>
public class CheckupParametros
{
    /// <summary>Perguntas respondidas "sim" (escolha uma ou mais; as outras contam como "não")</summary>
    [FromQuery(Name = "sim"), BindRequired] public string[] Sim { get; set; } = [];

    /// <summary>Modelo do item a criar (opcional). Ex.: CNH a7000000-0000-0000-0000-000000000003 (data única) ·
    /// Troca de óleo …001 (por uso) · IPVA …002 (recorrente) · Passaporte …008 (data única); lista completa em
    /// GET /api/v1/modelos. Precisa de uma resposta "sim" ligada a ele</summary>
    [FromQuery(Name = "modeloId")] public Guid? ModeloId { get; set; }

    /// <summary>Item de data única: data em que vence (aaaa-mm-dd)</summary>
    [FromQuery(Name = "dataVencimento")] public DateOnly? DataVencimento { get; set; }

    /// <summary>Item recorrente ou por uso: data da última vez (aaaa-mm-dd); não pode ser futura</summary>
    [FromQuery(Name = "dataBase")] public DateOnly? DataBase { get; set; }

    /// <summary>Item por uso (ex.: troca de óleo): km na última vez</summary>
    [FromQuery(Name = "kmBase")] public int? KmBase { get; set; }

    /// <summary>Veículo novo (opcional): apelido. Itens da área Veículo ficam ligados a ele</summary>
    [FromQuery(Name = "veiculoApelido")] public string? VeiculoApelido { get; set; }

    /// <summary>Veículo novo: Carro ou Moto</summary>
    [FromQuery(Name = "veiculoTipo")] public TipoVeiculo? VeiculoTipo { get; set; }

    /// <summary>Veículo novo: km atual</summary>
    [FromQuery(Name = "veiculoKmAtual")] public int? VeiculoKmAtual { get; set; }

    /// <summary>Veículo novo: km por mês, em média (padrão 1.000)</summary>
    [FromQuery(Name = "veiculoMediaKmMes")] public int? VeiculoMediaKmMes { get; set; }

    private const string RefVeiculo = "veiculo";

    /// <param name="itemDaAreaVeiculo">Se o modelo escolhido é da área Veículo (só então o item fica ligado ao veículo).</param>
    public CheckupRequest ParaRequest(bool itemDaAreaVeiculo)
    {
        var temVeiculo = !string.IsNullOrWhiteSpace(VeiculoApelido);
        var veiculos = temVeiculo
            ? [new VeiculoCheckup(RefVeiculo, VeiculoApelido!, VeiculoTipo ?? TipoVeiculo.Carro, VeiculoKmAtual ?? 0, VeiculoMediaKmMes ?? 1000)]
            : new List<VeiculoCheckup>();
        var itens = ModeloId is { } modelo
            ? [new ItemCheckup(modelo, temVeiculo && itemDaAreaVeiculo ? RefVeiculo : null, null, DataBase, KmBase, DataVencimento)]
            : new List<ItemCheckup>();

        return new CheckupRequest(Sim.Distinct().Select(c => new RespostaCheckup(c, true)).ToList(), veiculos, itens);
    }
}
