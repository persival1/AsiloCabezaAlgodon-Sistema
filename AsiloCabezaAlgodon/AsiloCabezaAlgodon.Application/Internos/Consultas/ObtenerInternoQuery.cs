using MediatR;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    public record FamiliarDto(
        int IdFamiliar,
        string Nombres,
        string Apellidos,
        string Parentesco,
        string Telefono,
        string Correo,
        decimal CuotaMensual,
        decimal SaldoPendiente
    );

    public record PsicopatologiaDto(int IdPsicopatologia, string Nombre, DateTime FechaDiagnostico);

    public record MedicamentoCajonDto(int IdMedicamentoCajon, string Nombre, string Dosis, string Frecuencia);

    public record InternoDetalleDto(
        int IdInterno,
        string Nombres,
        string Apellidos,
        DateTime FechaNacimiento,
        DateTime FechaIngreso,
        string MotivoIngreso,
        string Estado,
        FamiliarDto? Familiar,
        List<PsicopatologiaDto> Psicopatologias,
        List<MedicamentoCajonDto> MedicamentosCajon
    );

    public record ObtenerInternoQuery(int IdInterno) : IRequest<InternoDetalleDto?>;
}
