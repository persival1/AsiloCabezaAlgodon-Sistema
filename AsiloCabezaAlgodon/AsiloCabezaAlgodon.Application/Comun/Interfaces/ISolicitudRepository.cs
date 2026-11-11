using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<List<Solicitud>> ObtenerTodasAsync();
        Task<Solicitud?> ObtenerPorIdAsync(int id);
        Task<Solicitud> CrearAsync(Solicitud solicitud);
        Task<bool> ActualizarAsync(Solicitud solicitud);

        // AC-04: internos de las solicitudes asignadas a un especialista (acceso horizontal)
        Task<List<int>> InternosDeEspecialistaAsync(int idEspecialista);
    }
}
