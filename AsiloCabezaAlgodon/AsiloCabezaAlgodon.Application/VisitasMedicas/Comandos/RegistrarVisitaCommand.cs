using MediatR;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos
{
    public record ExamenInput(string NombreExamen);

    public record PrescripcionInput(string NombreMedicamento, string Dosis, string Indicaciones);

    // IdEspecialista NO lo manda el navegador: el controlador lo llena con el usuario del token
    // (semana 3, caso AUT-08), así nadie puede hacerse pasar por otro especialista.
    public record RegistrarVisitaCommand(
        int IdSolicitud,
        string Diagnostico,
        string Observaciones,
        decimal CostoConsulta,
        List<ExamenInput> Examenes,
        List<PrescripcionInput> Prescripciones,
        int IdEspecialista = 0
    ) : IRequest<int>;
}
