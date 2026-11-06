using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IInternoRepository
    {
        Task<List<Interno>> ObtenerTodosAsync();
        Task<Interno?> ObtenerPorIdAsync(int id);
        Task<Interno> CrearAsync(Interno interno);
        Task<bool> ActualizarAsync(Interno interno);
        Task<Psicopatologia> AgregarPsicopatologiaAsync(Psicopatologia psicopatologia);
        Task<MedicamentoCajon> AgregarMedicamentoCajonAsync(MedicamentoCajon medicamento);
    }
}