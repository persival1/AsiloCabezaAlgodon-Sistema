using MediatR;
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
