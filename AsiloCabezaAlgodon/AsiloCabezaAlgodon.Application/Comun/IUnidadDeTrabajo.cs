namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    // Plan de pruebas SQA, semana 4 (ROB-03): algunas operaciones guardan varias cosas
    // seguidas (por ejemplo, la visita y el cargo al familiar). Todo lo que se haga dentro de
    // "operacion" queda en una sola transacción: se guardan todos los cambios o ninguno.
    public interface IUnidadDeTrabajo
    {
        Task EjecutarEnTransaccionAsync(Func<Task> operacion);
    }
}
