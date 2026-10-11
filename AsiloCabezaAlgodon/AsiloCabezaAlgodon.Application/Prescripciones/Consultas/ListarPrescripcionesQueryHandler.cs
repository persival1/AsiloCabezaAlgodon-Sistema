using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Prescripciones.Consultas
{
    public class ListarPrescripcionesQueryHandler : IRequestHandler<ListarPrescripcionesQuery, List<PrescripcionListaDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ListarPrescripcionesQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<List<PrescripcionListaDto>> Handle(ListarPrescripcionesQuery request, CancellationToken cancellationToken)
        {
            var prescripciones = await _visitaMedicaRepository.ObtenerPrescripcionesAsync();

            return prescripciones.Select(p => new PrescripcionListaDto(
                p.IdPrescripcion,
                p.NombreMedicamento,
                p.Dosis,
                p.Indicaciones,
                p.Estado,
                p.Costo,
                $"{p.VisitaMedica.Solicitud.Interno.Nombres} {p.VisitaMedica.Solicitud.Interno.Apellidos}",
                p.VisitaMedica.FechaVisita
            )).ToList();
        }
    }
}
