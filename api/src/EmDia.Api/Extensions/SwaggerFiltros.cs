using EmDia.Application.DTOs.Auth;
using EmDia.Application.DTOs.Checkup;
using EmDia.Application.DTOs.Itens;
using EmDia.Application.DTOs.Usuarios;
using EmDia.Application.DTOs.Veiculos;
using EmDia.Domain.Enums;
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
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (Exemplos.TryGetValue(context.Type, out var json))
            schema.Example = OpenApiAnyFactory.CreateFromJson(json);
    }
}

/// <summary>Filtro <c>status</c> de GET /itens como lista de opções (texto livre deixava digitar valores inválidos).</summary>
public class OpcoesDeFiltroSwagger : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var status = operation.Parameters?.FirstOrDefault(p => p.Name == "status" && p.In == ParameterLocation.Query);
        if (status is null)
            return;

        status.Schema.Enum = Enum.GetNames<StatusItem>().Select(n => (IOpenApiAny)new OpenApiString(n)).ToList();
        status.Description = "Vencido, Atencao (vence dentro da antecedência), EmDia ou Encerrado";
    }
}
