using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Application.Services;
using EmDia.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmDia.Api.Extensions;

/// <summary>
/// Exemplos válidos nos corpos JSON do Swagger: "Try it out" → "Execute" funciona sem editar nada.
/// (O exemplo automático vem com "string" e 0, que a validação recusa.)
/// </summary>
public class ExemplosSwagger : ISchemaFilter
{
    private static readonly Dictionary<Type, string> Exemplos = new()
    {
        [typeof(RegistrarRequest)] = """{ "nome": "Demo Banca", "email": "demo.banca@emdia.dev", "senha": "Demo2026a" }""",
        [typeof(LoginRequest)] = """{ "email": "demo.banca@emdia.dev", "senha": "Demo2026a" }""",
        [typeof(SalvarItemRequest)] = """
            { "nome": "IPVA", "categoriaId": 1, "tipoVencimento": "DataUnica", "dataVencimento": "2026-12-20",
              "antecedenciaDias": 30, "passos": ["Emitir o boleto", "Pagar"] }
            """,
        // dataConclusao é obrigatória e não pode ser futura: 15/10/2026 vale nos testes e em produção
        [typeof(ConcluirRequest)] = """{ "dataConclusao": "2026-10-15", "confirmarPassosPendentes": true, "observacao": "Pago pelo app do banco" }""",
        [typeof(MarcarPassoRequest)] = """{ "concluido": true }""",
        [typeof(CheckupRequest)] = """
            { "respostas": [ { "codigo": "TEM_CNH", "resposta": true }, { "codigo": "TEM_CARRO", "resposta": false } ],
              "veiculos": [],
              "itens": [ { "modeloId": "a7000000-0000-0000-0000-000000000003", "dataVencimento": "2027-03-10" } ] }
            """,
        [typeof(CriarVeiculoRequest)] = """{ "apelido": "Gol", "tipo": "Carro", "kmAtual": 50000, "mediaKmMes": 1500 }""",
        [typeof(EditarVeiculoRequest)] = """{ "apelido": "Gol prata", "tipo": "Carro", "mediaKmMes": 1800 }""",
        [typeof(AtualizarKmRequest)] = """{ "kmAtual": 52000 }""",
        [typeof(PreferenciasDto)] = """{ "receberEmail": true, "maxReavisos": 2, "intervaloReavisoDias": 3, "modoFocoPadrao": false }""",
        [typeof(ExcluirContaRequest)] = """{ "senha": "Demo2026a" }""",
        [typeof(EsqueciSenhaRequest)] = """{ "email": "demo.banca@emdia.dev" }""",
        // O código vem do link do e-mail; o exemplo só mostra o formato
        [typeof(RedefinirSenhaRequest)] = """{ "token": "COLE-AQUI-O-CODIGO-DO-LINK", "novaSenha": "NovaSenha2026" }""",
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (Exemplos.TryGetValue(context.Type, out var json))
            schema.Example = OpenApiAnyFactory.CreateFromJson(json);
    }
}

/// <summary>
/// Listas de opções onde o valor é texto (texto livre deixava digitar valores inválidos): o filtro <c>status</c> de
/// GET /itens e as perguntas respondidas "sim" no POST /checkup/rapido.
/// </summary>
public class OpcoesDeFiltroSwagger : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var p in operation.Parameters?.Where(p => p.In == ParameterLocation.Query) ?? [])
        {
            if (p.Name == "status")
            {
                p.Schema.Enum = Opcoes(Enum.GetNames<StatusItem>());
                p.Description = "Vencido, Atencao (vence dentro da antecedência), EmDia ou Encerrado";
            }
            else if (p.Name == "sim" && p.Schema.Items is not null)
            {
                p.Schema.Items.Enum = Opcoes(CatalogoService.CodigosPerguntas);
            }
        }
    }

    private static List<IOpenApiAny> Opcoes(IEnumerable<string> valores) =>
        valores.Select(v => (IOpenApiAny)new OpenApiString(v)).ToList();
}

/// <summary>
/// Respostas que valem para várias rotas, documentadas em todas: 401 nas que exigem login e 429 do limite geral
/// (DV21). Também tira o cadeado das rotas abertas (cadastro, login e renovação).
/// </summary>
public class RespostasComunsSwagger : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var aberta = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        if (aberta)
            operation.Security = [];
        else
            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Sem login ou token vencido: faça POST /api/v1/auth/login e cole o accessToken em Authorize."
            });

        operation.Responses.TryAdd("429", new OpenApiResponse
        {
            Description = "MUITAS_REQUISICOES: mais de 300 requisições por minuto deste IP. Aguarde 1 minuto."
        });
    }
}
