namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Prescripcion
    {
        public int IdPrescripcion { get; set; }
        public string NombreMedicamento { get; set; } = string.Empty;
        public string Dosis { get; set; } = string.Empty;
        public string Indicaciones { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Despachado
        public decimal? Costo { get; set; }
        public DateTime? FechaDespacho { get; set; }

        public int IdVisitaMedica { get; set; }
        public VisitaMedica VisitaMedica { get; set; } = null!;
    }
}
