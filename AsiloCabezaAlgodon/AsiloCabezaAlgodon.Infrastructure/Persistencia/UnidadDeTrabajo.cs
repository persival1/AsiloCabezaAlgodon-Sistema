using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    // Todos los repositorios de una misma petición comparten este BDConexion (AddScoped), así
    // que la transacción abarca lo que guarde cualquiera de ellos dentro de "operacion".
    public class UnidadDeTrabajo : IUnidadDeTrabajo
    {
        private readonly BDConexion _context;

        public UnidadDeTrabajo(BDConexion context)
        {
            _context = context;
        }

        public async Task EjecutarEnTransaccionAsync(Func<Task> operacion)
        {
            // Con EnableRetryOnFailure (Program.cs), EF exige abrir la transacción dentro de la
            // estrategia de ejecución, para poder repetirla completa si la base no responde.
            var estrategia = _context.Database.CreateExecutionStrategy();
            await estrategia.ExecuteAsync(async () =>
            {
                await using var transaccion = await _context.Database.BeginTransactionAsync();
                await operacion();
                // Si "operacion" lanza una excepción, no se llega aquí y al cerrarse la
                // transacción sin confirmar se deshacen todos los cambios.
                await transaccion.CommitAsync();
            });
        }
    }
}
