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
    }
}
