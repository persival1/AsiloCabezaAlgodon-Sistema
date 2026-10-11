using MediatR;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public record AgregarMedicamentoCajonCommand(int IdInterno, string Nombre, string Dosis, string Frecuencia) : IRequest<int>;
}
