using System;
using System.Collections.Generic;
using System.Text;

namespace AsiloCabezaAlgodon.Domain.Entidad
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}