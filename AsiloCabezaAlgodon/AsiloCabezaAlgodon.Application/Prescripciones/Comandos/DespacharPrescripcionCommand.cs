using MediatR;

namespace AsiloCabezaAlgodon.Application.Prescripciones.Comandos
{
    public record DespacharPrescripcionCommand(int IdPrescripcion, decimal Costo) : IRequest<bool>;
}
