// Funciones que comparten los recorridos de Playwright.
import { expect } from "@playwright/test";

// Las etiquetas de los formularios no están unidas a su campo con "htmlFor", así que se
// busca el <div className="campo"> que contiene esa etiqueta y, adentro, su input o select.
export function campo(page, etiqueta) {
  const texto = typeof etiqueta === "string"
    ? page.getByText(etiqueta, { exact: true })
    : page.getByText(etiqueta);
  return page.locator(".campo").filter({ has: texto }).locator("input, select");
}

// Elige en un <select> la única opción que contiene ese texto. Espera a que la opción
// aparezca, porque muchas listas se cargan desde la API después de abrir la pantalla.
export async function elegirOpcion(select, texto) {
  const opcion = select.locator("option", { hasText: texto });
  await expect(opcion).toHaveCount(1);
  await select.selectOption(await opcion.getAttribute("value"));
}

// Cierra la sesión anterior (si había) e inicia sesión con otro usuario.
export async function entrarComo(page, usuario, clave = "prueba123") {
  await page.goto("/");
  await page.evaluate(() => localStorage.clear());
  await campo(page, "Usuario").fill(usuario);
  await campo(page, "Contraseña").fill(clave);
  await page.getByRole("button", { name: "Ingresar" }).click();
  await expect(page).toHaveURL(/\/dashboard/);
}

// Mañana a las 10:00, en el formato que pide <input type="datetime-local">.
export function mananaALas10() {
  const d = new Date(Date.now() + 24 * 60 * 60 * 1000);
  const dos = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${dos(d.getMonth() + 1)}-${dos(d.getDate())}T10:00`;
}
