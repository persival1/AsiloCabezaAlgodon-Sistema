using MediatR;
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
