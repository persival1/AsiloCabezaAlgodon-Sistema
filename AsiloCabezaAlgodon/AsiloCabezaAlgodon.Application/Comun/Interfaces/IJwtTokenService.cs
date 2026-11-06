using System;
using System.Collections.Generic;
using System.Text;

using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Comun.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerarToken(Usuario usuario);
    }
}