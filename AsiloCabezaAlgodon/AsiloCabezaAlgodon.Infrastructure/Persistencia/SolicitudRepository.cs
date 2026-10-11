using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class SolicitudRepository : ISolicitudRepository
    {
        private readonly BDConexion _context;

        public SolicitudRepository(BDConexion context)
        {
            _context = context;
        }

        private IQueryable<Solicitud> ConIncludes()
        {
            return _context.Solicitudes
                .Include(s => s.Interno).ThenInclude(i => i.Familiar)
                .Include(s => s.MedicoGeneral)
                .Include(s => s.Especialista)
                .Include(s => s.Enfermero);
        }

        public async Task<List<Solicitud>> ObtenerTodasAsync()
        {
            return await ConIncludes()
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Solicitud?> ObtenerPorIdAsync(int id)
        {
            return await ConIncludes().FirstOrDefaultAsync(s => s.IdSolicitud == id);
        }

        public async Task<Solicitud> CrearAsync(Solicitud solicitud)
        {
            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();
            return solicitud;
        }

        public async Task<bool> ActualizarAsync(Solicitud solicitud)
        {
            _context.Solicitudes.Update(solicitud);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
