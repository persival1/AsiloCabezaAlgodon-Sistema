namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Familiar
    {
        public int IdFamiliar { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Parentesco { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public decimal CuotaMensual { get; set; }
        public decimal SaldoPendiente { get; set; } = 0;

        public int IdInterno { get; set; }
        public Interno Interno { get; set; } = null!;
    }
}