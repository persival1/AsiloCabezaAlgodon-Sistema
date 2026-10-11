using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Caja.Cobros;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class CobrosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CobrosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var cobros = await _mediator.Send(new ListarCobrosQuery());
            return Ok(cobros);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearCobroCommand command)
        {
            var idCobro = await _mediator.Send(command);
            return Ok(new { idCobro });
        }
    }
}
