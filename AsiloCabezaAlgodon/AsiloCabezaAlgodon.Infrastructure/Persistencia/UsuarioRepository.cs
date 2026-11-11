using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly BDConexion _context;

        public UsuarioRepository(BDConexion context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.IdUsuario)
                .ToListAsync();
        }

        public async Task<bool> TieneRolAsync(int idUsuario, string rol)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == idUsuario && u.Rol.Nombre == rol && u.Estado == "activo");
        }

        public Task<Usuario?> ObtenerPorIdAsync(int idUsuario) =>
            _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        public Task<Rol?> ObtenerRolAsync(string nombre) =>
            _context.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre);

        public Task<int> ContarActivosConRolAsync(string rol) =>
            _context.Usuarios.CountAsync(u => u.Rol.Nombre == rol && u.Estado == "activo");

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public Task ActualizarAsync(Usuario usuario) => _context.SaveChangesAsync();
    }
}