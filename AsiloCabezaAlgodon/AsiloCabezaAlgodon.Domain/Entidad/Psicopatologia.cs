namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Psicopatologia
    {
        public int IdPsicopatologia { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaDiagnostico { get; set; }

        public int IdInterno { get; set; }
        public Interno Interno { get; set; } = null!;
    }
}