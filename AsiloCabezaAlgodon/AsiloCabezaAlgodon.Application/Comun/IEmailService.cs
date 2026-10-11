namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpo);
    }
}
