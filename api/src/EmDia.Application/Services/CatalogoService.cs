using EmDia.Application.DTOs.Catalogo;
using EmDia.Application.Interfaces.Repositories;
using EmDia.Domain.Entities;
using EmDia.Domain.Exceptions;

namespace EmDia.Application.Services;

/// <summary>Categorias, modelos prontos e perguntas do check-up (E08–E10).</summary>
public class CatalogoService(ICatalogoRepository catalogo)
{
    /// <summary>Texto e ajuda de cada pergunta do check-up, por código (spec §2.7 e wireframe T03).</summary>
    private static readonly Dictionary<string, (string Texto, string Ajuda)> Perguntas = new()
    {
        ["TEM_CARRO"] = ("Você tem carro ou moto?", "Assim já montamos os prazos do seu veículo."),
        ["TEM_CNH"] = ("Você tem CNH?", "A carteira de motorista tem validade."),
        ["TEM_PASSAPORTE"] = ("Você tem passaporte?", "Ele vence e demora para renovar."),
        ["TEM_PURIFICADOR"] = ("Você tem purificador de água?", "O filtro precisa de troca de tempos em tempos."),
        ["TEM_PET"] = ("Você tem pet?", "Vacinas têm data para renovar."),
        ["CONSULTA_ROTINA"] = ("Você faz consulta médica de rotina?", "Você escolhe o intervalo; não fazemos recomendações de saúde."),
        ["DECLARA_IR"] = ("Você declara Imposto de Renda?", "Todo ano tem prazo para enviar."),
        ["COMPROU_ELETRONICO"] = ("Comprou algum eletrônico no último ano?", "A garantia tem prazo para acabar."),
    };

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

        return modelos
            .Where(m => m.CodigoCheckup is not null)
            .GroupBy(m => m.CodigoCheckup!)
            .Select(g =>
            {
                var (texto, ajuda) = Perguntas.GetValueOrDefault(g.Key, (g.Key, ""));
                return new PerguntaResponse(g.Key, texto, ajuda, g.Min(m => m.CategoriaId),
                    g.OrderBy(m => m.Nome).Select(ParaResposta).ToList());
            })
            .OrderBy(p => p.CategoriaId)
            .ThenBy(p => p.Codigo)
            .ToList();
    }

    internal static ModeloResponse ParaResposta(ModeloItem m) => new(
        m.Id, m.Nome, m.CategoriaId, m.TipoVencimento, m.IntervaloDias, m.IntervaloKm,
        m.GarantiaFabricanteMeses, m.AntecedenciaDias, m.CodigoCheckup,
        m.Passos.OrderBy(p => p.Ordem).Select(p => p.Descricao).ToList());
}
