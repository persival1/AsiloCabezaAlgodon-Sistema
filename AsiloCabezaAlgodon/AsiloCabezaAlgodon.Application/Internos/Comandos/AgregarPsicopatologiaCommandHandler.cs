using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public class AgregarPsicopatologiaCommandHandler : IRequestHandler<AgregarPsicopatologiaCommand, int>
    {
        private readonly IInternoRepository _internoRepository;

        public AgregarPsicopatologiaCommandHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<int> Handle(AgregarPsicopatologiaCommand request, CancellationToken cancellationToken)
        {
            // Revisión del 30/10/2026: antes, con un interno inexistente la API respondía 500,
            // y se aceptaban psicopatologías sin nombre o diagnosticadas en el futuro.
            Validar.Texto(request.Nombre, "psicopatología");
            Validar.FechaNoFutura(request.FechaDiagnostico, "La fecha de diagnóstico");
            if (await _internoRepository.ObtenerPorIdAsync(request.IdInterno) == null)
                throw new DatosInvalidosException("El interno indicado no existe.");

            var creada = await _internoRepository.AgregarPsicopatologiaAsync(new Psicopatologia
            {
                IdInterno = request.IdInterno,
                Nombre = request.Nombre,
                FechaDiagnostico = request.FechaDiagnostico
            });

            return creada.IdPsicopatologia;
        }
    }
}
