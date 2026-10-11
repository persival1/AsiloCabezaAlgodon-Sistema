using System;
using System.Collections.Generic;
using System.Text;

namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public Rol Rol { get; set; } = null!;
        public string Estado { get; set; } = "activo";
        public string? Especialidad { get; set; } // solo aplica a rol Especialista
    }
}