// AC-13 (punto 6): búsqueda, filtros por estado y fecha, y paginación.
import { describe, expect, it } from "vitest";
import { filtrar, paginar, normalizar } from "../utilidades/tabla";

const SOLICITUDES = [
  { nombreInterno: "José Pérez", motivo: "Dolor de rodilla", estado: "Pendiente", fecha: "2026-11-01T10:00:00" },
  { nombreInterno: "María López", motivo: "Control de presión", estado: "Asignada", fecha: "2026-11-05T09:00:00" },
  { nombreInterno: "Ana Ruiz", motivo: "Revisión de cadera", estado: "Atendida", fecha: "2026-10-20T15:00:00" },
];
const OPCIONES = { campos: ["nombreInterno", "motivo"], estado: (s) => s.estado, fecha: (s) => s.fecha };
const SIN = { texto: "", estado: "", desde: "", hasta: "" };

describe("filtrar (PU-FE-TAB)", () => {
  it("busca sin importar mayúsculas ni tildes", () => {
    expect(normalizar("  JOSÉ ")).toBe("jose");
    expect(filtrar(SOLICITUDES, { ...SIN, texto: "jose" }, OPCIONES).map((s) => s.nombreInterno)).toEqual(["José Pérez"]);
  });

  it("busca también en el motivo", () => {
    expect(filtrar(SOLICITUDES, { ...SIN, texto: "presion" }, OPCIONES)).toHaveLength(1);
  });

  it("filtra por estado", () => {
    expect(filtrar(SOLICITUDES, { ...SIN, estado: "Asignada" }, OPCIONES).map((s) => s.nombreInterno)).toEqual(["María López"]);
  });

  it("filtra por rango de fechas, incluyendo los extremos", () => {
    const r = filtrar(SOLICITUDES, { ...SIN, desde: "2026-11-01", hasta: "2026-11-05" }, OPCIONES);
    expect(r.map((s) => s.nombreInterno)).toEqual(["José Pérez", "María López"]);
  });

  it("combina los filtros", () => {
    expect(filtrar(SOLICITUDES, { ...SIN, texto: "ana", estado: "Pendiente" }, OPCIONES)).toHaveLength(0);
  });

  it("sin filtros devuelve todo", () => {
    expect(filtrar(SOLICITUDES, SIN, OPCIONES)).toHaveLength(3);
  });
});

describe("paginar", () => {
  const filas = Array.from({ length: 23 }, (_, i) => i + 1);

  it("parte en páginas de 10", () => {
    const p = paginar(filas, 3);
    expect(p.totalPaginas).toBe(3);
    expect(p.filas).toEqual([21, 22, 23]);
  });

  it("no se sale de las páginas que existen", () => {
    expect(paginar(filas, 9).pagina).toBe(3);
    expect(paginar(filas, 0).pagina).toBe(1);
    expect(paginar([], 1)).toEqual({ filas: [], pagina: 1, totalPaginas: 1 });
  });
});
