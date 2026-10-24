using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Conciliación de saldos (respuesta a la observación SQA #4 - integridad financiera).
    //
    // El saldo del familiar (Familiar.SaldoPendiente) es un número GUARDADO que se va
    // actualizando con cada movimiento: cada costo médico lo sube y cada cobro lo baja.
    // Si algún movimiento no lo actualiza, el saldo se descuadra y nadie lo nota.
    //
    // Este reporte lo RECALCULA desde cero a partir de los movimientos reales y lo compara
    // con el valor guardado. Es de solo lectura (un Query): no cambia ningún dato.
    //
    //   saldo recalculado = costos médicos (consultas + exámenes + medicamentos) - cobros
    public record SaldoConciliacionDto(
        int IdFamiliar,
        string NombreFamiliar,
        string NombreInterno,
        decimal TotalCargos,
        decimal TotalCobros,
        decimal SaldoRecalculado,
        decimal SaldoGuardado,
        decimal Diferencia,
        bool Cuadra
    );

    public record ReporteConciliacionSaldosQuery() : IRequest<List<SaldoConciliacionDto>>;
}
