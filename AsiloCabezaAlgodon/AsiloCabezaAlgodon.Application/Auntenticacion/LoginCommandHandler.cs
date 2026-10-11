using System;
using System.Collections.Generic;
using System.Text;

using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Autenticacion.Comandos
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher _passwordHasher;

        public LoginCommandHandler(
            IUsuarioRepository usuarioRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher)
        {
            _usuarioRepository = usuarioRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(request.NombreUsuario);

            if (usuario == null || usuario.Estado != "activo")
                return new LoginResult(false, null, "Usuario o contraseña incorrectos.", null);

            if (!_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
                return new LoginResult(false, null, "Usuario o contraseña incorrectos.", null);

            var token = _jwtTokenService.GenerarToken(usuario);

            return new LoginResult(true, token, "Login exitoso.", usuario.Rol.Nombre);
        }
    }
}