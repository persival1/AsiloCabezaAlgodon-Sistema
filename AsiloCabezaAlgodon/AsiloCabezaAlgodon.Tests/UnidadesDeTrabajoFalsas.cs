using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Tests
{
    // Para las pruebas unitarias no hay base de datos, así que no hay transacción real.
    // La transacción de verdad se prueba en Integracion/RegistrarVisitaTransaccionTests.

    // Ejecuta la operación tal cual (como si la transacción siempre se confirmara).
    public class SinTransaccion : IUnidadDeTrabajo
    {
        public Task EjecutarEnTransaccionAsync(Func<Task> operacion) => operacion();
    }

    // Nunca ejecuta la operación. Sirve para comprobar que el handler hace TODOS sus
    // guardados dentro de la transacción: si alguno quedara afuera, se ejecutaría igual.
    public class TransaccionQueNoEjecuta : IUnidadDeTrabajo
    {
        public Task EjecutarEnTransaccionAsync(Func<Task> operacion) => Task.CompletedTask;
    }
}
