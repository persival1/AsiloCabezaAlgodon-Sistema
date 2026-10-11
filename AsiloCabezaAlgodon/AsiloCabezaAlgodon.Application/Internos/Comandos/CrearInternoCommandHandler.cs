using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    public class CrearInternoCommandHandler : IRequestHandler<CrearInternoCommand, int>
    {
        private readonly IInternoRepository _internoRepository;

        public CrearInternoCommandHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<int> Handle(CrearInternoCommand request, CancellationToken cancellationToken)
        {
            var interno = new Interno
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                FechaNacimiento = request.FechaNacimiento,
                FechaIngreso = request.FechaIngreso,
                MotivoIngreso = request.MotivoIngreso,
                Estado = "activo",
                Familiar = new Familiar
                {
                    Nombres = request.NombreFamiliar,
                    Apellidos = request.ApellidoFamiliar,
                    Parentesco = request.Parentesco,
                    Telefono = request.TelefonoFamiliar,
                    Correo = request.CorreoFamiliar,
                    CuotaMensual = request.CuotaMensual
                }
            };

            var creado = await _internoRepository.CrearAsync(interno);
            return creado.IdInterno;
        }
    }
}
