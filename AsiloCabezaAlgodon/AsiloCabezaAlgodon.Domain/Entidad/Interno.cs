namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Interno
    {
        public int IdInterno { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string MotivoIngreso { get; set; } = string.Empty;
        public string Estado { get; set; } = "activo";

        public Familiar? Familiar { get; set; }
        public ICollection<Psicopatologia> Psicopatologias { get; set; } = new List<Psicopatologia>();
        public ICollection<MedicamentoCajon> MedicamentosCajon { get; set; } = new List<MedicamentoCajon>();
    }
}