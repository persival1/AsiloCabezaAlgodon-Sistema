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

        // AC-08: edición, alta/baja y corrección de la ficha
        Task<bool> ExisteDuplicadoAsync(string nombres, string apellidos, DateTime fechaNacimiento, int? excluirIdInterno = null);
        Task<Psicopatologia?> ObtenerPsicopatologiaAsync(int idPsicopatologia);
        Task<MedicamentoCajon?> ObtenerMedicamentoCajonAsync(int idMedicamentoCajon);
        Task EliminarPsicopatologiaAsync(Psicopatologia psicopatologia);
        Task EliminarMedicamentoCajonAsync(MedicamentoCajon medicamento);
        Task GuardarCambiosAsync();
    }
}