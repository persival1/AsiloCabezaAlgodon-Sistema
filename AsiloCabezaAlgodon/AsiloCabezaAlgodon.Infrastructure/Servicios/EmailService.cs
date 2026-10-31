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

        // Semana 3 (COR-02, COR-03, COR-04): devuelve true solo si el correo salió. Si falta la
        // configuración o el envío falla, devuelve false (y lo deja en el registro) para que la
        // API no le diga al administrador «correo enviado» cuando no se envió.
        // El registro nunca incluye la contraseña SMTP (COR-05).
        public async Task<bool> EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var host = _config["Smtp:Host"];
            var puerto = _config["Smtp:Puerto"];
            var usuario = _config["Smtp:Usuario"];
            var password = _config["Smtp:Password"];
            var remitente = _config["Smtp:Remitente"] ?? usuario;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario))
            {
                _logger.LogWarning("SMTP no configurado (appsettings: Smtp:Host/Usuario/Password). No se envió el correo a {Destinatario}: {Asunto}", destinatario, asunto);
                return false;
            }

            try
            {
                // Dentro del try: un correo con formato inválido (COR-03) también lanza error aquí.
                using var mensaje = new MailMessage(remitente!, destinatario, asunto, cuerpo);
                using var cliente = new SmtpClient(host, int.Parse(puerto ?? "587"))
                {
                    Credentials = new NetworkCredential(usuario, password),
                    EnableSsl = true
                };

                await cliente.SendMailAsync(mensaje);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario}", destinatario);
                return false;
            }
        }
    }
}
