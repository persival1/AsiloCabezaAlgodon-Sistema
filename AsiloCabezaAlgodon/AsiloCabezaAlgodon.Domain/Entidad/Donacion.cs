namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Donacion
    {
        public int IdDonacion { get; set; }
        public string TipoDonante { get; set; } = string.Empty; // EmpresaNacional, EmpresaInternacional, Gobierno, Particular
        public string NombreDonante { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Descripcion { get; set; } = string.Empty;
    }
}
