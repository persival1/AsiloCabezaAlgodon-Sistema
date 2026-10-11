using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Caja.Donaciones;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class DonacionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DonacionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var donaciones = await _mediator.Send(new ListarDonacionesQuery());
            return Ok(donaciones);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearDonacionCommand command)
        {
            var idDonacion = await _mediator.Send(command);
            return Ok(new { idDonacion });
        }
    }
}
