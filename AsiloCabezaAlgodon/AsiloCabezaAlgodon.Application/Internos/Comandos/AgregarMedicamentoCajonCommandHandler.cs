using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public class AgregarMedicamentoCajonCommandHandler : IRequestHandler<AgregarMedicamentoCajonCommand, int>
    {
        private readonly IInternoRepository _internoRepository;

        public AgregarMedicamentoCajonCommandHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<int> Handle(AgregarMedicamentoCajonCommand request, CancellationToken cancellationToken)
        {
            // Revisión del 30/10/2026: antes, con un interno inexistente la API respondía 500,
            // y se aceptaban medicinas de cajón sin nombre.
            Validar.Texto(request.Nombre, "medicamento");
            if (await _internoRepository.ObtenerPorIdAsync(request.IdInterno) == null)
                throw new DatosInvalidosException("El interno indicado no existe.");

            var creado = await _internoRepository.AgregarMedicamentoCajonAsync(new MedicamentoCajon
            {
                IdInterno = request.IdInterno,
                Nombre = request.Nombre,
                Dosis = request.Dosis,
                Frecuencia = request.Frecuencia
            });

            return creado.IdMedicamentoCajon;
        }
    }
}
