using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Examenes.Comandos
{
    public class RegistrarResultadoCommandHandler : IRequestHandler<RegistrarResultadoCommand, bool>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;

        public RegistrarResultadoCommandHandler(IVisitaMedicaRepository visitaMedicaRepository, IUnidadDeTrabajo unidadDeTrabajo)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
            _unidadDeTrabajo = unidadDeTrabajo;
        }

        public async Task<bool> Handle(RegistrarResultadoCommand request, CancellationToken cancellationToken)
        {
            var examen = await _visitaMedicaRepository.ObtenerExamenPorIdAsync(request.IdExamenSolicitado);

            if (examen == null)
                return false;

            // CORRECCIÓN (plan de pruebas SQA, caso LAB-06): antes registrar dos veces el
            // resultado cargaba dos veces el costo al familiar. Ahora solo se registra si el
            // examen sigue Pendiente; si no, se rechaza con 409.
            if (examen.Estado != "Pendiente")
                throw new ConflictoException("Este examen ya tiene un resultado registrado.");

            // Caso LAB-03: resultado vacío o costo inválido
            Validar.Texto(request.Resultado, "resultado");
            Validar.Monto(request.Costo, "El costo", permiteCero: true);

            examen.Resultado = request.Resultado;
            examen.FechaResultado = DateTime.Now;
            examen.Estado = "ResultadoRegistrado";
            examen.Costo = request.Costo;

            // Semana 4 (ROB-03): resultado y cargo al familiar, juntos o ninguno.
            await _unidadDeTrabajo.EjecutarEnTransaccionAsync(async () =>
            {
                await _visitaMedicaRepository.ActualizarExamenAsync(examen);
                await _visitaMedicaRepository.CargarCostoAFamiliarAsync(examen.IdVisitaMedica, request.Costo);
            });
            return true;
        }
    }
}
