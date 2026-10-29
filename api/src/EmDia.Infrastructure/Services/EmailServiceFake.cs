using EmDia.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// E-mail sem SMTP configurado (DV16): guarda as mensagens (os testes conferem) e escreve no log do servidor.
/// </summary>
public class EmailServiceFake(ILogger<EmailServiceFake>? log = null) : IEmailService
{
    public List<(string Destinatario, string Assunto, string Corpo)> EmailsEnviados { get; } = [];

    public Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken ct = default)
    {
        lock (EmailsEnviados)
            EmailsEnviados.Add((destinatario, assunto, corpo));
        log?.LogInformation("E-mail não enviado (SMTP não configurado) para {Destinatario}: {Assunto}\n{Corpo}",
            destinatario, assunto, corpo);
        return Task.CompletedTask;
    }
}
