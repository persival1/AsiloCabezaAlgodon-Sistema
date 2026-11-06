// Plan de pruebas SQA, semana 5: escenario E3 (AUT-10) en el navegador.
// No crea datos ni inicia sesión: pone un token falso, como si hubiera vencido.
import { test, expect } from "@playwright/test";

test("E3. Sesión vencida: vuelve al login con aviso y borra el token", async ({ page }) => {
  await page.goto("/");
  await page.evaluate(() => {
    localStorage.setItem("token", "token-vencido-o-firmado-con-otra-llave");
    localStorage.setItem("rol", "Administrador");
  });

  await page.goto("/dashboard/internos");

  await expect(page.getByText("Tu sesión expiró o ya no es válida. Iniciá sesión de nuevo.")).toBeVisible();
  await expect(page).toHaveURL(/:5173\/$/);
  expect(await page.evaluate(() => localStorage.getItem("token"))).toBeNull();
});
