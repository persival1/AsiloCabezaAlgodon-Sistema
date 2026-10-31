using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Internos.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Casos RES-02 (campos obligatorios) y RES-03 (formatos) del plan de SQA
    public class CrearInternoCommandHandlerTests
    {
        private readonly IInternoRepository _repo = Substitute.For<IInternoRepository>();

        // Datos válidos del plan (Interno Prueba A); cada prueba cambia solo un dato
        private static CrearInternoCommand Valido() => new(
            Nombres: "Interno", Apellidos: "Prueba A",
            FechaNacimiento: new DateTime(1945, 3, 15), FechaIngreso: new DateTime(2026, 10, 1),
            MotivoIngreso: "Cuidado permanente",
            NombreFamiliar: "Familiar", ApellidoFamiliar: "Prueba A", Parentesco: "Hijo",
            TelefonoFamiliar: "5555-0001", CorreoFamiliar: "familiar.a@ejemplo.test",
            CuotaMensual: 500m);

        public CrearInternoCommandHandlerTests()
        {
            _repo.CrearAsync(Arg.Any<Interno>()).Returns(c => c.Arg<Interno>());
        }

        public static TheoryData<CrearInternoCommand> Invalidos() => new()
        {
            Valido() with { Nombres = "" },                                  // RES-02
            Valido() with { Apellidos = "  " },                              // RES-02
            Valido() with { CorreoFamiliar = "" },                           // RES-02
            Valido() with { CorreoFamiliar = "familiar.ejemplo" },           // RES-03
            Valido() with { CuotaMensual = -1m },                            // RES-03
            Valido() with { FechaNacimiento = new DateTime(2030, 1, 1) },    // RES-03
            Valido() with { FechaIngreso = new DateTime(1940, 1, 1) },       // RES-03: ingreso antes del nacimiento
        };

        [Theory]
        [MemberData(nameof(Invalidos))]
        public async Task Datos_invalidos_se_rechazan_sin_crear_registros(CrearInternoCommand comando)
        {
            var handler = new CrearInternoCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() => handler.Handle(comando, CancellationToken.None));

            await _repo.DidNotReceive().CrearAsync(Arg.Any<Interno>());
        }

        [Fact] // RES-01
        public async Task Datos_validos_crean_el_interno_con_su_familiar()
        {
            var handler = new CrearInternoCommandHandler(_repo);

            await handler.Handle(Valido(), CancellationToken.None);

            await _repo.Received(1).CrearAsync(Arg.Is<Interno>(i => i.Familiar != null && i.Familiar.SaldoPendiente == 0));
        }
    }
}
