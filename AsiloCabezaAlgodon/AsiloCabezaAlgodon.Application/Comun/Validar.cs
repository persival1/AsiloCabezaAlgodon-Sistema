using System.Net.Mail;

namespace AsiloCabezaAlgodon.Application.Comun
{
    // Reglas de validación que usan varios handlers. Si un dato no cumple, se lanza
    // DatosInvalidosException con un mensaje que se puede mostrar al usuario.
    //
    // Reglas de montos (propuesta del plan de SQA, apartados 8.1 y 8.2, por confirmar con el analista):
    //  - Nunca negativos.
    //  - Máximo 99,999,999.99: es lo que cabe en las columnas decimal(10,2) de la base.
    //  - Máximo dos decimales (la base redondearía en silencio Q10.555 a Q10.56).
    //  - Cobros, donaciones y gastos deben ser mayores que cero. Los costos médicos (consulta,
    //    examen, medicamento) aceptan Q0.00 para registrar algo sin costo, por ejemplo una
    //    medicina donada; un costo de cero no se carga a la cuenta del familiar.
    //
    // "campo" se escribe con su artículo ("El monto", "La cuota mensual") para que el mensaje
    // se lea bien en español.
    public static class Validar
    {
        public const decimal MontoMaximo = 99_999_999.99m;

        public static void Monto(decimal monto, string campo, bool permiteCero = false)
        {
            if (monto < 0)
                throw new DatosInvalidosException($"{campo} no puede ser menor que cero.");

            if (monto == 0 && !permiteCero)
                throw new DatosInvalidosException($"{campo} debe ser mayor que cero.");

            if (monto > MontoMaximo)
                throw new DatosInvalidosException($"{campo} no puede ser mayor que Q{MontoMaximo:N2}.");

            if (decimal.Round(monto, 2) != monto)
                throw new DatosInvalidosException($"{campo} solo puede tener dos decimales.");
        }

        public static void Texto(string? valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new DatosInvalidosException($"El campo «{campo}» es obligatorio.");
        }

        public static void Correo(string? valor, string campo)
        {
            Texto(valor, campo);
            if (!MailAddress.TryCreate(valor, out _))
                throw new DatosInvalidosException($"El campo «{campo}» no es un correo válido.");
        }

        // AC-05 y AC-08: contraseña de al menos 8 caracteres, con letras y números.
        public static void Clave(string? clave)
        {
            if (string.IsNullOrWhiteSpace(clave) || clave.Length < 8)
                throw new DatosInvalidosException("La contraseña debe tener al menos 8 caracteres.");
            if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
                throw new DatosInvalidosException("La contraseña debe tener letras y números.");
        }

        public static void FechaNoFutura(DateTime fecha, string campo)
        {
            if (fecha.Date > DateTime.Today)
                throw new DatosInvalidosException($"{campo} no puede ser una fecha futura.");
        }
    }
}
