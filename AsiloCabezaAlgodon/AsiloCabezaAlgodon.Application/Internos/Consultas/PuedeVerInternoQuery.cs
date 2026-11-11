using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    // AC-04 y AC-10 (mínimo privilegio, acceso horizontal): quién puede ver la ficha de un interno.
    //   Administrador y Médico general: todos los internos (registran internos y los refieren).
    //   Especialista: solo los internos de las solicitudes que la fundación le asignó.
    //   Los demás roles: ninguno (ya lo bloquea el [Authorize] de cada endpoint).
    public record PuedeVerInternoQuery(int IdInterno, int IdUsuario, string Rol) : IRequest<bool>;

    public class PuedeVerInternoQueryHandler : IRequestHandler<PuedeVerInternoQuery, bool>
    {
        private readonly ISolicitudRepository _solicitudRepository;

        public PuedeVerInternoQueryHandler(ISolicitudRepository solicitudRepository)
        {
            _solicitudRepository = solicitudRepository;
        }

        public async Task<bool> Handle(PuedeVerInternoQuery request, CancellationToken cancellationToken)
        {
            if (request.Rol is "Administrador" or "MedicoGeneral") return true;
            if (request.Rol != "Especialista") return false;
            return (await _solicitudRepository.InternosDeEspecialistaAsync(request.IdUsuario)).Contains(request.IdInterno);
        }
    }
}
