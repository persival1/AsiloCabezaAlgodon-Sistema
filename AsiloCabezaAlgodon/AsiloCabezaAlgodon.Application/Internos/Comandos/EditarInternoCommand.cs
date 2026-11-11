using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    // AC-08, CU-006: editar los datos del interno y de su familiar responsable.
    // La cuota mensual es un dato financiero: solo la cambia el Administrador (PuedeCambiarCuota);
    // si la pide otro rol, se conserva la que tenía.
    public record EditarInternoCommand(
        int IdInterno,
        string Nombres,
        string Apellidos,
        DateTime FechaNacimiento,
        DateTime FechaIngreso,
        string MotivoIngreso,
        string NombreFamiliar,
        string ApellidoFamiliar,
        string Parentesco,
        string TelefonoFamiliar,
        string CorreoFamiliar,
        decimal? CuotaMensual,
        bool PuedeCambiarCuota = false
    ) : IRequest<bool>;

    public class EditarInternoCommandHandler : IRequestHandler<EditarInternoCommand, bool>
    {
        private readonly IInternoRepository _internoRepository;

        public EditarInternoCommandHandler(IInternoRepository internoRepository)
        {
            _internoRepository = internoRepository;
        }

        public async Task<bool> Handle(EditarInternoCommand request, CancellationToken cancellationToken)
        {
            var interno = await _internoRepository.ObtenerPorIdAsync(request.IdInterno);
            if (interno == null)
                return false;

            // Mismas reglas que al registrar (RES-02 y RES-03)
            Validar.Texto(request.Nombres, "nombres");
            Validar.Texto(request.Apellidos, "apellidos");
            Validar.Texto(request.MotivoIngreso, "motivo de ingreso");
            Validar.Texto(request.NombreFamiliar, "nombre del familiar");
            Validar.Texto(request.ApellidoFamiliar, "apellido del familiar");
            Validar.Texto(request.Parentesco, "parentesco");
            Validar.Texto(request.TelefonoFamiliar, "teléfono del familiar");
            Validar.Correo(request.CorreoFamiliar, "correo del familiar");
            Validar.FechaNoFutura(request.FechaNacimiento, "La fecha de nacimiento");
            if (request.FechaIngreso.Date < request.FechaNacimiento.Date)
                throw new DatosInvalidosException("La fecha de ingreso no puede ser anterior a la fecha de nacimiento.");
            if (request.PuedeCambiarCuota && request.CuotaMensual.HasValue)
                Validar.Monto(request.CuotaMensual.Value, "La cuota mensual", permiteCero: true);

            if (await _internoRepository.ExisteDuplicadoAsync(request.Nombres, request.Apellidos, request.FechaNacimiento, request.IdInterno))
                throw new ConflictoException("Ya existe otro interno con esos nombres, apellidos y fecha de nacimiento.");

            interno.Nombres = request.Nombres.Trim();
            interno.Apellidos = request.Apellidos.Trim();
            interno.FechaNacimiento = request.FechaNacimiento;
            interno.FechaIngreso = request.FechaIngreso;
            interno.MotivoIngreso = request.MotivoIngreso.Trim();

            interno.Familiar ??= new Familiar();
            interno.Familiar.Nombres = request.NombreFamiliar.Trim();
            interno.Familiar.Apellidos = request.ApellidoFamiliar.Trim();
            interno.Familiar.Parentesco = request.Parentesco.Trim();
            interno.Familiar.Telefono = request.TelefonoFamiliar.Trim();
            interno.Familiar.Correo = request.CorreoFamiliar.Trim();
            if (request.PuedeCambiarCuota && request.CuotaMensual.HasValue)
                interno.Familiar.CuotaMensual = request.CuotaMensual.Value;

            await _internoRepository.GuardarCambiosAsync();
            return true;
        }
    }

    // AC-08, CU-007: alta o baja del interno. La baja no borra nada (la ficha, las visitas y los
    // cobros quedan para los informes); solo impide crear solicitudes nuevas. No se puede dar de
    // baja a un interno con solicitudes pendientes o citas asignadas: primero se cancelan.
    public record CambiarEstadoInternoCommand(int IdInterno, string Estado) : IRequest<bool>;

    public class CambiarEstadoInternoCommandHandler : IRequestHandler<CambiarEstadoInternoCommand, bool>
    {
        public static readonly string[] Estados = { "activo", "baja" };

        private readonly IInternoRepository _internoRepository;
        private readonly ISolicitudRepository _solicitudRepository;

        public CambiarEstadoInternoCommandHandler(IInternoRepository internoRepository, ISolicitudRepository solicitudRepository)
        {
            _internoRepository = internoRepository;
            _solicitudRepository = solicitudRepository;
        }

        public async Task<bool> Handle(CambiarEstadoInternoCommand request, CancellationToken cancellationToken)
        {
            var estado = (request.Estado ?? "").Trim().ToLowerInvariant();
            if (!Estados.Contains(estado))
                throw new DatosInvalidosException("El estado debe ser «activo» o «baja».");

            var interno = await _internoRepository.ObtenerPorIdAsync(request.IdInterno);
            if (interno == null)
                return false;
            if (interno.Estado == estado)
                throw new ConflictoException(estado == "baja" ? "El interno ya está de baja." : "El interno ya está activo.");

            if (estado == "baja")
            {
                var abiertas = (await _solicitudRepository.ObtenerTodasAsync())
                    .Count(s => s.IdInterno == request.IdInterno && (s.Estado == "Pendiente" || s.Estado == "Asignada"));
                if (abiertas > 0)
                    throw new ConflictoException($"El interno tiene {abiertas} solicitud(es) pendiente(s) o con cita asignada. Cancelalas antes de darlo de baja.");
            }

            interno.Estado = estado;
            await _internoRepository.GuardarCambiosAsync();
            return true;
        }
    }
}
