// PU-FE-01 a PU-FE-05: interceptores de Servicios/api.js
// Pruebas escritas por SQA (plan de pruebas, Anexo A) e incorporadas al repositorio.
import { describe, it, expect, beforeEach } from "vitest";
import api from "../Servicios/api.js";

const peticion = api.interceptors.request.handlers[0].fulfilled;
const errorRespuesta = api.interceptors.response.handlers[0].rejected;

describe("Servicios/api.js", () => {
  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();
    Object.defineProperty(window, "location", { value: { href: "/dashboard" }, writable: true });
  });

  it("PU-FE-01: usa la URL por defecto cuando no existe VITE_API_URL", () => {
    expect(api.defaults.baseURL).toBe("https://localhost:7022/api");
  });

  it("PU-FE-02: agrega el encabezado Authorization cuando hay token", () => {
    localStorage.setItem("token", "token-de-prueba");
    const config = peticion({ headers: {} });
    expect(config.headers.Authorization).toBe("Bearer token-de-prueba");
  });

  it("PU-FE-03: ante un 401 en un endpoint protegido cierra la sesión y redirige con aviso", async () => {
    localStorage.setItem("token", "vencido");
    localStorage.setItem("rol", "Administrador");
    const error = { config: { url: "/Cobros" }, response: { status: 401 } };
    // AC-17: la promesa queda pendiente (no se rechaza) para que no haya errores sin manejar
    const resultado = await Promise.race([
      errorRespuesta(error).then(() => "resuelta", () => "rechazada"),
      new Promise((r) => setTimeout(() => r("pendiente"), 50)),
    ]);
    expect(resultado).toBe("pendiente");
    expect(localStorage.getItem("token")).toBeNull();
    expect(localStorage.getItem("rol")).toBeNull();
    expect(sessionStorage.getItem("sesionExpirada")).toBe("1");
    expect(window.location.href).toBe("/");
  });

  it("PU-FE-04: un 401 del login no cierra la sesión ni redirige", async () => {
    localStorage.setItem("token", "previo");
    const error = { config: { url: "/Auth/login" }, response: { status: 401 } };
    await expect(errorRespuesta(error)).rejects.toBe(error);
    expect(localStorage.getItem("token")).toBe("previo");
    expect(window.location.href).toBe("/dashboard");
  });

  it("PU-FE-05: un 403 no cierra la sesión", async () => {
    localStorage.setItem("token", "valido");
    const error = { config: { url: "/Cobros" }, response: { status: 403 } };
    await expect(errorRespuesta(error)).rejects.toBe(error);
    expect(localStorage.getItem("token")).toBe("valido");
  });
});
