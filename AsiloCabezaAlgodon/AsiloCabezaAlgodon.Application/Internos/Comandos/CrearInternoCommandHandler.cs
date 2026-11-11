using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
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
            // Validaciones en el servidor (plan de pruebas SQA, casos RES-02 y RES-03).
            // El formulario de React ya exige estos campos, pero alguien podría llamar a la
            // API directamente (por ejemplo con Postman) y saltarse el formulario.
            Validar.Texto(request.Nombres, "nombres");
            Validar.Texto(request.Apellidos, "apellidos");
            Validar.Texto(request.MotivoIngreso, "motivo de ingreso");
            Validar.Texto(request.NombreFamiliar, "nombre del familiar");
            Validar.Texto(request.ApellidoFamiliar, "apellido del familiar");
            Validar.Texto(request.Parentesco, "parentesco");
            Validar.Texto(request.TelefonoFamiliar, "teléfono del familiar");
            Validar.Correo(request.CorreoFamiliar, "correo del familiar");
            Validar.Monto(request.CuotaMensual, "La cuota mensual", permiteCero: true);
            Validar.FechaNoFutura(request.FechaNacimiento, "La fecha de nacimiento");

            if (request.FechaIngreso.Date < request.FechaNacimiento.Date)
                throw new DatosInvalidosException("La fecha de ingreso no puede ser anterior a la fecha de nacimiento.");

            // AC-08: no se registra dos veces a la misma persona
            if (await _internoRepository.ExisteDuplicadoAsync(request.Nombres, request.Apellidos, request.FechaNacimiento))
                throw new ConflictoException("Ya existe un interno con esos nombres, apellidos y fecha de nacimiento.");

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
