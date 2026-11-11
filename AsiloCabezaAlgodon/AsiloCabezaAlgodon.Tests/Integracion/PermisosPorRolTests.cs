using System.Net.Http.Headers;
using System.Text;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AsiloCabezaAlgodon.Tests.Integracion
{
    // Levanta la API completa en memoria (los mismos controllers, [Authorize] y middleware de
    // Program.cs) sobre la base de pruebas "asilo_pruebas_permisos", con un usuario por rol y el
    // interno 1. Los tokens se generan con el mismo JwtTokenService de la API, así no se pasa
    // por el login (ni por su límite de intentos).
    public class ApiDePruebas : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private static readonly string Conexion = BasesDePrueba.Conexion("asilo_pruebas_permisos");

        public Dictionary<string, string> Tokens { get; } = new();

        public ApiDePruebas()
        {
            // La API lee la cadena de conexión al arrancar; con esta variable usa la base de pruebas.
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", Conexion);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development"); // usa la llave JWT de desarrollo
        }

        public async Task InitializeAsync()
        {
            await using var ctx = new BDConexion(new DbContextOptionsBuilder<BDConexion>().UseSqlServer(Conexion).Options);
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();

            var usuarios = PermisosPorRolTests.Roles
                .Select(rol => new Usuario { NombreUsuario = rol.ToLower(), NombreCompleto = $"Usuario {rol}", Rol = new Rol { Nombre = rol } })
                .ToList();
            ctx.Usuarios.AddRange(usuarios);
            ctx.Internos.Add(new Interno
            {
                Nombres = "Interno", Apellidos = "De Prueba", MotivoIngreso = "Prueba",
                FechaNacimiento = new DateTime(1940, 1, 1), FechaIngreso = new DateTime(2020, 1, 1),
                Familiar = new Familiar { Nombres = "Familiar", Apellidos = "De Prueba", Parentesco = "Hijo", Telefono = "1", Correo = "familiar@prueba.test" }
            });
            await ctx.SaveChangesAsync();

            using var alcance = Services.CreateScope();
            var jwt = alcance.ServiceProvider.GetRequiredService<IJwtTokenService>();
            foreach (var usuario in usuarios)
                Tokens[usuario.Rol.Nombre] = jwt.GenerarToken(usuario);
        }

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
    }

    // Fase 3 del plan de automatización de SQA: la matriz de permisos del script
    // pruebas/probar-permisos.ps1 convertida en una prueba parametrizada (155 casos desde AC-07).
    //   200 = permitido   401 = sin sesión   403 = hay sesión pero el rol no tiene permiso
    // Igual que el script, las escrituras solo se prueban con los roles que NO pueden usarlas,
    // para no crear datos. Necesita SQL Server: en GitHub Actions se excluye (Tipo!=Integracion).
    [Trait("Tipo", "Integracion")]
    public class PermisosPorRolTests : IClassFixture<ApiDePruebas>
    {
        public static readonly string[] Roles = { "Administrador", "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" };

        private static readonly (string Ruta, string[] Permitidos)[] Lecturas =
        {
            ("/Internos", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Internos/1", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Solicitudes", new[] { "Administrador", "MedicoGeneral", "Especialista", "Enfermero" }),
            ("/VisitasMedicas", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Examenes", new[] { "Administrador", "Laboratorio" }),
            ("/Prescripciones", new[] { "Administrador", "Farmacia" }),
            ("/Donaciones", new[] { "Administrador" }),
            ("/Gastos", new[] { "Administrador" }),
            ("/Cobros", new[] { "Administrador" }),
            ("/PagosFundacion", new[] { "Administrador" }),
            ("/Usuarios", new[] { "Administrador" }),
            ("/Reportes/costos-por-cita", new[] { "Administrador" }),
            ("/Reportes/entradas", new[] { "Administrador" }),
            ("/Reportes/ficha-medica/1", new[] { "Administrador" }),
        };

        private static readonly (string Metodo, string Ruta, string[] Prohibidos)[] Escrituras =
        {
            ("POST", "/Internos", new[] { "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("POST", "/Solicitudes", new[] { "Administrador", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("PUT", "/Solicitudes/1/asignar", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("POST", "/VisitasMedicas", new[] { "Administrador", "MedicoGeneral", "Enfermero", "Laboratorio", "Farmacia" }),
            ("PUT", "/Examenes/1/resultado", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Farmacia" }),
            ("PUT", "/Prescripciones/1/despachar", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio" }),
            ("POST", "/Donaciones", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("POST", "/Gastos", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("POST", "/Cobros", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
            ("POST", "/PagosFundacion", new[] { "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia" }),
        };

        public static IEnumerable<object[]> Casos()
        {
            foreach (var (ruta, permitidos) in Lecturas)
            {
                foreach (var rol in Roles)
                    yield return new object[] { "GET", ruta, rol, permitidos.Contains(rol) ? 200 : 403 };
                yield return new object[] { "GET", ruta, "SinSesion", 401 };
            }
            foreach (var (metodo, ruta, prohibidos) in Escrituras)
            {
                foreach (var rol in prohibidos)
                    yield return new object[] { metodo, ruta, rol, 403 };
                yield return new object[] { metodo, ruta, "SinSesion", 401 };
            }
        }

        private readonly ApiDePruebas _api;

        public PermisosPorRolTests(ApiDePruebas api)
        {
            _api = api;
        }

        [Fact]
        public void La_matriz_tiene_los_mismos_155_casos_que_el_script()
        {
            Assert.Equal(155, Casos().Count());
        }

        [Theory]
        [MemberData(nameof(Casos))]
        public async Task Cada_rol_recibe_el_codigo_esperado(string metodo, string ruta, string actor, int esperado)
        {
            var peticion = new HttpRequestMessage(new HttpMethod(metodo), "/api" + ruta);
            if (actor != "SinSesion")
                peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _api.Tokens[actor]);
            if (metodo != "GET")
                peticion.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var respuesta = await _api.CreateClient().SendAsync(peticion);

            Assert.Equal(esperado, (int)respuesta.StatusCode);
        }
    }
}
