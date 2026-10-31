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

    // Semana 3: el usuario tiene el rol correcto, pero el registro no le pertenece:
    // por ejemplo, un especialista que intenta registrar la visita de una solicitud
    // asignada a otro especialista. La API responde 403 (Forbidden).
    public class AccesoDenegadoException : Exception
    {
        public AccesoDenegadoException(string mensaje) : base(mensaje) { }
    }
}
