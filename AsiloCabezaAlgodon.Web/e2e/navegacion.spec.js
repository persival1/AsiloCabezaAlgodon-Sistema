// AC-13 (puntos 8 y 9): cada rol ve en el menú solo lo que la matriz de AC-09 le permite,
// todas las opciones abren su pantalla sin errores, el botón "atrás" del navegador regresa
// y los indicadores del inicio llevan a su módulo. Solo lee datos: no crea nada.
import { test, expect } from "@playwright/test";
import { entrarComo } from "./ayudantes";

// Mismo contenido que docs/matriz-actores-y-roles.md (sección 3)
const MENU_POR_ROL = {
  admin: ["Inicio", "Internos", "Solicitudes", "Visitas médicas", "Cobros", "Donaciones", "Gastos", "Pagos a la fundación", "Reportes", "Usuarios"],
  medicogeneral: ["Inicio", "Internos", "Solicitudes", "Reportes"],
  especialista: ["Inicio", "Mis citas", "Visitas médicas", "Mis pacientes", "Reportes"],
  enfermero: ["Inicio", "Mis acompañamientos"],
  laboratorio: ["Inicio", "Exámenes solicitados"],
  farmacia: ["Inicio", "Prescripciones"],
};

for (const [usuario, esperado] of Object.entries(MENU_POR_ROL)) {
  test(`Navegación de ${usuario}: menú, pantallas, regreso e indicadores`, async ({ page }) => {
    const errores = [];
    page.on("pageerror", (e) => errores.push(e.message));

    await entrarComo(page, usuario, usuario === "admin" ? "admin123" : "prueba123");
    const menu = page.getByRole("navigation", { name: "Menú principal" });

    // Punto 8: el menú tiene exactamente las opciones del rol
    await expect(menu.getByRole("link")).toHaveText(esperado);

    // Punto 9: cada opción abre su pantalla con un título y queda marcada como activa
    for (const opcion of esperado) {
      await menu.getByRole("link", { name: opcion, exact: true }).click();
      await expect(page.locator("main h1")).toBeVisible();
      await expect(menu.getByRole("link", { name: opcion, exact: true })).toHaveAttribute("aria-current", "page");
    }

    // El botón "atrás" del navegador regresa a la pantalla anterior
    if (esperado.length > 2) {
      const penultima = esperado.at(-2);
      await page.goBack();
      await expect(menu.getByRole("link", { name: penultima, exact: true })).toHaveAttribute("aria-current", "page");
    }

    // Punto 1: el primer indicador del inicio lleva a su módulo en un clic
    await menu.getByRole("link", { name: "Inicio", exact: true }).click();
    const indicador = page.locator("a.tarjeta-stat").first();
    await expect(indicador).toBeVisible();
    const destino = await indicador.getAttribute("href");
    await indicador.click();
    await expect(page).toHaveURL(new RegExp(`${destino}$`));
    await expect(page.locator("main h1")).toBeVisible();

    expect(errores, "errores de JavaScript en la página").toEqual([]);
  });
}
