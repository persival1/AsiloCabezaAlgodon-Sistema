using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using AsiloCabezaAlgodon.Application.Usuarios.Comandos;
using AsiloCabezaAlgodon.Application.Usuarios.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private int IdUsuarioActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _mediator.Send(new ListarUsuariosQuery());
            return Ok(usuarios);
        }

        // AC-08, CU-004: crear, modificar y activar o desactivar usuarios
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioCommand command)
        {
            var idUsuario = await _mediator.Send(command);
            return Ok(new { idUsuario });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarUsuarioRequest r)
        {
            var ok = await _mediator.Send(new EditarUsuarioCommand(id, r.NombreCompleto, r.Correo, r.Rol, r.Especialidad, IdUsuarioActual));
            return ok ? NoContent() : NotFound();
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoUsuarioRequest r)
        {
            var ok = await _mediator.Send(new CambiarEstadoUsuarioCommand(id, r.Estado, IdUsuarioActual));
            return ok ? NoContent() : NotFound();
        }

        // AC-05: contraseña temporal para quien olvidó la suya (el Administrador se la entrega en persona)
        [HttpPut("{id}/clave")]
        public async Task<IActionResult> RestablecerClave(int id, [FromBody] RestablecerClaveRequest r)
        {
            var ok = await _mediator.Send(new RestablecerClaveCommand(id, r.ClaveTemporal));
            return ok ? NoContent() : NotFound();
        }
    }

    public record EditarUsuarioRequest(string NombreCompleto, string Correo, string Rol, string? Especialidad);
    public record EstadoUsuarioRequest(string Estado);
    public record RestablecerClaveRequest(string ClaveTemporal);
}