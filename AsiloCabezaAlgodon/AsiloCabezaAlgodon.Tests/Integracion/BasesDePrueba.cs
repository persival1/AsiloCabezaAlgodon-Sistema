using Microsoft.Data.SqlClient;

namespace AsiloCabezaAlgodon.Tests.Integracion
{
    // Cadena de conexión para las pruebas de integración. Cada clase de pruebas usa su propia
    // base (nunca la del sistema), que se borra y se vuelve a crear. Por defecto es SQL Express
    // local; para otra instancia se define la variable de entorno ASILO_BD_PRUEBAS.
    public static class BasesDePrueba
    {
        public static string Conexion(string nombreBase)
        {
            var servidor = Environment.GetEnvironmentVariable("ASILO_BD_PRUEBAS")
                ?? @"Server=localhost\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;";
            return new SqlConnectionStringBuilder(servidor) { InitialCatalog = nombreBase }.ConnectionString;
        }
    }
}
