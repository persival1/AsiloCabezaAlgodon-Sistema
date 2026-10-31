using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface ICajaRepository
    {
        Task<List<Donacion>> ObtenerDonacionesAsync();
        Task<Donacion> CrearDonacionAsync(Donacion donacion);

        Task<List<Gasto>> ObtenerGastosAsync();
        Task<Gasto> CrearGastoAsync(Gasto gasto);

        Task<List<Cobro>> ObtenerCobrosAsync();
        Task<Cobro> CrearCobroAsync(Cobro cobro);

        Task<bool> ExisteFamiliarAsync(int idFamiliar);

        // true si la visita médica es de un interno cuyo familiar responsable es idFamiliar
        Task<bool> VisitaEsDelFamiliarAsync(int idVisitaMedica, int idFamiliar);
    }
}
