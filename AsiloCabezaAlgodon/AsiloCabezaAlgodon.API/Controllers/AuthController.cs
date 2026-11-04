using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using AsiloCabezaAlgodon.Application.Autenticacion.Comandos;

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
    }
}