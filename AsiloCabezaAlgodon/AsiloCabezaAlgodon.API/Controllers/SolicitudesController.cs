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

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            // Cada rol solo ve lo que le corresponde; Administrador ve todo.
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

        [HttpPut("{id}/asignar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Asignar(int id, [FromBody] AsignarMedicoRequest request)
        {
            var exito = await _mediator.Send(new AsignarMedicoCommand(id, request.IdEspecialista, request.IdEnfermero, request.FechaHoraCita));

            if (!exito)
                return NotFound();

            return Ok(new { mensaje = "Solicitud asignada y correo enviado al familiar." });
        }
    }

    public record CrearSolicitudRequest(int IdInterno, string Motivo, string? EspecialidadRequerida);

    public record AsignarMedicoRequest(int IdEspecialista, int? IdEnfermero, DateTime FechaHoraCita);
}
