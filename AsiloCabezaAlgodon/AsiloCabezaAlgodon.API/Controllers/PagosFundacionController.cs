using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Caja.PagosFundacion;

namespace AsiloCabezaAlgodon.API.Controllers
{
    // AC-07: pagos que el asilo le hace a la fundación. Solo la administración los registra.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class PagosFundacionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PagosFundacionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            return Ok(await _mediator.Send(new ListarPagosFundacionQuery()));
        }

        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarPagoFundacionCommand command)
        {
            var idPagoFundacion = await _mediator.Send(command);
            return Ok(new { idPagoFundacion });
        }
    }
}
