using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Consultas;
using AsiloCabezaAlgodon.Application.Internos.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisitasMedicasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VisitasMedicasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Especialista,MedicoGeneral")]
        public async Task<IActionResult> Listar(int? idInterno)
        {
            var rol = User.FindFirstValue(ClaimTypes.Role)!;
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Si se pide el historial de un interno puntual (para ver su ficha médica),
            // se muestra completo sin importar qué especialista atendió cada visita.
            // AC-04: pero el especialista solo puede pedirlo de sus pacientes.
            if (idInterno.HasValue && !await _mediator.Send(new PuedeVerInternoQuery(idInterno.Value, idUsuario, rol)))
                return Forbid();

            var query = idInterno.HasValue
                ? new ListarVisitasQuery(IdInterno: idInterno)
                : User.FindFirstValue(ClaimTypes.Role) == "Especialista"
                    ? new ListarVisitasQuery(IdEspecialista: int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
                    : new ListarVisitasQuery();

            var visitas = await _mediator.Send(query);
            if (rol != "Administrador")
                visitas = visitas.Select(v => v with { TotalCobrado = null, EstadoPago = null }).ToList();
            return Ok(visitas);
        }

        [HttpPost]
        [Authorize(Roles = "Especialista")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarVisitaCommand command)
        {
            // AUT-08: el especialista sale del token, no del cuerpo de la petición.
            var idEspecialista = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var idVisita = await _mediator.Send(command with { IdEspecialista = idEspecialista });
            return Ok(new { idVisita });
        }
    }
}
