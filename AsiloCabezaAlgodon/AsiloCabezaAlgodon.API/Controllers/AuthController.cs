using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using AsiloCabezaAlgodon.Application.Autenticacion.Comandos;
using AsiloCabezaAlgodon.Application.Usuarios.Comandos;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")] // semana 4: máximo 10 intentos por minuto (ver Program.cs)
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Exitoso)
                return Unauthorized(result);

            return Ok(result);
        }

        // AC-05: cada usuario cambia su propia contraseña (el usuario sale del token, no del cuerpo).
        // Comparte el límite de intentos del login para que no sirva para adivinar contraseñas.
        [HttpPut("cambiar-clave")]
        [Authorize]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> CambiarClave([FromBody] CambiarClaveRequest request)
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ok = await _mediator.Send(new CambiarClaveCommand(idUsuario, request.ClaveActual, request.ClaveNueva));
            return ok ? NoContent() : Unauthorized();
        }
    }

    public record CambiarClaveRequest(string ClaveActual, string ClaveNueva);
}