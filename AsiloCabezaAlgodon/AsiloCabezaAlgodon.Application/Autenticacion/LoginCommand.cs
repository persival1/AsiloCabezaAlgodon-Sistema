using System;
using System.Collections.Generic;
using System.Text;

using MediatR;

namespace AsiloCabezaAlgodon.Application.Autenticacion.Comandos
{
    public record LoginCommand(string NombreUsuario, string Password) : IRequest<LoginResult>;

    public record LoginResult(bool Exitoso, string? Token, string? Mensaje, string? Rol);
}