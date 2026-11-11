using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    public class ListarInternosQueryHandler : IRequestHandler<ListarInternosQuery, List<InternoDto>>
    {
        private readonly IInternoRepository _internoRepository;
        private readonly ISolicitudRepository _solicitudRepository;

        public ListarInternosQueryHandler(IInternoRepository internoRepository, ISolicitudRepository solicitudRepository)
        {
            _internoRepository = internoRepository;
            _solicitudRepository = solicitudRepository;
        }

        public async Task<List<InternoDto>> Handle(ListarInternosQuery request, CancellationToken cancellationToken)
        {
            var internos = await _internoRepository.ObtenerTodosAsync();
            if (request.IdEspecialista != null)
            {
                var propios = await _solicitudRepository.InternosDeEspecialistaAsync(request.IdEspecialista.Value);
                internos = internos.Where(i => propios.Contains(i.IdInterno)).ToList();
            }

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
