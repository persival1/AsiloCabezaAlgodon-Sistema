using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Validación de datos de la visita (FAR-03 y costo de consulta).
    // Las reglas de estado y de especialista asignado (AUT-08, EXP-07) son de la semana 3.
    public class RegistrarVisitaCommandHandlerTests
    {
        private readonly IVisitaMedicaRepository _visitas = Substitute.For<IVisitaMedicaRepository>();
        private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();

        private static RegistrarVisitaCommand Visita(decimal costo, string medicamento) => new(
            IdSolicitud: 1, Diagnostico: "Lumbalgia mecánica", Observaciones: "Reposo relativo",
            CostoConsulta: costo,
            Examenes: new List<ExamenInput> { new("Hemograma") },
            Prescripciones: new List<PrescripcionInput> { new(medicamento, "50 mg", "Cada 12 horas por 5 días") });

        [Fact]
        public async Task Costo_de_consulta_negativo_se_rechaza()
        {
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(Visita(-150m, "Diclofenaco"), CancellationToken.None));

            await _visitas.DidNotReceive().CrearAsync(Arg.Any<VisitaMedica>());
        }

        [Fact] // FAR-03: prescripción sin nombre de medicamento
        public async Task Prescripcion_sin_nombre_se_rechaza()
        {
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(Visita(150m, ""), CancellationToken.None));

            await _visitas.DidNotReceive().CrearAsync(Arg.Any<VisitaMedica>());
        }
    }
}
