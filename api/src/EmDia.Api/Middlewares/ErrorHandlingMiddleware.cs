using System.Text.Json;
using EmDia.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EmDia.Api.Middlewares;

/// <summary>
/// Intercepta exceções e devolve ProblemDetails (RFC 9457) — spec §4.4.
/// Nunca expõe stack trace em produção.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await TratarAsync(ctx, ex);
        }
    }

    private async Task TratarAsync(HttpContext ctx, Exception ex)
    {
        int status;
        string code;
        string mensagem;

        if (ex is NotFoundException nf)
        {
            status = 404; code = nf.Code; mensagem = nf.Message;
        }
        else if (ex is ConflictException cf)
        {
            status = 409; code = cf.Code; mensagem = cf.Message;
        }
        else if (ex is UnauthorizedException ue)
        {
            status = 401; code = ue.Code; mensagem = ue.Message;
        }
        else if (ex is DomainException de)
        {
            status = 422; code = de.Code; mensagem = de.Message;
        }
        else
        {
            status = 500;
            code = "ERRO_INTERNO";
            mensagem = _env.IsDevelopment() ? $"{ex.GetType().Name}: {ex.Message}" : "Ocorreu um erro inesperado.";
            _logger.LogError(ex, "Erro não tratado: {Path}", ctx.Request.Path);
        }

        var problema = new ProblemDetails
        {
            Status   = status,
            Title    = ObterTitulo(status),
            Detail   = mensagem,
            Instance = ctx.Request.Path
        };
        problema.Extensions["code"] = code;

        ctx.Response.StatusCode  = status;
        ctx.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problema, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await ctx.Response.WriteAsync(json);
    }

    private static string ObterTitulo(int status) => status switch
    {
        400 => "Requisição inválida",
        401 => "Não autorizado",
        404 => "Não encontrado",
        409 => "Conflito",
        422 => "Entidade não processável",
        429 => "Muitas requisições",
        _   => "Erro interno do servidor"
    };
}
