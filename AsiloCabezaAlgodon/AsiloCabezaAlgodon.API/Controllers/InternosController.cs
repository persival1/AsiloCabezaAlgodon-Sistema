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

        // CORRECCIÓN (observación SQA #3 - permisos por rol):
        // Antes este endpoint solo tenía el [Authorize] general de la clase, o sea que
        // CUALQUIER usuario con sesión (Enfermero, Laboratorio, Farmacia) podía pedirlo
        // llamando a la API directamente, aunque su menú no mostrara "Internos".
        // Ocultar un botón en la interfaz no es seguridad: el servidor debe rechazarlo.
        // Ahora solo pueden leer internos los roles que necesitan la ficha para trabajar;
        // el enunciado indica que quien tiene acceso a la ficha médica es el médico.
        [HttpGet]
        [Authorize(Roles = "Administrador,MedicoGeneral,Especialista")]
        public async Task<IActionResult> Listar()
        {
            var internos = await _mediator.Send(new ListarInternosQuery());
            return Ok(internos);
        }

        // Misma corrección que arriba. Este endpoint devuelve la ficha completa
        // (psicopatologías, medicinas de cajón, datos y saldo del familiar), que es
        // información sensible y por eso queda restringida a los mismos tres roles.
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,MedicoGeneral,Especialista")]
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
