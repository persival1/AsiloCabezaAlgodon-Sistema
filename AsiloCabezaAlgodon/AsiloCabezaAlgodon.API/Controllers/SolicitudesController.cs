using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Solicitudes.Comandos;
using AsiloCabezaAlgodon.Application.Solicitudes.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolicitudesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SolicitudesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private int IdUsuarioActual =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string RolActual =>
            User.FindFirstValue(ClaimTypes.Role)!;

        // CORRECCIÓN (observación SQA #3 - permisos por rol):
        // Antes cualquier usuario con sesión podía llamar a este endpoint, y los roles que
        // no estaban en el "switch" de abajo (Laboratorio y Farmacia) caían en la opción por
        // defecto y recibían TODAS las solicitudes. Laboratorio y Farmacia no trabajan con
        // solicitudes (solo con exámenes y prescripciones), así que ahora se rechazan con 403.
        [HttpGet]
        [Authorize(Roles = "Administrador,MedicoGeneral,Especialista,Enfermero")]
        public async Task<IActionResult> Listar()
        {
            // Cada rol solo ve lo que le corresponde. Con el [Authorize] de arriba, la opción
            // por defecto (_) ya solo puede ser el Administrador, que ve todo.
            var query = RolActual switch
            {
                "MedicoGeneral" => new ListarSolicitudesQuery(IdMedicoGeneral: IdUsuarioActual),
                "Especialista" => new ListarSolicitudesQuery(IdEspecialista: IdUsuarioActual),
                "Enfermero" => new ListarSolicitudesQuery(IdEnfermero: IdUsuarioActual),
                _ => new ListarSolicitudesQuery()
            };

            var solicitudes = await _mediator.Send(query);
            return Ok(solicitudes);
        }

        [HttpPost]
        [Authorize(Roles = "MedicoGeneral")]
        public async Task<IActionResult> Crear([FromBody] CrearSolicitudRequest request)
        {
            var idSolicitud = await _mediator.Send(new CrearSolicitudCommand(request.IdInterno, request.Motivo, request.EspecialidadRequerida, IdUsuarioActual));
            return Ok(new { idSolicitud });
        }

        // AC-08, CU-013: el médico general corrige su solicitud mientras está Pendiente
        [HttpPut("{id}")]
        [Authorize(Roles = "MedicoGeneral")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarSolicitudRequest request)
        {
            var ok = await _mediator.Send(new EditarSolicitudCommand(id, request.Motivo, request.EspecialidadRequerida, IdUsuarioActual));
            return ok ? NoContent() : NotFound();
        }

        // AC-08, CU-014 y CU-019: cancelar una solicitud pendiente o una cita asignada
        [HttpPut("{id}/cancelar")]
        [Authorize(Roles = "Administrador,MedicoGeneral")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var resultado = await _mediator.Send(new CancelarSolicitudCommand(id, IdUsuarioActual, RolActual));
            if (resultado == null)
                return NotFound();
            return Ok(new { mensaje = "Solicitud cancelada.", correoEnviado = resultado.CorreoEnviado });
        }

        // CU-018 (reprogramar una cita): es este mismo endpoint sobre una solicitud Asignada
        [HttpPut("{id}/asignar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Asignar(int id, [FromBody] AsignarMedicoRequest request)
        {
            var resultado = await _mediator.Send(new AsignarMedicoCommand(id, request.IdEspecialista, request.IdEnfermero, request.FechaHoraCita));

            if (resultado == null)
                return NotFound();

            // COR-04: antes decía «correo enviado» siempre, aunque no se hubiera enviado.
            var mensaje = resultado.CorreoEnviado
                ? "Solicitud asignada y correo enviado al familiar."
                : "Solicitud asignada, pero el correo al familiar no se pudo enviar.";

            return Ok(new { mensaje, correoEnviado = resultado.CorreoEnviado });
        }
    }

    public record CrearSolicitudRequest(int IdInterno, string Motivo, string? EspecialidadRequerida);

    public record EditarSolicitudRequest(string Motivo, string? EspecialidadRequerida);

    public record AsignarMedicoRequest(int IdEspecialista, int? IdEnfermero, DateTime FechaHoraCita);
}
