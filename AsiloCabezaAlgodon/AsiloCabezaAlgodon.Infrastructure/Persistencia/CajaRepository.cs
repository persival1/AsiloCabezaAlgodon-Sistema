using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class CajaRepository : ICajaRepository
    {
        private readonly BDConexion _context;

        public CajaRepository(BDConexion context)
        {
            _context = context;
        }

        public async Task<List<Donacion>> ObtenerDonacionesAsync()
        {
            return await _context.Donaciones.OrderByDescending(d => d.Fecha).ToListAsync();
        }

        public async Task<Donacion> CrearDonacionAsync(Donacion donacion)
        {
            _context.Donaciones.Add(donacion);
            await _context.SaveChangesAsync();
            return donacion;
        }

        public async Task<List<Gasto>> ObtenerGastosAsync()
        {
            return await _context.Gastos.OrderByDescending(g => g.Fecha).ToListAsync();
        }

        public async Task<Gasto> CrearGastoAsync(Gasto gasto)
        {
            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();
            return gasto;
        }

        public async Task<List<Cobro>> ObtenerCobrosAsync()
        {
            return await _context.Cobros
                .Include(c => c.Familiar).ThenInclude(f => f.Interno)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        public async Task<Cobro> CrearCobroAsync(Cobro cobro)
        {
            _context.Cobros.Add(cobro);

            var familiar = await _context.Familiares.FindAsync(cobro.IdFamiliar);
            if (familiar != null)
                familiar.SaldoPendiente -= cobro.Monto;

            await _context.SaveChangesAsync();
            return cobro;
        }
    }
}
