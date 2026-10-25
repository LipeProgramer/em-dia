using EmDia.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EmDia.Infrastructure.Services;

/// <summary>
/// Envio real de e-mail via MailKit/SMTP (spec §4.6).
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(
            _config["Email:Remetente"] ?? "noreply@emdia.app"));
        msg.To.Add(MailboxAddress.Parse(destinatario));
        msg.Subject = assunto;
        msg.Body = new TextPart("html") { Text = corpo };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Email:Smtp:Host"] ?? "localhost",
            int.Parse(_config["Email:Smtp:Porta"] ?? "587"),
            SecureSocketOptions.StartTls, ct);

        var usuario = _config["Email:Smtp:Usuario"];
        var senha = _config["Email:Smtp:Senha"];
        if (!string.IsNullOrEmpty(usuario))
            await client.AuthenticateAsync(usuario, senha, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}
