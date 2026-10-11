using MediatR;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos
{
    public record ExamenInput(string NombreExamen);

    public record PrescripcionInput(string NombreMedicamento, string Dosis, string Indicaciones);

    public record RegistrarVisitaCommand(
        int IdSolicitud,
        string Diagnostico,
        string Observaciones,
        decimal CostoConsulta,
        List<ExamenInput> Examenes,
        List<PrescripcionInput> Prescripciones
    ) : IRequest<int>;
}
