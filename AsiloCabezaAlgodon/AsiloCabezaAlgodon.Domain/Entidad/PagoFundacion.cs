namespace AsiloCabezaAlgodon.Domain.Entidad
{
    // AC-07: pago que el asilo le hace a la fundación por las consultas, exámenes y
    // medicamentos de sus internos (montos ya con el descuento de la fundación).
    public class PagoFundacion
    {
        public int IdPagoFundacion { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
        public string NumeroComprobante { get; set; } = string.Empty; // recibo o boleta de la fundación
        public string Descripcion { get; set; } = string.Empty;
    }
}
