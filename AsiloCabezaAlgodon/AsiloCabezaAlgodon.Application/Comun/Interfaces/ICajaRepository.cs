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

        // AC-01: lo que falta pagar de una visita = sus cargos (consulta, exámenes y
        // medicamentos con costo) menos los cobros ya ligados a ella. null si no existe.
        Task<decimal?> PendienteDeVisitaAsync(int idVisitaMedica);

        // AC-07: pagos del asilo a la fundación y total de cargos médicos (lo que se le debe)
        Task<List<PagoFundacion>> ObtenerPagosFundacionAsync();
        Task<PagoFundacion> CrearPagoFundacionAsync(PagoFundacion pago);
        Task<decimal> TotalCargosMedicosAsync();
    }
}
