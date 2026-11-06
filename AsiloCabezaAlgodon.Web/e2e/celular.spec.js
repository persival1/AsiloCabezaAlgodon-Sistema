// El sistema se puede usar en un celular: con 375 px de ancho (un teléfono común) ninguna
// pantalla se sale por los lados, y las tablas largas se deslizan dentro de su espacio.
// Guarda una captura de cada pantalla en test-results (sirve como evidencia).
import { test, expect } from "@playwright/test";
import { entrarComo } from "./ayudantes";

test("Celular (375 px): las pantallas caben a lo ancho y las tablas se deslizan", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await entrarComo(page, "admin", "admin123");

  for (const ruta of ["/dashboard", "/dashboard/internos", "/dashboard/solicitudes", "/dashboard/cobros", "/dashboard/reportes"]) {
    await page.goto(ruta);
    await expect(page.getByRole("button", { name: "Cerrar sesión" })).toBeVisible();
    await expect(page.locator("main h1")).toBeVisible();

    const anchoPagina = await page.evaluate(() => document.documentElement.scrollWidth);
    expect(anchoPagina, `ancho de la página en ${ruta}`).toBeLessThanOrEqual(375);

    await page.screenshot({ path: test.info().outputPath(`celular${ruta.replaceAll("/", "-")}.png`), fullPage: true });
  }

  await page.goto("/dashboard/internos");
  const desplazamiento = await page.locator("table.tabla").evaluate((t) => getComputedStyle(t).overflowX);
  expect(desplazamiento).toBe("auto");
});
