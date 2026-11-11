using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Reportes;
using AsiloCabezaAlgodon.Application.Internos.Consultas;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    // AC-10: cada informe declara sus roles. Los financieros son solo del Administrador; los
    // clínicos (ficha, exámenes y medicamentos de un paciente) también los usan el médico
    // general y el especialista, y este último solo para sus pacientes (acceso horizontal).
    public class ReportesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private const string Clinicos = "Administrador,MedicoGeneral,Especialista";

        private Task<bool> PuedeVer(int idInterno) => _mediator.Send(new PuedeVerInternoQuery(
            idInterno, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), User.FindFirstValue(ClaimTypes.Role)!));

        [HttpGet("costos-por-cita")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CostosPorCita()
        {
            return Ok(await _mediator.Send(new ReporteCostosPorCitaQuery()));
        }

        [HttpGet("ficha-medica/{idInterno}")]
        [Authorize(Roles = Clinicos)]
        public async Task<IActionResult> FichaMedica(int idInterno)
        {
            if (!await PuedeVer(idInterno)) return Forbid();
            var resultado = await _mediator.Send(new ReporteFichaMedicaQuery(idInterno));
            return resultado == null ? NotFound() : Ok(resultado);
        }

        [HttpGet("cobros-por-paciente")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CobrosPorPaciente(int idInterno, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = await _mediator.Send(new ReporteCobrosPorPacienteQuery(idInterno, fechaInicio, fechaFin));
            return resultado == null ? NotFound() : Ok(resultado);
        }

        [HttpGet("pagos-fundacion")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PagosFundacion(DateTime? fechaInicio, DateTime? fechaFin)
        {
            return Ok(await _mediator.Send(new ReportePagosFundacionQuery(fechaInicio, fechaFin)));
        }

        [HttpGet("entradas")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Entradas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            return Ok(await _mediator.Send(new ReporteEntradasQuery(fechaInicio, fechaFin)));
        }

        [HttpGet("examenes-por-paciente/{idInterno}")]
        [Authorize(Roles = Clinicos)]
        public async Task<IActionResult> ExamenesPorPaciente(int idInterno)
        {
            if (!await PuedeVer(idInterno)) return Forbid();
            return Ok(await _mediator.Send(new ReporteExamenesPorPacienteQuery(idInterno)));
        }

        [HttpGet("medicamentos-por-paciente/{idInterno}")]
        [Authorize(Roles = Clinicos)]
        public async Task<IActionResult> MedicamentosPorPaciente(int idInterno)
        {
            if (!await PuedeVer(idInterno)) return Forbid();
            return Ok(await _mediator.Send(new ReporteMedicamentosPorPacienteQuery(idInterno)));
        }

        // Observación SQA #4: recalcula el saldo de cada familiar desde sus movimientos
        // (costos médicos - cobros) y lo compara con el saldo guardado. Solo lectura.
        [HttpGet("conciliacion-saldos")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ConciliacionSaldos()
        {
            return Ok(await _mediator.Send(new ReporteConciliacionSaldosQuery()));
        }
    }
}
