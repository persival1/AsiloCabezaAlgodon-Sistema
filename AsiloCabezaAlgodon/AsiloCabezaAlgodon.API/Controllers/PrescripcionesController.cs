using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Prescripciones.Comandos;
using AsiloCabezaAlgodon.Application.Prescripciones.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Farmacia,Administrador")]
    public class PrescripcionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrescripcionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var prescripciones = await _mediator.Send(new ListarPrescripcionesQuery());
            return Ok(prescripciones);
        }

        [HttpPut("{id}/despachar")]
        public async Task<IActionResult> Despachar(int id, [FromBody] DespacharPrescripcionRequest request)
        {
            var exito = await _mediator.Send(new DespacharPrescripcionCommand(id, request.Costo));

            if (!exito)
                return NotFound();

            return Ok();
        }
    }

    public record DespacharPrescripcionRequest(decimal Costo);
}
