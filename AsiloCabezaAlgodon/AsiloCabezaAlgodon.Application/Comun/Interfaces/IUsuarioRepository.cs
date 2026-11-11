using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<List<Usuario>> ObtenerTodosAsync();

        // Semana 3 (EXP-06): para revisar que el especialista y el enfermero asignados
        // existan, estén activos y tengan ese rol.
        Task<bool> TieneRolAsync(int idUsuario, string rol);

        // AC-08 y AC-05: administración de usuarios y cambio de contraseña
        Task<Usuario?> ObtenerPorIdAsync(int idUsuario);
        Task<Rol?> ObtenerRolAsync(string nombre);
        Task<int> ContarActivosConRolAsync(string rol);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
    }
}