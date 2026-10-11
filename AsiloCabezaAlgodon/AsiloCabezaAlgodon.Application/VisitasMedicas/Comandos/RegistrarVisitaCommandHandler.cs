using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos
{
    public class RegistrarVisitaCommandHandler : IRequestHandler<RegistrarVisitaCommand, int>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly ISolicitudRepository _solicitudRepository;

        public RegistrarVisitaCommandHandler(IVisitaMedicaRepository visitaMedicaRepository, ISolicitudRepository solicitudRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
            _solicitudRepository = solicitudRepository;
        }

        public async Task<int> Handle(RegistrarVisitaCommand request, CancellationToken cancellationToken)
        {
            var visita = new VisitaMedica
            {
                IdSolicitud = request.IdSolicitud,
                Diagnostico = request.Diagnostico,
                Observaciones = request.Observaciones,
                FechaVisita = DateTime.Now,
                CostoConsulta = request.CostoConsulta,
                ExamenesSolicitados = request.Examenes
                    .Select(e => new ExamenSolicitado { NombreExamen = e.NombreExamen, Estado = "Pendiente" })
                    .ToList(),
                Prescripciones = request.Prescripciones
                    .Select(p => new Prescripcion { NombreMedicamento = p.NombreMedicamento, Dosis = p.Dosis, Indicaciones = p.Indicaciones, Estado = "Pendiente" })
                    .ToList()
            };

            var creada = await _visitaMedicaRepository.CrearAsync(visita);

            await _visitaMedicaRepository.CargarCostoAFamiliarAsync(creada.IdVisitaMedica, request.CostoConsulta);

            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);
            if (solicitud != null)
            {
                solicitud.Estado = "Atendida";
                await _solicitudRepository.ActualizarAsync(solicitud);
            }

            return creada.IdVisitaMedica;
        }
    }
}
