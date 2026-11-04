using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos
{
    public class RegistrarVisitaCommandHandler : IRequestHandler<RegistrarVisitaCommand, int>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;

        public RegistrarVisitaCommandHandler(IVisitaMedicaRepository visitaMedicaRepository, ISolicitudRepository solicitudRepository, IUnidadDeTrabajo unidadDeTrabajo)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
            _solicitudRepository = solicitudRepository;
            _unidadDeTrabajo = unidadDeTrabajo;
        }

        public async Task<int> Handle(RegistrarVisitaCommand request, CancellationToken cancellationToken)
        {
            // Validaciones en el servidor (plan de pruebas SQA, caso FAR-03 y costo de consulta).
            // La consulta acepta Q0.00 (sin costo), que no se carga al familiar.
            Validar.Texto(request.Diagnostico, "diagnóstico");
            Validar.Monto(request.CostoConsulta, "El costo de la consulta", permiteCero: true);
            foreach (var examen in request.Examenes)
                Validar.Texto(examen.NombreExamen, "nombre del examen");
            foreach (var prescripcion in request.Prescripciones)
                Validar.Texto(prescripcion.NombreMedicamento, "nombre del medicamento");

            // CORRECCIÓN (plan de pruebas SQA, semana 3): antes se podía registrar una visita sobre
            // cualquier solicitud, aunque fuera de otro especialista (AUT-08), estuviera Pendiente o
            // ya Atendida (EXP-07). Una segunda visita cobraba la consulta otra vez al familiar.
            // Ahora se revisa todo ANTES de guardar o cobrar algo.
            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);
            if (solicitud == null)
                throw new DatosInvalidosException("La solicitud indicada no existe.");

            if (solicitud.Estado == "Atendida")
                throw new ConflictoException("Esta solicitud ya fue atendida.");
            if (solicitud.Estado != "Asignada")
                throw new ConflictoException("La solicitud todavía no tiene especialista asignado.");

            if (solicitud.IdEspecialista != request.IdEspecialista)
                throw new AccesoDenegadoException("Solo el especialista asignado puede registrar la visita de esta solicitud.");

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

            // Semana 4 (ROB-03): los tres guardados van juntos. Si uno falla, no queda la visita
            // sin cobrar ni la consulta cobrada con la solicitud todavía Asignada.
            await _unidadDeTrabajo.EjecutarEnTransaccionAsync(async () =>
            {
                await _visitaMedicaRepository.CrearAsync(visita);
                await _visitaMedicaRepository.CargarCostoAFamiliarAsync(visita.IdVisitaMedica, request.CostoConsulta);

                solicitud.Estado = "Atendida";
                await _solicitudRepository.ActualizarAsync(solicitud);
            });

            return visita.IdVisitaMedica;
        }
    }
}
