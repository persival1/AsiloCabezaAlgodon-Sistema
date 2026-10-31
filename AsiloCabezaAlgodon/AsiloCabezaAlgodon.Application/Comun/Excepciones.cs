namespace AsiloCabezaAlgodon.Application.Comun
{
    // Plan de pruebas SQA, semana 2: el servidor debe rechazar lo inválido con un mensaje claro,
    // no solo el formulario de React. Los handlers lanzan una de estas dos excepciones y
    // Program.cs las convierte en la respuesta HTTP correcta (ver el middleware de errores).

    // Los datos enviados no son válidos: monto negativo, campo vacío, fecha imposible...
    // La API responde 400 (Bad Request).
    public class DatosInvalidosException : Exception
    {
        public DatosInvalidosException(string mensaje) : base(mensaje) { }
    }

    // Los datos están bien, pero la operación choca con el estado actual del registro:
    // por ejemplo, despachar una prescripción que ya fue despachada.
    // La API responde 409 (Conflict).
    public class ConflictoException : Exception
    {
        public ConflictoException(string mensaje) : base(mensaje) { }
    }
}
