using EmDia.Application.Interfaces;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// Implementação fake do IEmailService para testes — apenas loga o e-mail.
/// </summary>
public class EmailServiceFake : IEmailService
{
    public List<(string Destinatario, string Assunto, string Corpo)> EmailsEnviados { get; } = [];

    public Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken ct = default)
    {
        EmailsEnviados.Add((destinatario, assunto, corpo));
        return Task.CompletedTask;
    }
}
