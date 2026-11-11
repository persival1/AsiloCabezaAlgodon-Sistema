using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Caja.Cobros
{
    public class CrearCobroCommandHandler : IRequestHandler<CrearCobroCommand, int>
    {
        private readonly ICajaRepository _cajaRepository;

        public CrearCobroCommandHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<int> Handle(CrearCobroCommand request, CancellationToken cancellationToken)
        {
            // Validaciones en el servidor (plan de pruebas SQA, casos FIN-05 y FIN-06).
            // Antes un monto negativo AUMENTABA la deuda del familiar en lugar de bajarla.
            Validar.Texto(request.Concepto, "concepto");
            Validar.Monto(request.Monto, "El monto");

            if (!await _cajaRepository.ExisteFamiliarAsync(request.IdFamiliar))
                throw new DatosInvalidosException("El familiar indicado no existe.");

            // Un cargo médico solo se puede ligar a una visita del interno de ESTE familiar;
            // si no, el pago marcaría como pagada la cita de otra persona.
            if (request.IdVisitaMedica != null &&
                !await _cajaRepository.VisitaEsDelFamiliarAsync(request.IdVisitaMedica.Value, request.IdFamiliar))
                throw new DatosInvalidosException("La visita médica no corresponde al interno de ese familiar.");

            // AC-01: no se puede pagar más de lo que falta de esa visita; si no, el saldo
            // quedaría «a favor» por un error de digitación.
            if (request.IdVisitaMedica != null)
            {
                var pendiente = await _cajaRepository.PendienteDeVisitaAsync(request.IdVisitaMedica.Value) ?? 0;
                if (request.Monto > pendiente)
                    throw new DatosInvalidosException($"El monto supera lo que falta pagar de esa visita (Q{pendiente:0.00}).");
            }

            var cobro = new Cobro
            {
                IdFamiliar = request.IdFamiliar,
                Concepto = request.Concepto,
                Monto = request.Monto,
                Fecha = DateTime.Now,
                IdVisitaMedica = request.IdVisitaMedica
            };

            var creado = await _cajaRepository.CrearCobroAsync(cobro);
            return creado.IdCobro;
        }
    }
}
