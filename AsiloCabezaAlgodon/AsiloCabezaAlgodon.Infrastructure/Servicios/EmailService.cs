using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Infrastructure.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var host = _config["Smtp:Host"];
            var puerto = _config["Smtp:Puerto"];
            var usuario = _config["Smtp:Usuario"];
            var password = _config["Smtp:Password"];
            var remitente = _config["Smtp:Remitente"] ?? usuario;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario))
            {
                _logger.LogWarning("SMTP no configurado (appsettings: Smtp:Host/Usuario/Password). No se envió el correo a {Destinatario}: {Asunto}", destinatario, asunto);
                return;
            }

            using var mensaje = new MailMessage(remitente!, destinatario, asunto, cuerpo);
            using var cliente = new SmtpClient(host, int.Parse(puerto ?? "587"))
            {
                Credentials = new NetworkCredential(usuario, password),
                EnableSsl = true
            };

            try
            {
                await cliente.SendMailAsync(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario}", destinatario);
            }
        }
    }
}
