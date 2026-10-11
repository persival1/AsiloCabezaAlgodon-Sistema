namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Cobro
    {
        public int IdCobro { get; set; }
        public string Concepto { get; set; } = string.Empty; // ej. "Cuota mensual", "Cargo médico"
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int IdFamiliar { get; set; }
        public Familiar Familiar { get; set; } = null!;

        // Solo se llena cuando el Concepto es un cargo médico de una visita puntual;
        // una cuota mensual, por ejemplo, no está ligada a ninguna visita.
        public int? IdVisitaMedica { get; set; }
        public VisitaMedica? VisitaMedica { get; set; }
    }
}
