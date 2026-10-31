using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Caja.Gastos
{
    public class CrearGastoCommandHandler : IRequestHandler<CrearGastoCommand, int>
    {
        private readonly ICajaRepository _cajaRepository;

        public CrearGastoCommandHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<int> Handle(CrearGastoCommand request, CancellationToken cancellationToken)
        {
            // Validaciones en el servidor (plan de pruebas SQA, caso FIN-05)
            Validar.Texto(request.Concepto, "concepto");
            Validar.Monto(request.Monto, "El monto");

            var gasto = new Gasto
            {
                Concepto = request.Concepto,
                Monto = request.Monto,
                Fecha = DateTime.Now
            };

            var creado = await _cajaRepository.CrearGastoAsync(gasto);
            return creado.IdGasto;
        }
    }
}
