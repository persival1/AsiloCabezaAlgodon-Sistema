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
    }
}
