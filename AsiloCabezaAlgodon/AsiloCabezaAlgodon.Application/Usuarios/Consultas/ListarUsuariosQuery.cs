using MediatR;

namespace AsiloCabezaAlgodon.Application.Usuarios.Consultas
{
    public record UsuarioDto(int IdUsuario, string NombreUsuario, string NombreCompleto, string Correo, string Rol, string Estado, string? Especialidad);

    public record ListarUsuariosQuery() : IRequest<List<UsuarioDto>>;
}