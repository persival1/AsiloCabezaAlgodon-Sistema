using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Caja.Gastos;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class GastosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GastosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var gastos = await _mediator.Send(new ListarGastosQuery());
            return Ok(gastos);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearGastoCommand command)
        {
            var idGasto = await _mediator.Send(command);
            return Ok(new { idGasto });
        }
    }
}
