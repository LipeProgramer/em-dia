using System.Text.Json;
using EmDia.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Middlewares;

/// <summary>
/// Middleware global de erros (spec §3.5): converte as exceções em ProblemDetails (RFC 9457)
/// e registra o erro em log, sem stack trace na resposta e sem dados sensíveis.
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex) when (!ctx.Response.HasStarted)
        {
            var problema = Converter(ex);
            if (problema.Status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Erro inesperado em {Metodo} {Caminho}", ctx.Request.Method, ctx.Request.Path);

            ctx.Response.Clear();
            ctx.Response.StatusCode = problema.Status!.Value;
            // Serializa pelo tipo real, para incluir "errors" do ValidationProblemDetails
            await ctx.Response.WriteAsJsonAsync(problema, problema.GetType(), Json, "application/problem+json");
        }
    }

    private static ProblemDetails Converter(Exception ex) => ex switch
    {
        ValidationException v => DadosInvalidos(v.Errors.GroupBy(e => CamelCase(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray())),
        CampoInvalidoException c => DadosInvalidos(new() { [c.Campo] = [c.Message] }),
        UnauthorizedException u => Problema(401, "Não autorizado", u.Message, u.Code),
        NotFoundException n => Problema(404, "Não encontrado", n.Message, n.Code),
        ConflictException c => Problema(409, "Conflito", c.Message, c.Code),
        DomainException d => Problema(422, "Regra de negócio", d.Message, d.Code),
        BadHttpRequestException { StatusCode: 413 } => Problema(413, "Requisição muito grande",
            "O conteúdo enviado é maior que o permitido.", "CORPO_GRANDE"),
        _ => Problema(500, "Erro inesperado", "Algo deu errado do nosso lado. Tente de novo em instantes.", "ERRO_INTERNO")
    };

    public static ValidationProblemDetails DadosInvalidos(Dictionary<string, string[]> erros) =>
        new(erros) { Title = "Dados inválidos", Status = StatusCodes.Status400BadRequest };

    private static ProblemDetails Problema(int status, string titulo, string detalhe, string code) => new()
    {
        Status = status,
        Title = titulo,
        Detail = detalhe,
        Extensions = { ["code"] = code }
    };

    /// <summary>"Itens[0].DataBase" → "itens[0].dataBase" (nomes dos campos iguais aos do JSON).</summary>
    public static string CamelCase(string caminho) =>
        string.Join('.', caminho.Split('.').Select(p => p.Length == 0 ? p : char.ToLowerInvariant(p[0]) + p[1..]));
}
