// AC-13 (punto 2): cada indicador del inicio coincide con los registros que lo originan.
import { describe, expect, it } from "vitest";
import { indicadoresInicio, rangoDelMes, nombreMes } from "../utilidades/indicadores";

const HOY = new Date(2026, 10, 10); // 10 de noviembre de 2026
const valores = (r) => Object.fromEntries(r.tarjetas.map((t) => [t.etiqueta, t.valor]));

describe("indicadores del inicio (PU-FE-IND)", () => {
  it("el período del mes va del primer al último día", () => {
    expect(rangoDelMes(HOY)).toEqual({ desde: "2026-11-01", hasta: "2026-11-30" });
    expect(nombreMes(HOY)).toBe("noviembre de 2026");
  });

  it("Administrador: pendientes al día de hoy y caja solo del mes", () => {
    const r = indicadoresInicio("Administrador", {
      solicitudes: [
        { idSolicitud: 1, estado: "Pendiente", nombreInterno: "A", motivo: "x", fechaCreacion: "2026-11-02" },
        { idSolicitud: 2, estado: "Asignada", nombreInterno: "B", motivo: "y", fechaCreacion: "2026-11-01" },
        { idSolicitud: 3, estado: "Pendiente", nombreInterno: "C", motivo: "z", fechaCreacion: "2026-10-30" },
      ],
      visitas: [{ estadoPago: "PendientePago" }, { estadoPago: "Pagada" }, { estadoPago: "SinCargos" }],
      fundacion: { saldoPendiente: 545 },
      donaciones: [{ monto: 1000, fecha: "2026-11-03T10:00:00" }, { monto: 500, fecha: "2026-10-31T10:00:00" }],
      cobros: [{ monto: 150, fecha: "2026-11-30T23:00:00" }, { monto: 75, fecha: "2026-12-01T08:00:00" }],
    }, HOY);

    expect(valores(r)).toEqual({
      "Solicitudes por asignar": 2,
      "Visitas pendientes de pago": 1,
      "Adeudado a la fundación": "Q545.00",
      "Donaciones recibidas": "Q1000.00",
      "Cobros a familiares": "Q150.00",
    });
    expect(r.tarjetas.find((t) => t.etiqueta === "Donaciones recibidas").periodo).toBe("En noviembre de 2026");
    // la lista de pendientes empieza por la solicitud más antigua
    expect(r.pendientes.filas.map((f) => f.id)).toEqual([3, 1]);
  });

  it("cada tarjeta lleva a su módulo y dice qué requisito la respalda", () => {
    for (const rol of ["Administrador", "MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia"]) {
      const r = indicadoresInicio(rol, {
        solicitudes: [], visitas: [], donaciones: [], cobros: [], fundacion: { saldoPendiente: 0 }, examenes: [], prescripciones: [],
      }, HOY);
      expect(r.tarjetas.length).toBeGreaterThan(0);
      for (const t of r.tarjetas) {
        expect(t.ruta).toMatch(/^\/dashboard\//);
        expect(t.requisito).toMatch(/^Requisito/);
        expect(t.periodo).toBeTruthy();
      }
    }
  });

  it("Especialista: solo cuenta las citas asignadas, ordenadas por fecha", () => {
    const r = indicadoresInicio("Especialista", {
      solicitudes: [
        { idSolicitud: 1, estado: "Asignada", fechaHoraCita: "2026-11-20T10:00:00", nombreInterno: "A", motivo: "x" },
        { idSolicitud: 2, estado: "Atendida", fechaHoraCita: "2026-11-01T10:00:00", nombreInterno: "B", motivo: "y" },
        { idSolicitud: 3, estado: "Asignada", fechaHoraCita: "2026-11-12T10:00:00", nombreInterno: "C", motivo: "z" },
      ],
    }, HOY);
    expect(r.tarjetas[0].valor).toBe(2);
    expect(r.pendientes.filas.map((f) => f.id)).toEqual([3, 1]);
  });

  it("Laboratorio y Farmacia: solo lo pendiente", () => {
    const lab = indicadoresInicio("Laboratorio", {
      examenes: [{ idExamenSolicitado: 1, estado: "Pendiente", nombreExamen: "Hemograma", nombreInterno: "A", fechaVisita: "2026-11-01" },
        { idExamenSolicitado: 2, estado: "ResultadoRegistrado", nombreExamen: "Orina", nombreInterno: "B", fechaVisita: "2026-11-01" }],
    }, HOY);
    const far = indicadoresInicio("Farmacia", {
      prescripciones: [{ idPrescripcion: 1, estado: "Despachado", nombreMedicamento: "X", dosis: "1", nombreInterno: "A", fechaVisita: "2026-11-01" }],
    }, HOY);
    expect(lab.tarjetas[0].valor).toBe(1);
    expect(far.tarjetas[0].valor).toBe(0);
    expect(far.pendientes.filas).toEqual([]);
  });
});
