using MediatR;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public record AgregarPsicopatologiaCommand(int IdInterno, string Nombre, DateTime FechaDiagnostico) : IRequest<int>;
}
