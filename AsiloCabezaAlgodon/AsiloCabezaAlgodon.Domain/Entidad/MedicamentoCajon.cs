namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class MedicamentoCajon
    {
        public int IdMedicamentoCajon { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Dosis { get; set; } = string.Empty;
        public string Frecuencia { get; set; } = string.Empty;

        public int IdInterno { get; set; }
        public Interno Interno { get; set; } = null!;
    }
}