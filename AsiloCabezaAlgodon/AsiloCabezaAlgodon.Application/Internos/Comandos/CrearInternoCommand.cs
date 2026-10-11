using MediatR;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public record CrearInternoCommand(
        string Nombres,
        string Apellidos,
        DateTime FechaNacimiento,
        DateTime FechaIngreso,
        string MotivoIngreso,
        string NombreFamiliar,
        string ApellidoFamiliar,
        string Parentesco,
        string TelefonoFamiliar,
        string CorreoFamiliar,
        decimal CuotaMensual
    ) : IRequest<int>;
}
