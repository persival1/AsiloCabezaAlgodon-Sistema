using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IVisitaMedicaRepository
    {
        Task<List<VisitaMedica>> ObtenerTodasAsync();
        Task<VisitaMedica> CrearAsync(VisitaMedica visita);
        Task<List<ExamenSolicitado>> ObtenerExamenesAsync();
        Task<List<Prescripcion>> ObtenerPrescripcionesAsync();
        Task<ExamenSolicitado?> ObtenerExamenPorIdAsync(int id);
        Task ActualizarExamenAsync(ExamenSolicitado examen);
        Task<Prescripcion?> ObtenerPrescripcionPorIdAsync(int id);
        Task ActualizarPrescripcionAsync(Prescripcion prescripcion);
        Task CargarCostoAFamiliarAsync(int idVisitaMedica, decimal monto);
    }
}
