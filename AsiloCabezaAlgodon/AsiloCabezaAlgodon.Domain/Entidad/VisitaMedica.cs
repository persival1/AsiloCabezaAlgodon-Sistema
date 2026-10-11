namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class VisitaMedica
    {
        public int IdVisitaMedica { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaVisita { get; set; } = DateTime.Now;
        public decimal CostoConsulta { get; set; }

        public int IdSolicitud { get; set; }
        public Solicitud Solicitud { get; set; } = null!;

        public ICollection<ExamenSolicitado> ExamenesSolicitados { get; set; } = new List<ExamenSolicitado>();
        public ICollection<Prescripcion> Prescripciones { get; set; } = new List<Prescripcion>();
    }
}
