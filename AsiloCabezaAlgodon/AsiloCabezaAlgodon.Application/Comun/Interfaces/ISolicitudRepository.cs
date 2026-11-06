using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface ISolicitudRepository
    {
        Task<List<Solicitud>> ObtenerTodasAsync();
        Task<Solicitud?> ObtenerPorIdAsync(int id);
        Task<Solicitud> CrearAsync(Solicitud solicitud);
        Task<bool> ActualizarAsync(Solicitud solicitud);
    }
}
