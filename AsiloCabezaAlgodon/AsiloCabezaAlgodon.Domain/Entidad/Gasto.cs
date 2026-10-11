namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Gasto
    {
        public int IdGasto { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
