using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class InternoRepository : IInternoRepository
    {
        private readonly BDConexion _context;

        public InternoRepository(BDConexion context)
        {
            _context = context;
        }

        public async Task<List<Interno>> ObtenerTodosAsync()
        {
            return await _context.Internos
                .Include(i => i.Familiar)
                .OrderBy(i => i.Apellidos)
                .ToListAsync();
        }

        public async Task<Interno?> ObtenerPorIdAsync(int id)
        {
            return await _context.Internos
                .Include(i => i.Familiar)
                .Include(i => i.Psicopatologias)
                .Include(i => i.MedicamentosCajon)
                .FirstOrDefaultAsync(i => i.IdInterno == id);
        }

        public async Task<Interno> CrearAsync(Interno interno)
        {
            _context.Internos.Add(interno);
            await _context.SaveChangesAsync();
            return interno;
        }

        public async Task<bool> ActualizarAsync(Interno interno)
        {
            _context.Internos.Update(interno);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Psicopatologia> AgregarPsicopatologiaAsync(Psicopatologia psicopatologia)
        {
            _context.Psicopatologias.Add(psicopatologia);
            await _context.SaveChangesAsync();
            return psicopatologia;
        }

        public async Task<MedicamentoCajon> AgregarMedicamentoCajonAsync(MedicamentoCajon medicamento)
        {
            _context.MedicamentosCajon.Add(medicamento);
            await _context.SaveChangesAsync();
            return medicamento;
        }

        // AC-08: mismo nombre, apellidos y fecha de nacimiento = la misma persona.
        // SQL Server compara sin distinguir mayúsculas (intercalación por defecto).
        public async Task<bool> ExisteDuplicadoAsync(string nombres, string apellidos, DateTime fechaNacimiento, int? excluirIdInterno = null)
        {
            var n = nombres.Trim();
            var a = apellidos.Trim();
            return await _context.Internos.AnyAsync(i =>
                i.Nombres == n && i.Apellidos == a && i.FechaNacimiento.Date == fechaNacimiento.Date &&
                (excluirIdInterno == null || i.IdInterno != excluirIdInterno));
        }

        public Task<Psicopatologia?> ObtenerPsicopatologiaAsync(int idPsicopatologia) =>
            _context.Psicopatologias.FirstOrDefaultAsync(p => p.IdPsicopatologia == idPsicopatologia);

        public Task<MedicamentoCajon?> ObtenerMedicamentoCajonAsync(int idMedicamentoCajon) =>
            _context.MedicamentosCajon.FirstOrDefaultAsync(m => m.IdMedicamentoCajon == idMedicamentoCajon);

        public async Task EliminarPsicopatologiaAsync(Psicopatologia psicopatologia)
        {
            _context.Psicopatologias.Remove(psicopatologia);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarMedicamentoCajonAsync(MedicamentoCajon medicamento)
        {
            _context.MedicamentosCajon.Remove(medicamento);
            await _context.SaveChangesAsync();
        }

        public Task GuardarCambiosAsync() => _context.SaveChangesAsync();
    }
}
