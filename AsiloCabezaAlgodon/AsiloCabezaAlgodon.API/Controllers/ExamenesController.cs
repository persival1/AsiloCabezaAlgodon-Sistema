using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Examenes.Comandos;
using AsiloCabezaAlgodon.Application.Examenes.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Laboratorio,Administrador")]
    public class ExamenesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExamenesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var examenes = await _mediator.Send(new ListarExamenesQuery());
            return Ok(examenes);
        }

        [HttpPut("{id}/resultado")]
        public async Task<IActionResult> RegistrarResultado(int id, [FromBody] RegistrarResultadoRequest request)
        {
            var exito = await _mediator.Send(new RegistrarResultadoCommand(id, request.Resultado, request.Costo));

            if (!exito)
                return NotFound();

            return Ok();
        }
    }

    public record RegistrarResultadoRequest(string Resultado, decimal Costo);
}
