using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>Categorias, modelos prontos e perguntas do check-up (E08–E10).</summary>
public class CatalogoService(ICatalogoRepository catalogo)
{
    /// <summary>
    /// Perguntas do check-up por área, na ordem em que aparecem (Veículo → Documentos → Casa → Saúde → Finanças →
    /// Garantias). Cada código ativa os modelos com o mesmo <c>codigo_checkup</c> (spec §2.7 e wireframe T03).
    /// </summary>
    private static readonly (string Codigo, string Texto, string Ajuda)[] Perguntas =
    [
        ("TEM_CARRO", "Você tem carro ou moto?", "Assim já montamos os prazos do seu veículo: óleo, IPVA e licenciamento."),
        ("TEM_SEGURO_AUTO", "Seu veículo tem seguro?", "A apólice vence uma vez por ano."),
        ("TEM_CNH", "Você tem CNH?", "A carteira de motorista tem validade."),
        ("TEM_PASSAPORTE", "Você tem passaporte?", "Ele vence e demora para renovar."),
        ("TEM_CERTIFICADO", "Você usa certificado digital (e-CPF ou e-CNPJ)?", "Ele expira e para de funcionar de repente."),
        ("MORA_ALUGUEL", "Você mora de aluguel?", "O contrato tem data para renovar."),
        ("TEM_PURIFICADOR", "Você tem purificador de água?", "O filtro precisa de troca de tempos em tempos."),
        ("TEM_AR_CONDICIONADO", "Você tem ar-condicionado?", "A limpeza deixa o ar melhor e o aparelho dura mais."),
        ("CONSULTA_ROTINA", "Você faz consulta médica de rotina?", "Você escolhe o intervalo; não fazemos recomendações de saúde."),
        ("VAI_DENTISTA", "Você vai ao dentista regularmente?", "Você escolhe o intervalo; é só um lembrete."),
        ("USA_OCULOS", "Você usa óculos ou lentes?", "O exame de vista tem data para refazer."),
        ("TEM_PET", "Você tem pet?", "Vacinas têm data para renovar."),
        ("DECLARA_IR", "Você declara Imposto de Renda?", "Todo ano tem prazo para enviar."),
        ("PAGA_IPTU", "Você paga IPTU?", "O imposto do imóvel vence uma vez por ano."),
        ("TEM_SEGURO_VIDA", "Você tem seguro de vida?", "A renovação costuma ser anual."),
        ("COMPROU_ELETRONICO", "Comprou algum eletrônico no último ano?", "Celular, TV, notebook: a garantia tem prazo para acabar."),
        ("COMPROU_ELETRODOMESTICO", "Comprou algum eletrodoméstico no último ano?", "Geladeira, máquina de lavar, micro-ondas: a garantia tem prazo."),
    ];

    /// <summary>Códigos das perguntas, na ordem do check-up (lista de opções no Swagger).</summary>
    public static IEnumerable<string> CodigosPerguntas => Perguntas.Select(p => p.Codigo);

    public async Task<IReadOnlyList<CategoriaResponse>> ListarCategoriasAsync(CancellationToken ct) =>
        (await catalogo.ListarCategoriasAsync(ct))
            .Select(c => new CategoriaResponse(c.Id, c.Nome, c.Icone, c.Ordem))
            .ToList();

    /// <summary>E09: só modelos ativos; categoriaId entre 1 e 6, se informado.</summary>
    public async Task<IReadOnlyList<ModeloResponse>> ListarModelosAsync(short? categoriaId, CancellationToken ct)
    {
        if (categoriaId is < 1 or > 6)
            throw new CampoInvalidoException("categoriaId", "Escolha uma categoria entre 1 e 6.");

        return (await catalogo.ListarModelosAtivosAsync(categoriaId, ct)).Select(ParaResposta).ToList();
    }

    /// <summary>E10: perguntas derivadas dos códigos dos modelos ativos.</summary>
    public async Task<IReadOnlyList<PerguntaResponse>> ListarPerguntasAsync(CancellationToken ct)
    {
        var modelos = await catalogo.ListarModelosAtivosAsync(null, ct);

        var ordem = Perguntas.Select((p, i) => (p.Codigo, i)).ToDictionary(x => x.Codigo, x => x.i);

        return modelos
            .Where(m => m.CodigoCheckup is not null)
            .GroupBy(m => m.CodigoCheckup!)
            .Select(g =>
            {
                var pergunta = Perguntas.FirstOrDefault(p => p.Codigo == g.Key);
                return new PerguntaResponse(g.Key, pergunta.Texto ?? g.Key, pergunta.Ajuda ?? "", g.Min(m => m.CategoriaId),
                    g.OrderBy(m => m.Nome).Select(ParaResposta).ToList());
            })
            .OrderBy(p => p.CategoriaId)
            .ThenBy(p => ordem.GetValueOrDefault(p.Codigo, int.MaxValue))
            .ToList();
    }

    internal static ModeloResponse ParaResposta(ModeloItem m) => new(
        m.Id, m.Nome, m.CategoriaId, m.TipoVencimento, m.IntervaloDias, m.IntervaloKm,
        m.GarantiaFabricanteMeses, m.AntecedenciaDias, m.CodigoCheckup,
        m.Passos.OrderBy(p => p.Ordem).Select(p => p.Descricao).ToList());
}
