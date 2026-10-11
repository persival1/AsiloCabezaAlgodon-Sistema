using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AsiloCabezaAlgodon.Application.Reportes;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class ReportesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("costos-por-cita")]
        public async Task<IActionResult> CostosPorCita()
        {
            return Ok(await _mediator.Send(new ReporteCostosPorCitaQuery()));
        }

        [HttpGet("ficha-medica/{idInterno}")]
        public async Task<IActionResult> FichaMedica(int idInterno)
        {
            var resultado = await _mediator.Send(new ReporteFichaMedicaQuery(idInterno));
            return resultado == null ? NotFound() : Ok(resultado);
        }

        [HttpGet("cobros-por-paciente")]
        public async Task<IActionResult> CobrosPorPaciente(int idInterno, DateTime fechaInicio, DateTime fechaFin)
        {
            var resultado = await _mediator.Send(new ReporteCobrosPorPacienteQuery(idInterno, fechaInicio, fechaFin));
            return resultado == null ? NotFound() : Ok(resultado);
        }

        [HttpGet("pagos-fundacion")]
        public async Task<IActionResult> PagosFundacion(DateTime? fechaInicio, DateTime? fechaFin)
        {
            return Ok(await _mediator.Send(new ReportePagosFundacionQuery(fechaInicio, fechaFin)));
        }

        [HttpGet("entradas")]
        public async Task<IActionResult> Entradas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            return Ok(await _mediator.Send(new ReporteEntradasQuery(fechaInicio, fechaFin)));
        }

        [HttpGet("examenes-por-paciente/{idInterno}")]
        public async Task<IActionResult> ExamenesPorPaciente(int idInterno)
        {
            return Ok(await _mediator.Send(new ReporteExamenesPorPacienteQuery(idInterno)));
        }

        [HttpGet("medicamentos-por-paciente/{idInterno}")]
        public async Task<IActionResult> MedicamentosPorPaciente(int idInterno)
        {
            return Ok(await _mediator.Send(new ReporteMedicamentosPorPacienteQuery(idInterno)));
        }
    }
}
