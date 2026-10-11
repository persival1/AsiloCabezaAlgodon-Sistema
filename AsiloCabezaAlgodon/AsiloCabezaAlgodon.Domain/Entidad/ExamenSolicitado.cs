namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class ExamenSolicitado
    {
        public int IdExamenSolicitado { get; set; }
        public string NombreExamen { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, ResultadoRegistrado
        public string? Resultado { get; set; }
        public DateTime? FechaResultado { get; set; }
        public decimal? Costo { get; set; }

        public int IdVisitaMedica { get; set; }
        public VisitaMedica VisitaMedica { get; set; } = null!;
    }
}
