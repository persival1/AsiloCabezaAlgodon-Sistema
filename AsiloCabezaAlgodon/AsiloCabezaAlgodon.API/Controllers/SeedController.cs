using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly BDConexion _context;

        public SeedController(BDConexion context)
        {
            _context = context;
        }

        [HttpPost("admin")]
        public async Task<IActionResult> SembrarAdmin()
        {
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
            var roles = await _context.Roles.ToListAsync();

            if (!roles.Any())
                return BadRequest("Primero ejecutá /api/Seed/admin para crear los roles.");

            var usuariosDePrueba = new[]
            {
                new { Usuario = "medicogeneral", Nombre = "Dr. Médico General", Rol = "MedicoGeneral", Especialidad = (string?)null },
                new { Usuario = "especialista", Nombre = "Dr. Especialista", Rol = "Especialista", Especialidad = (string?)"Geriatría" },
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