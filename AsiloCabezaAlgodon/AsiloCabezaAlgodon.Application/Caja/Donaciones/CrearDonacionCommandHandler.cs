using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Caja.Donaciones
{
    public class CrearDonacionCommandHandler : IRequestHandler<CrearDonacionCommand, int>
    {
        private readonly ICajaRepository _cajaRepository;

        public CrearDonacionCommandHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<int> Handle(CrearDonacionCommand request, CancellationToken cancellationToken)
        {
            // Validaciones en el servidor (plan de pruebas SQA, caso FIN-05)
            Validar.Texto(request.TipoDonante, "tipo de donante");
            Validar.Texto(request.NombreDonante, "donante");
            Validar.Monto(request.Monto, "El monto");

            var donacion = new Donacion
            {
                TipoDonante = request.TipoDonante,
                NombreDonante = request.NombreDonante,
                Monto = request.Monto,
                Descripcion = request.Descripcion,
                Fecha = DateTime.Now
            };

            var creada = await _cajaRepository.CrearDonacionAsync(donacion);
            return creada.IdDonacion;
        }
    }
}
