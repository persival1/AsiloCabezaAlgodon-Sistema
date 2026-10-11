using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Usuarios.Consultas
{
    public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, List<UsuarioDto>>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<List<UsuarioDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
        {
            var usuarios = await _usuarioRepository.ObtenerTodosAsync();

            return usuarios.Select(u => new UsuarioDto(
                u.IdUsuario,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Correo,
                u.Rol.Nombre,
                u.Estado,
                u.Especialidad
            )).ToList();
        }
    }
}