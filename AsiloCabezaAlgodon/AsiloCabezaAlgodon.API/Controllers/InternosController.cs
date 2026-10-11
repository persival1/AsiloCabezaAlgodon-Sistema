using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Internos.Comandos;
using AsiloCabezaAlgodon.Application.Internos.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InternosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InternosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var internos = await _mediator.Send(new ListarInternosQuery());
            return Ok(internos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var interno = await _mediator.Send(new ObtenerInternoQuery(id));

            if (interno == null)
                return NotFound();

            return Ok(interno);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,MedicoGeneral")]
        public async Task<IActionResult> Crear([FromBody] CrearInternoCommand command)
        {
            var idInterno = await _mediator.Send(command);
            return Ok(new { idInterno });
        }

        [HttpPost("{id}/psicopatologias")]
        [Authorize(Roles = "Administrador,MedicoGeneral,Especialista")]
        public async Task<IActionResult> AgregarPsicopatologia(int id, [FromBody] AgregarPsicopatologiaRequest request)
        {
            var idPsicopatologia = await _mediator.Send(new AgregarPsicopatologiaCommand(id, request.Nombre, request.FechaDiagnostico));
            return Ok(new { idPsicopatologia });
        }

        [HttpPost("{id}/medicamentos-cajon")]
        [Authorize(Roles = "Administrador,MedicoGeneral,Especialista")]
        public async Task<IActionResult> AgregarMedicamentoCajon(int id, [FromBody] AgregarMedicamentoCajonRequest request)
        {
            var idMedicamento = await _mediator.Send(new AgregarMedicamentoCajonCommand(id, request.Nombre, request.Dosis, request.Frecuencia));
            return Ok(new { idMedicamento });
        }
    }

    public record AgregarPsicopatologiaRequest(string Nombre, DateTime FechaDiagnostico);
    public record AgregarMedicamentoCajonRequest(string Nombre, string Dosis, string Frecuencia);
}
