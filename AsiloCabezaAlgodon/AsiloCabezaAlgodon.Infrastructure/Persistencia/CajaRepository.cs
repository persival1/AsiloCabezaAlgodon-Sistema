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

            // AC-01, regla del saldo: el saldo del familiar es lo que debe por cargos médicos.
            // Solo un pago ligado a una visita lo baja. La cuota mensual es un ingreso del
            // asilo por la estadía y no paga cargos médicos, así que no lo cambia.
            if (cobro.IdVisitaMedica != null)
            {
                var familiar = await _context.Familiares.FindAsync(cobro.IdFamiliar);
                if (familiar != null)
                    familiar.SaldoPendiente -= cobro.Monto;
            }

            await _context.SaveChangesAsync();
            return cobro;
        }

        public async Task<bool> ExisteFamiliarAsync(int idFamiliar)
        {
            return await _context.Familiares.AnyAsync(f => f.IdFamiliar == idFamiliar);
        }

        public async Task<decimal?> PendienteDeVisitaAsync(int idVisitaMedica)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.ExamenesSolicitados)
                .Include(v => v.Prescripciones)
                .FirstOrDefaultAsync(v => v.IdVisitaMedica == idVisitaMedica);
            if (visita == null) return null;

            var cargos = (visita.CostoConsulta > 0 ? visita.CostoConsulta : 0)
                + visita.ExamenesSolicitados.Where(e => e.Costo > 0).Sum(e => e.Costo!.Value)
                + visita.Prescripciones.Where(p => p.Costo > 0).Sum(p => p.Costo!.Value);
            var pagado = await _context.Cobros.Where(c => c.IdVisitaMedica == idVisitaMedica).SumAsync(c => (decimal?)c.Monto) ?? 0;
            return cargos - pagado;
        }

        public async Task<bool> VisitaEsDelFamiliarAsync(int idVisitaMedica, int idFamiliar)
        {
            return await _context.VisitasMedicas.AnyAsync(v =>
                v.IdVisitaMedica == idVisitaMedica &&
                v.Solicitud.Interno.Familiar != null &&
                v.Solicitud.Interno.Familiar.IdFamiliar == idFamiliar);
        }
    }
}
