using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    public class ListarInternosQueryHandler : IRequestHandler<ListarInternosQuery, List<InternoDto>>
    {
        private readonly IInternoRepository _internoRepository;

        public ListarInternosQueryHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<List<InternoDto>> Handle(ListarInternosQuery request, CancellationToken cancellationToken)
        {
            var internos = await _internoRepository.ObtenerTodosAsync();

            return internos.Select(i => new InternoDto(
                i.IdInterno,
                i.Nombres,
                i.Apellidos,
                i.FechaNacimiento,
                i.FechaIngreso,
                i.MotivoIngreso,
                i.Estado,
                i.Familiar?.IdFamiliar,
                i.Familiar != null ? $"{i.Familiar.Nombres} {i.Familiar.Apellidos}" : null,
                i.Familiar?.Correo
            )).ToList();
        }
    }
}
