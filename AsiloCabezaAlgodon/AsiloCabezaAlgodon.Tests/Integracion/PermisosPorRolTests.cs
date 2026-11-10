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
            // Interno 1: tiene una solicitud asignada al especialista (es "su" paciente).
            // Interno 2: no tiene ninguna; sirve para probar el acceso horizontal (AC-04).
            var propio = new Interno
            {
                Nombres = "Interno", Apellidos = "De Prueba", MotivoIngreso = "Prueba",
                FechaNacimiento = new DateTime(1940, 1, 1), FechaIngreso = new DateTime(2020, 1, 1),
                Familiar = new Familiar { Nombres = "Familiar", Apellidos = "De Prueba", Parentesco = "Hijo", Telefono = "1", Correo = "familiar@prueba.test", CuotaMensual = 500 }
            };
            var ajeno = new Interno
            {
                Nombres = "Otro", Apellidos = "Interno", MotivoIngreso = "Prueba",
                FechaNacimiento = new DateTime(1942, 1, 1), FechaIngreso = new DateTime(2021, 1, 1),
                Familiar = new Familiar { Nombres = "Otro", Apellidos = "Familiar", Parentesco = "Hija", Telefono = "2", Correo = "otro@prueba.test" }
            };
            ctx.Internos.AddRange(propio, ajeno);
            var solicitud = new Solicitud
            {
                Motivo = "Control", Estado = "Atendida", Interno = propio,
                MedicoGeneral = usuarios.First(u => u.Rol.Nombre == "MedicoGeneral"),
                Especialista = usuarios.First(u => u.Rol.Nombre == "Especialista")
            };
            ctx.Solicitudes.Add(solicitud);
            // Una visita con cargo de Q150, para probar que el estado de pago solo lo ve el Administrador.
            ctx.VisitasMedicas.Add(new VisitaMedica { Solicitud = solicitud, Diagnostico = "Control", CostoConsulta = 150 });
            await ctx.SaveChangesAsync();

            using var alcance = Services.CreateScope();
            var jwt = alcance.ServiceProvider.GetRequiredService<IJwtTokenService>();
            foreach (var usuario in usuarios)
                Tokens[usuario.Rol.Nombre] = jwt.GenerarToken(usuario);
        }

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
    }

    // Fase 3 del plan de automatización de SQA: la matriz de permisos del script
    // pruebas/probar-permisos.ps1 convertida en una prueba parametrizada (218 casos desde AC-13).
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
            ("/Internos/2", new[] { "Administrador", "MedicoGeneral" }), // AC-04: no es paciente del especialista
            ("/Solicitudes", new[] { "Administrador", "MedicoGeneral", "Especialista", "Enfermero" }),
            ("/VisitasMedicas", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/VisitasMedicas?idInterno=1", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/VisitasMedicas?idInterno=2", new[] { "Administrador", "MedicoGeneral" }), // AC-04: historial de un interno ajeno
            ("/Examenes", new[] { "Administrador", "Laboratorio" }),
            ("/Prescripciones", new[] { "Administrador", "Farmacia" }),
            ("/Donaciones", new[] { "Administrador" }),
            ("/Gastos", new[] { "Administrador" }),
            ("/Cobros", new[] { "Administrador" }),
            ("/PagosFundacion", new[] { "Administrador" }),
            ("/Usuarios", new[] { "Administrador" }),
            ("/Reportes/costos-por-cita", new[] { "Administrador" }),
            ("/Reportes/entradas", new[] { "Administrador" }),
            // AC-10: informes clínicos para los médicos; financieros solo para el Administrador
            ("/Reportes/ficha-medica/1", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Reportes/ficha-medica/2", new[] { "Administrador", "MedicoGeneral" }),
            ("/Reportes/examenes-por-paciente/1", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Reportes/medicamentos-por-paciente/1", new[] { "Administrador", "MedicoGeneral", "Especialista" }),
            ("/Reportes/pagos-fundacion", new[] { "Administrador" }),
            ("/Reportes/conciliacion-saldos", new[] { "Administrador" }),
            ("/Reportes/cobros-por-paciente?idInterno=1&fechaInicio=2026-01-01&fechaFin=2026-12-31", new[] { "Administrador" }),
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
        public void La_matriz_tiene_los_mismos_218_casos_que_el_script()
        {
            Assert.Equal(218, Casos().Count());
        }

        [Theory] // AC-04: lo pagado de cada visita solo lo ve el Administrador
        [InlineData("Administrador", true)]
        [InlineData("MedicoGeneral", false)]
        [InlineData("Especialista", false)]
        public async Task El_estado_de_pago_de_las_visitas_solo_lo_ve_el_administrador(string rol, bool vePago)
        {
            var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/VisitasMedicas?idInterno=1");
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _api.Tokens[rol]);
            var json = await (await _api.CreateClient().SendAsync(peticion)).Content.ReadAsStringAsync();
            Assert.Contains("\"totalCargos\":150", json);
            Assert.Equal(vePago, json.Contains("\"estadoPago\":\"PendientePago\""));
            Assert.Equal(!vePago, json.Contains("\"estadoPago\":null"));
        }

        [Theory] // AC-04: la cuota y el saldo del familiar solo los ve el Administrador
        [InlineData("Administrador", true)]
        [InlineData("MedicoGeneral", false)]
        [InlineData("Especialista", false)]
        public async Task La_ficha_solo_muestra_finanzas_al_administrador(string rol, bool veFinanzas)
        {
            var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/Internos/1");
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _api.Tokens[rol]);
            var json = await (await _api.CreateClient().SendAsync(peticion)).Content.ReadAsStringAsync();
            Assert.Equal(veFinanzas, json.Contains("\"cuotaMensual\":500"));
            Assert.Equal(!veFinanzas, json.Contains("\"cuotaMensual\":null"));
        }

        [Fact] // AC-04: el especialista solo ve en la lista a sus pacientes
        public async Task El_especialista_solo_lista_a_sus_pacientes()
        {
            var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/Internos");
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _api.Tokens["Especialista"]);
            var json = await (await _api.CreateClient().SendAsync(peticion)).Content.ReadAsStringAsync();
            Assert.Contains("\"idInterno\":1", json);
            Assert.DoesNotContain("\"idInterno\":2", json);
        }

        [Fact] // AC-15: la versión se consulta sin sesión y no expone datos
        public async Task La_version_es_publica()
        {
            var respuesta = await _api.CreateClient().GetAsync("/api/Version");
            Assert.Equal(200, (int)respuesta.StatusCode);
            Assert.Contains("1.0.0-rc2", await respuesta.Content.ReadAsStringAsync());
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
