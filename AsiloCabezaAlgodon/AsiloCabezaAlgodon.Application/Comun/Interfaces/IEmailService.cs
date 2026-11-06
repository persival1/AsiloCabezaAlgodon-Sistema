namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IEmailService
    {
        // Devuelve true solo si el servidor de correo aceptó el mensaje (semana 3, COR-04).
        Task<bool> EnviarAsync(string destinatario, string asunto, string cuerpo);
    }
}
