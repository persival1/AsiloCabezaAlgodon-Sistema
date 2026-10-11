using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class VisitaMedicaRepository : IVisitaMedicaRepository
    {
        private readonly BDConexion _context;

        public VisitaMedicaRepository(BDConexion context)
        {
            _context = context;
        }

        public async Task<List<VisitaMedica>> ObtenerTodasAsync()
        {
            return await _context.VisitasMedicas
                .Include(v => v.Solicitud).ThenInclude(s => s.Interno)
                .Include(v => v.Solicitud).ThenInclude(s => s.Especialista)
                .Include(v => v.ExamenesSolicitados)
                .Include(v => v.Prescripciones)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();
        }

        public async Task<VisitaMedica> CrearAsync(VisitaMedica visita)
        {
            _context.VisitasMedicas.Add(visita);
            await _context.SaveChangesAsync();
            return visita;
        }

        public async Task<List<ExamenSolicitado>> ObtenerExamenesAsync()
        {
            return await _context.ExamenesSolicitados
                .Include(e => e.VisitaMedica).ThenInclude(v => v.Solicitud).ThenInclude(s => s.Interno)
                .OrderBy(e => e.Estado == "Pendiente" ? 0 : 1)
                .ThenByDescending(e => e.VisitaMedica.FechaVisita)
                .ToListAsync();
        }

        public async Task<List<Prescripcion>> ObtenerPrescripcionesAsync()
        {
            return await _context.Prescripciones
                .Include(p => p.VisitaMedica).ThenInclude(v => v.Solicitud).ThenInclude(s => s.Interno)
                .OrderBy(p => p.Estado == "Pendiente" ? 0 : 1)
                .ThenByDescending(p => p.VisitaMedica.FechaVisita)
                .ToListAsync();
        }

        public async Task<ExamenSolicitado?> ObtenerExamenPorIdAsync(int id)
        {
            return await _context.ExamenesSolicitados
                .Include(e => e.VisitaMedica).ThenInclude(v => v.Solicitud).ThenInclude(s => s.Interno)
                .FirstOrDefaultAsync(e => e.IdExamenSolicitado == id);
        }

        public async Task ActualizarExamenAsync(ExamenSolicitado examen)
        {
            _context.ExamenesSolicitados.Update(examen);
            await _context.SaveChangesAsync();
        }

        public async Task<Prescripcion?> ObtenerPrescripcionPorIdAsync(int id)
        {
            return await _context.Prescripciones
                .Include(p => p.VisitaMedica).ThenInclude(v => v.Solicitud).ThenInclude(s => s.Interno)
                .FirstOrDefaultAsync(p => p.IdPrescripcion == id);
        }

        public async Task ActualizarPrescripcionAsync(Prescripcion prescripcion)
        {
            _context.Prescripciones.Update(prescripcion);
            await _context.SaveChangesAsync();
        }

        public async Task CargarCostoAFamiliarAsync(int idVisitaMedica, decimal monto)
        {
            if (monto <= 0)
                return;

            var visita = await _context.VisitasMedicas
                .Include(v => v.Solicitud).ThenInclude(s => s.Interno).ThenInclude(i => i.Familiar)
                .FirstOrDefaultAsync(v => v.IdVisitaMedica == idVisitaMedica);

            var familiar = visita?.Solicitud.Interno.Familiar;
            if (familiar == null)
                return;

            familiar.SaldoPendiente += monto;
            await _context.SaveChangesAsync();
        }
    }
}
