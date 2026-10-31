using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.API.Controllers
{
    // CORRECCIÓN (observación SQA #2 - SeedController):
    // Este controlador crea los roles y los usuarios de prueba (con contraseñas conocidas
    // como "admin123"). No tiene [Authorize] a propósito, porque se usa para crear el PRIMER
    // usuario, cuando todavía no existe nadie que pueda iniciar sesión.
    // El riesgo era que en un servidor real quedara abierto a cualquiera. Ahora solo
    // responde cuando la aplicación corre en el entorno "Development" (el que usa
    // `dotnet run --launch-profile https`). En cualquier otro entorno devuelve 404, como
    // si el endpoint no existiera.
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly BDConexion _context;
        private readonly IWebHostEnvironment _entorno;

        public SeedController(BDConexion context, IWebHostEnvironment entorno)
        {
            _context = context;
            _entorno = entorno;
        }

        // true solo si estamos en Development. Se revisa al inicio de cada acción.
        private bool EsDesarrollo => _entorno.IsDevelopment();

        [HttpPost("admin")]
        public async Task<IActionResult> SembrarAdmin()
        {
            if (!EsDesarrollo)
                return NotFound();

            if (_context.Usuarios.Any())
                return BadRequest("Ya existen usuarios.");

            var rolAdmin = new Rol { Nombre = "Administrador" };
            _context.Roles.AddRange(
                rolAdmin,
                new Rol { Nombre = "MedicoGeneral" },
                new Rol { Nombre = "Especialista" },
                new Rol { Nombre = "Enfermero" },
                new Rol { Nombre = "Laboratorio" },
                new Rol { Nombre = "Farmacia" }
            );
            await _context.SaveChangesAsync();

            var usuario = new Usuario
            {
                NombreUsuario = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                NombreCompleto = "Administrador del Sistema",
                Correo = "admin@asilo.gt",
                IdRol = rolAdmin.IdRol,
                Estado = "activo"
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario admin creado.");
        }

        [HttpPost("usuarios-prueba")]
        public async Task<IActionResult> SembrarUsuariosPrueba()
        {
            if (!EsDesarrollo)
                return NotFound();

            var roles = await _context.Roles.ToListAsync();

            if (!roles.Any())
                return BadRequest("Primero ejecutá /api/Seed/admin para crear los roles.");

            var usuariosDePrueba = new[]
            {
                new { Usuario = "medicogeneral", Nombre = "Dr. Médico General", Rol = "MedicoGeneral", Especialidad = (string?)null },
                new { Usuario = "especialista", Nombre = "Dr. Especialista", Rol = "Especialista", Especialidad = (string?)"Geriatría" },
                // Segundo especialista: hace falta para la prueba AUT-08 del plan de SQA
                // (un especialista intenta registrar la visita de una solicitud de otro).
                new { Usuario = "especialista2", Nombre = "Dra. Segunda Especialista", Rol = "Especialista", Especialidad = (string?)"Cardiología" },
                new { Usuario = "enfermero", Nombre = "Enfermero de Turno", Rol = "Enfermero", Especialidad = (string?)null },
                new { Usuario = "laboratorio", Nombre = "Técnico de Laboratorio", Rol = "Laboratorio", Especialidad = (string?)null },
                new { Usuario = "farmacia", Nombre = "Encargado de Farmacia", Rol = "Farmacia", Especialidad = (string?)null }
            };

            foreach (var u in usuariosDePrueba)
            {
                var existente = await _context.Usuarios.FirstOrDefaultAsync(x => x.NombreUsuario == u.Usuario);

                if (existente != null)
                {
                    existente.Especialidad = u.Especialidad;
                    continue;
                }

                var rol = roles.First(r => r.Nombre == u.Rol);

                _context.Usuarios.Add(new Usuario
                {
                    NombreUsuario = u.Usuario,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("prueba123"),
                    NombreCompleto = u.Nombre,
                    Correo = $"{u.Usuario}@asilo.gt",
                    IdRol = rol.IdRol,
                    Estado = "activo",
                    Especialidad = u.Especialidad
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Usuarios de prueba creados/actualizados.");
        }
    }
}