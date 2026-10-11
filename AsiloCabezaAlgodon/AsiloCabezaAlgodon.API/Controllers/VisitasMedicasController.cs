using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Consultas;

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
            // Si se pide el historial de un interno puntual (para ver su ficha médica),
            // se muestra completo sin importar qué especialista atendió cada visita.
            var query = idInterno.HasValue
                ? new ListarVisitasQuery(IdInterno: idInterno)
                : User.FindFirstValue(ClaimTypes.Role) == "Especialista"
                    ? new ListarVisitasQuery(IdEspecialista: int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
                    : new ListarVisitasQuery();

            var visitas = await _mediator.Send(query);
            return Ok(visitas);
        }

        [HttpPost]
        [Authorize(Roles = "Especialista")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarVisitaCommand command)
        {
            var idVisita = await _mediator.Send(command);
            return Ok(new { idVisita });
        }
    }
}
