using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Caja.PagosFundacion
{
    public record RegistrarPagoFundacionCommand(decimal Monto, string NumeroComprobante, string? Descripcion) : IRequest<int>;

    // AC-07: registra un pago del asilo a la fundación. Lo adeudado es el total de cargos
    // médicos (consultas, exámenes y medicamentos) menos lo que ya se le pagó.
    public class RegistrarPagoFundacionCommandHandler : IRequestHandler<RegistrarPagoFundacionCommand, int>
    {
        private readonly ICajaRepository _cajaRepository;

        public RegistrarPagoFundacionCommandHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<int> Handle(RegistrarPagoFundacionCommand request, CancellationToken cancellationToken)
        {
            Validar.Monto(request.Monto, "El monto");
            Validar.Texto(request.NumeroComprobante, "número de comprobante");

            var pagado = (await _cajaRepository.ObtenerPagosFundacionAsync()).Sum(p => p.Monto);
            var adeudado = await _cajaRepository.TotalCargosMedicosAsync() - pagado;
            if (request.Monto > adeudado)
                throw new DatosInvalidosException($"El monto supera lo que se le debe a la fundación (Q{adeudado:0.00}).");

            var pago = new PagoFundacion
            {
                Fecha = DateTime.Now,
                Monto = request.Monto,
                NumeroComprobante = request.NumeroComprobante.Trim(),
                Descripcion = request.Descripcion?.Trim() ?? string.Empty
            };
            var creado = await _cajaRepository.CrearPagoFundacionAsync(pago);
            return creado.IdPagoFundacion;
        }
    }
}
