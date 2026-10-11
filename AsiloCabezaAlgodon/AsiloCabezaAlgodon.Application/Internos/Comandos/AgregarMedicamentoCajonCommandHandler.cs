using MediatR;
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
