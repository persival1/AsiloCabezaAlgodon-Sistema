using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Internos.Comandos
{
    // AC-08, CU-009 y CU-010: corregir o quitar una psicopatología o una medicina de cajón
    // registrada por error. El registro debe pertenecer al interno de la ruta: así no se puede
    // tocar la ficha de otro interno cambiando solo el número del registro.

    public record EditarPsicopatologiaCommand(int IdInterno, int IdPsicopatologia, string Nombre, DateTime FechaDiagnostico) : IRequest<bool>;
    public record EliminarPsicopatologiaCommand(int IdInterno, int IdPsicopatologia) : IRequest<bool>;
    public record EditarMedicamentoCajonCommand(int IdInterno, int IdMedicamentoCajon, string Nombre, string Dosis, string Frecuencia) : IRequest<bool>;
    public record EliminarMedicamentoCajonCommand(int IdInterno, int IdMedicamentoCajon) : IRequest<bool>;

    public class CorregirFichaCommandHandler :
        IRequestHandler<EditarPsicopatologiaCommand, bool>,
        IRequestHandler<EliminarPsicopatologiaCommand, bool>,
        IRequestHandler<EditarMedicamentoCajonCommand, bool>,
        IRequestHandler<EliminarMedicamentoCajonCommand, bool>
    {
        private readonly IInternoRepository _repo;

        public CorregirFichaCommandHandler(IInternoRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(EditarPsicopatologiaCommand request, CancellationToken cancellationToken)
        {
            Validar.Texto(request.Nombre, "nombre");
            Validar.FechaNoFutura(request.FechaDiagnostico, "La fecha de diagnóstico");
            var p = await _repo.ObtenerPsicopatologiaAsync(request.IdPsicopatologia);
            if (p == null || p.IdInterno != request.IdInterno) return false;
            p.Nombre = request.Nombre.Trim();
            p.FechaDiagnostico = request.FechaDiagnostico;
            await _repo.GuardarCambiosAsync();
            return true;
        }

        public async Task<bool> Handle(EliminarPsicopatologiaCommand request, CancellationToken cancellationToken)
        {
            var p = await _repo.ObtenerPsicopatologiaAsync(request.IdPsicopatologia);
            if (p == null || p.IdInterno != request.IdInterno) return false;
            await _repo.EliminarPsicopatologiaAsync(p);
            return true;
        }

        public async Task<bool> Handle(EditarMedicamentoCajonCommand request, CancellationToken cancellationToken)
        {
            Validar.Texto(request.Nombre, "nombre");
            Validar.Texto(request.Dosis, "dosis");
            Validar.Texto(request.Frecuencia, "frecuencia");
            var m = await _repo.ObtenerMedicamentoCajonAsync(request.IdMedicamentoCajon);
            if (m == null || m.IdInterno != request.IdInterno) return false;
            m.Nombre = request.Nombre.Trim();
            m.Dosis = request.Dosis.Trim();
            m.Frecuencia = request.Frecuencia.Trim();
            await _repo.GuardarCambiosAsync();
            return true;
        }

        public async Task<bool> Handle(EliminarMedicamentoCajonCommand request, CancellationToken cancellationToken)
        {
            var m = await _repo.ObtenerMedicamentoCajonAsync(request.IdMedicamentoCajon);
            if (m == null || m.IdInterno != request.IdInterno) return false;
            await _repo.EliminarMedicamentoCajonAsync(m);
            return true;
        }
    }
}
