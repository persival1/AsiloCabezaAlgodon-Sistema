// AC-13 (punto 5): mensajes de validación en español junto al campo.
import { describe, expect, it, beforeAll } from "vitest";
import { instalarValidacionDeCampos } from "../utilidades/validacionCampos";

beforeAll(() => instalarValidacionDeCampos(document));

function formulario(html) {
  document.body.innerHTML = `<form>${html}<button type="submit">Guardar</button></form>`;
  return document.querySelector("form");
}

describe("validación de campos (PU-FE-VAL)", () => {
  it("un campo obligatorio vacío muestra el mensaje junto al campo", () => {
    const form = formulario(`<div class="campo"><label>Nombres</label><input required /></div>`);
    expect(form.checkValidity()).toBe(false);
    const caja = form.querySelector(".campo");
    expect(caja.dataset.error).toBe("Este campo es obligatorio.");
    expect(form.querySelector("input").getAttribute("aria-invalid")).toBe("true");
  });

  it("el mensaje desaparece cuando el dato se corrige", () => {
    const form = formulario(`<div class="campo"><label>Correo</label><input type="email" required /></div>`);
    const input = form.querySelector("input");
    input.value = "no-es-correo";
    form.checkValidity();
    expect(form.querySelector(".campo").dataset.error).toMatch(/correo válido/);

    input.value = "familia@correo.com";
    input.dispatchEvent(new Event("input", { bubbles: true }));
    expect(form.querySelector(".campo").dataset.error).toBeUndefined();
    expect(input.hasAttribute("aria-invalid")).toBe(false);
  });

  it("los montos fuera de rango dicen el límite", () => {
    const form = formulario(`<div class="campo"><label>Monto</label><input type="number" min="0.01" max="545" value="600" /></div>`);
    form.checkValidity();
    expect(form.querySelector(".campo").dataset.error).toBe("El valor máximo es 545.");
  });

  it("un select obligatorio pide elegir una opción", () => {
    const form = formulario(`<div class="campo"><label>Interno</label><select required><option value="">Elegí</option><option value="1">A</option></select></div>`);
    form.checkValidity();
    expect(form.querySelector(".campo").dataset.error).toBe("Elegí una opción.");
  });
});
