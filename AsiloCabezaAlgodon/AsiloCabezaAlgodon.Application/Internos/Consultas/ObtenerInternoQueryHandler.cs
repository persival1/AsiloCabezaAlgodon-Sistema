using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    public class ObtenerInternoQueryHandler : IRequestHandler<ObtenerInternoQuery, InternoDetalleDto?>
    {
        private readonly IInternoRepository _internoRepository;

        public ObtenerInternoQueryHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<InternoDetalleDto?> Handle(ObtenerInternoQuery request, CancellationToken cancellationToken)
        {
            var interno = await _internoRepository.ObtenerPorIdAsync(request.IdInterno);

            if (interno == null)
                return null;

            var familiar = interno.Familiar == null ? null : new FamiliarDto(
                interno.Familiar.IdFamiliar,
                interno.Familiar.Nombres,
                interno.Familiar.Apellidos,
                interno.Familiar.Parentesco,
                interno.Familiar.Telefono,
                interno.Familiar.Correo,
                interno.Familiar.CuotaMensual,
                interno.Familiar.SaldoPendiente
            );

            return new InternoDetalleDto(
                interno.IdInterno,
                interno.Nombres,
                interno.Apellidos,
                interno.FechaNacimiento,
                interno.FechaIngreso,
                interno.MotivoIngreso,
                interno.Estado,
                familiar,
                interno.Psicopatologias.Select(p => new PsicopatologiaDto(p.IdPsicopatologia, p.Nombre, p.FechaDiagnostico)).ToList(),
                interno.MedicamentosCajon.Select(m => new MedicamentoCajonDto(m.IdMedicamentoCajon, m.Nombre, m.Dosis, m.Frecuencia)).ToList()
            );
        }
    }
}
