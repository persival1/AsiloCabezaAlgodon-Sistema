using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Usuarios.Comandos
{
    // AC-08, CU-004: el Administrador crea, modifica y desactiva usuarios.
    // AC-05: el Administrador restablece la contraseña de quien la olvidó (con una temporal que
    // le entrega en persona) y cada usuario puede cambiar la suya. La recuperación por correo
    // queda excluida mientras el correo de Azure no esté configurado (AC-11): docs/ac-05-contrasenas.md
    public static class RolesDelSistema
    {
        public static readonly string[] Todos = { "Administrador", "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" };
    }

    public record CrearUsuarioCommand(string NombreUsuario, string NombreCompleto, string Correo, string Rol, string? Especialidad, string Clave) : IRequest<int>;
    public record EditarUsuarioCommand(int IdUsuario, string NombreCompleto, string Correo, string Rol, string? Especialidad, int IdAdministradorActual) : IRequest<bool>;
    public record CambiarEstadoUsuarioCommand(int IdUsuario, string Estado, int IdAdministradorActual) : IRequest<bool>;
    public record RestablecerClaveCommand(int IdUsuario, string ClaveTemporal) : IRequest<bool>;
    public record CambiarClaveCommand(int IdUsuario, string ClaveActual, string ClaveNueva) : IRequest<bool>;

    public class UsuariosCommandHandler :
        IRequestHandler<CrearUsuarioCommand, int>,
        IRequestHandler<EditarUsuarioCommand, bool>,
        IRequestHandler<CambiarEstadoUsuarioCommand, bool>,
        IRequestHandler<RestablecerClaveCommand, bool>,
        IRequestHandler<CambiarClaveCommand, bool>
    {
        private readonly IUsuarioRepository _repo;
        private readonly IPasswordHasher _hasher;

        public UsuariosCommandHandler(IUsuarioRepository repo, IPasswordHasher hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        private async Task<Rol> ValidarDatos(string nombreCompleto, string correo, string rol, string? especialidad)
        {
            Validar.Texto(nombreCompleto, "nombre completo");
            Validar.Correo(correo, "correo");
            if (!RolesDelSistema.Todos.Contains(rol))
                throw new DatosInvalidosException("El rol elegido no existe.");
            if (rol == "Especialista")
                Validar.Texto(especialidad, "especialidad");
            return await _repo.ObtenerRolAsync(rol)
                ?? throw new DatosInvalidosException("El rol elegido no existe en la base de datos.");
        }

        public async Task<int> Handle(CrearUsuarioCommand request, CancellationToken cancellationToken)
        {
            Validar.Texto(request.NombreUsuario, "usuario");
            var nombreUsuario = request.NombreUsuario.Trim().ToLowerInvariant();
            if (nombreUsuario.Length < 3 || !nombreUsuario.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_'))
                throw new DatosInvalidosException("El usuario debe tener al menos 3 caracteres: letras, números, punto o guion bajo, sin espacios.");
            var rol = await ValidarDatos(request.NombreCompleto, request.Correo, request.Rol, request.Especialidad);
            Validar.Clave(request.Clave);
            if (await _repo.ObtenerPorNombreUsuarioAsync(nombreUsuario) != null)
                throw new ConflictoException($"Ya existe el usuario «{nombreUsuario}».");

            var usuario = await _repo.CrearAsync(new Usuario
            {
                NombreUsuario = nombreUsuario,
                NombreCompleto = request.NombreCompleto.Trim(),
                Correo = request.Correo.Trim(),
                IdRol = rol.IdRol,
                Especialidad = request.Rol == "Especialista" ? request.Especialidad!.Trim() : null,
                PasswordHash = _hasher.Hashear(request.Clave),
                Estado = "activo",
            });
            return usuario.IdUsuario;
        }

        public async Task<bool> Handle(EditarUsuarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _repo.ObtenerPorIdAsync(request.IdUsuario);
            if (usuario == null) return false;
            var rol = await ValidarDatos(request.NombreCompleto, request.Correo, request.Rol, request.Especialidad);

            // Siempre debe quedar al menos un Administrador activo, y nadie se quita a sí mismo el rol.
            if (usuario.Rol.Nombre == "Administrador" && request.Rol != "Administrador")
            {
                if (usuario.IdUsuario == request.IdAdministradorActual)
                    throw new ConflictoException("No podés quitarte a vos mismo el rol de Administrador.");
                if (usuario.Estado == "activo" && await _repo.ContarActivosConRolAsync("Administrador") <= 1)
                    throw new ConflictoException("Debe quedar al menos un Administrador activo.");
            }

            usuario.NombreCompleto = request.NombreCompleto.Trim();
            usuario.Correo = request.Correo.Trim();
            usuario.IdRol = rol.IdRol;
            usuario.Rol = rol;
            usuario.Especialidad = request.Rol == "Especialista" ? request.Especialidad!.Trim() : null;
            await _repo.ActualizarAsync(usuario);
            return true;
        }

        // Desactivar no borra: el usuario sigue en las visitas y solicitudes que atendió,
        // pero ya no puede iniciar sesión ni ser asignado a citas nuevas.
        public async Task<bool> Handle(CambiarEstadoUsuarioCommand request, CancellationToken cancellationToken)
        {
            var estado = (request.Estado ?? "").Trim().ToLowerInvariant();
            if (estado != "activo" && estado != "inactivo")
                throw new DatosInvalidosException("El estado debe ser «activo» o «inactivo».");
            var usuario = await _repo.ObtenerPorIdAsync(request.IdUsuario);
            if (usuario == null) return false;
            if (estado == "inactivo")
            {
                if (usuario.IdUsuario == request.IdAdministradorActual)
                    throw new ConflictoException("No podés desactivar tu propio usuario.");
                if (usuario.Rol.Nombre == "Administrador" && usuario.Estado == "activo" && await _repo.ContarActivosConRolAsync("Administrador") <= 1)
                    throw new ConflictoException("Debe quedar al menos un Administrador activo.");
            }
            usuario.Estado = estado;
            await _repo.ActualizarAsync(usuario);
            return true;
        }

        public async Task<bool> Handle(RestablecerClaveCommand request, CancellationToken cancellationToken)
        {
            Validar.Clave(request.ClaveTemporal);
            var usuario = await _repo.ObtenerPorIdAsync(request.IdUsuario);
            if (usuario == null) return false;
            usuario.PasswordHash = _hasher.Hashear(request.ClaveTemporal);
            await _repo.ActualizarAsync(usuario);
            return true;
        }

        public async Task<bool> Handle(CambiarClaveCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _repo.ObtenerPorIdAsync(request.IdUsuario);
            if (usuario == null || usuario.Estado != "activo") return false;
            if (string.IsNullOrEmpty(request.ClaveActual) || !_hasher.Verificar(request.ClaveActual, usuario.PasswordHash))
                throw new DatosInvalidosException("La contraseña actual no es correcta.");
            Validar.Clave(request.ClaveNueva);
            if (request.ClaveNueva == request.ClaveActual)
                throw new DatosInvalidosException("La contraseña nueva debe ser distinta de la actual.");
            usuario.PasswordHash = _hasher.Hashear(request.ClaveNueva);
            await _repo.ActualizarAsync(usuario);
            return true;
        }
    }
}
