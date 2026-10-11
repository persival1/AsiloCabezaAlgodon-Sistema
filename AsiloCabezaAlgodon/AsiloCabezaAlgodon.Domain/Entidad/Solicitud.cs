namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Solicitud
    {
        public int IdSolicitud { get; set; }
        public string Motivo { get; set; } = string.Empty;

        // Lo indica el médico general al crear la solicitud: a qué especialidad
        // considera que debe remitirse al paciente (la fundación asigna después
        // un especialista concreto, idealmente de esa especialidad).
        public string? EspecialidadRequerida { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Asignada, Atendida, Cancelada

        public int IdInterno { get; set; }
        public Interno Interno { get; set; } = null!;

        public int IdMedicoGeneral { get; set; }
        public Usuario MedicoGeneral { get; set; } = null!;

        public int? IdEspecialista { get; set; }
        public Usuario? Especialista { get; set; }

        public int? IdEnfermero { get; set; }
        public Usuario? Enfermero { get; set; }

        public DateTime? FechaHoraCita { get; set; }
    }
}
