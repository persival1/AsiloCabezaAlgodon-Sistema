// AC-08 y AC-05: las operaciones nuevas, en el navegador y con la API real.
// Crea datos (un usuario, un interno y una solicitud por ejecución): solo en local, nunca en Azure.
// Las capturas quedan en pruebas/evidencia-ac/ac-08 como evidencia.
import { test, expect } from "@playwright/test";
import path from "node:path";
import { campo, elegirOpcion, entrarComo } from "./ayudantes";

const sello = Date.now().toString().slice(-6);
const usuarioNuevo = `lab.e2e${sello}`;
const apellidoInterno = `Operaciones ${sello}`;
const motivo = `Revisión E2E ${sello}`;
const EVIDENCIA = path.resolve("../pruebas/evidencia-ac/ac-08");

async function foto(page, nombre) {
  await page.evaluate(() => { document.activeElement?.blur?.(); window.scrollTo(0, 0); });
  await page.screenshot({ path: path.join(EVIDENCIA, `${nombre}.png`), fullPage: true });
}

async function salir(page) {
  await page.getByRole("button", { name: "Cerrar sesión" }).click();
  await expect(page).toHaveURL(/\/$/);
}

test.describe.serial("AC-08 y AC-05: operaciones sobre usuarios, internos y solicitudes", () => {
  test.beforeEach(({ page }) => {
    page.on("dialog", (d) => d.accept()); // confirmaciones de «¿Desactivar…?», «¿Quitar…?», «¿Cancelar…?»
  });

  test("CU-004 y AC-05: crear usuario, contraseña temporal, cambio de contraseña y desactivación", async ({ page }) => {
    await entrarComo(page, "admin", "admin123");
    await page.getByRole("link", { name: "Usuarios", exact: true }).click();

    // Crear: primero con contraseña débil (la rechaza el servidor), luego válida
    await page.getByRole("button", { name: "Nuevo usuario" }).click();
    await campo(page, "Usuario (para iniciar sesión)").fill(usuarioNuevo);
    await campo(page, "Nombre completo").fill("Laboratorista de prueba");
    await campo(page, "Correo").fill("lab.e2e@prueba.test");
    await page.locator("#u-rol").selectOption("Laboratorio");
    await campo(page, "Contraseña inicial").fill("soloLetras");
    await page.getByRole("button", { name: "Guardar usuario" }).click();
    await expect(page.locator("form .error")).toContainText("letras y números");
    await campo(page, "Contraseña inicial").fill("inicial2026");
    await page.getByRole("button", { name: "Guardar usuario" }).click();
    const fila = page.locator("tr", { hasText: usuarioNuevo });
    await expect(fila).toContainText("Laboratorio");
    await foto(page, "usuarios-creado");

    // AC-05: el Administrador le asigna una contraseña temporal
    await fila.getByRole("button", { name: "Contraseña" }).click();
    await campo(page, "Contraseña temporal").fill("temporal2026");
    await page.getByRole("button", { name: "Asignar contraseña" }).click();
    await expect(page.locator(".exito")).toContainText("Contraseña temporal asignada");
    await salir(page);

    // El usuario entra con la temporal y la cambia por una propia
    await entrarComo(page, usuarioNuevo, "temporal2026");
    await page.getByRole("link", { name: "Cambiar contraseña" }).click();
    await campo(page, "Contraseña actual").fill("equivocada1");
    await campo(page, "Contraseña nueva").fill("propia2026x");
    await campo(page, "Repetí la contraseña nueva").fill("propia2026x");
    await page.getByRole("button", { name: "Cambiar contraseña" }).click();
    await expect(page.locator(".error")).toContainText("actual no es correcta");
    await campo(page, "Contraseña actual").fill("temporal2026");
    await page.getByRole("button", { name: "Cambiar contraseña" }).click();
    await expect(page.locator(".exito")).toContainText("tu contraseña cambió");
    await foto(page, "cambiar-contrasena");
    await salir(page);
    await entrarComo(page, usuarioNuevo, "propia2026x");
    await salir(page);

    // Desactivado: ya no puede iniciar sesión
    await entrarComo(page, "admin", "admin123");
    await page.goto("/dashboard/usuarios");
    await page.locator("tr", { hasText: usuarioNuevo }).getByRole("button", { name: "Desactivar" }).click();
    await expect(page.locator("tr", { hasText: usuarioNuevo })).toContainText("inactivo");
    await foto(page, "usuarios-desactivado");
    await salir(page);
    await campo(page, "Usuario").fill(usuarioNuevo);
    await campo(page, "Contraseña").fill("propia2026x");
    await page.getByRole("button", { name: "Ingresar" }).click();
    await expect(page.locator(".login-formulario .error")).toContainText("incorrectos");
  });

  test("CU-006, CU-007, CU-009, CU-010, CU-013 y CU-014: interno y solicitud", async ({ page }) => {
    // La prueba anterior usó 7 de los 10 intentos por minuto del login (y del cambio de
    // contraseña, que comparte el límite): se espera a que se libere.
    test.setTimeout(240_000);
    await page.waitForTimeout(61_000);

    // El Administrador registra un interno y lo corrige
    await entrarComo(page, "admin", "admin123");
    await page.goto("/dashboard/internos");
    await page.getByRole("button", { name: "Nuevo interno" }).click();
    await campo(page, "Nombres").nth(0).fill("Prueba");
    await campo(page, "Apellidos").nth(0).fill(apellidoInterno);
    await campo(page, "Fecha de nacimiento").fill("1944-02-02");
    await campo(page, "Fecha de ingreso").fill("2025-01-10");
    await campo(page, "Motivo de ingreso").fill("Prueba de operaciones");
    await campo(page, "Nombres").nth(1).fill("Familiar");
    await campo(page, "Apellidos").nth(1).fill(apellidoInterno);
    await campo(page, "Parentesco").fill("Hijo");
    await campo(page, "Teléfono").fill("55550000");
    await campo(page, "Correo (para notificaciones)").fill("familiar.op@prueba.test");
    await campo(page, "Cuota mensual (Q)").fill("400");
    await page.getByRole("button", { name: "Guardar interno" }).click();
    await page.locator("tr", { hasText: apellidoInterno }).getByRole("link", { name: "Ver ficha" }).click();

    // CU-006: editar datos
    await page.getByRole("button", { name: "Editar datos" }).click();
    await campo(page, "Teléfono").fill("55559999");
    await campo(page, "Cuota mensual (Q)").fill("450");
    await page.getByRole("button", { name: "Guardar cambios" }).click();
    await expect(page.locator(".exito")).toContainText("Se guardaron los datos");
    await expect(page.locator("main")).toContainText("55559999");
    await expect(page.locator("main")).toContainText("Q450.00");

    // CU-009: agregar, corregir y quitar una psicopatología
    const psicos = page.locator(".tarjeta-formulario", { hasText: "Psicopatologías" });
    await psicos.getByRole("button", { name: "Agregar" }).click();
    await campo(page, "Nombre").fill("Ansieda");
    await psicos.getByRole("button", { name: "Guardar" }).click();
    await psicos.getByRole("button", { name: "Corregir" }).click();
    await campo(page, "Nombre").fill("Ansiedad generalizada");
    await psicos.getByRole("button", { name: "Guardar" }).click();
    await expect(psicos).toContainText("Ansiedad generalizada");
    await foto(page, "ficha-editada");
    await psicos.getByRole("button", { name: "Quitar" }).click();
    await expect(psicos).toContainText("Ninguna registrada");

    // CU-013 y CU-014: el médico general crea, corrige y cancela una solicitud
    await salir(page);
    await entrarComo(page, "medicogeneral");
    await page.goto("/dashboard/solicitudes");
    await elegirOpcion(campo(page, "Interno"), apellidoInterno);
    await campo(page, "Motivo").fill("Revisoin");
    await page.getByRole("button", { name: "Crear solicitud" }).click();
    await page.locator("tr", { hasText: "Revisoin" }).getByRole("button", { name: "Corregir" }).click();
    await campo(page, "Motivo").fill(motivo);
    await page.getByRole("button", { name: "Guardar corrección" }).click();
    const solicitud = page.locator("tr", { hasText: motivo });
    await expect(solicitud).toContainText("Pendiente");
    await solicitud.getByRole("button", { name: "Cancelar" }).click();
    await expect(solicitud).toContainText("Cancelada");
    await foto(page, "solicitud-cancelada");

    // CU-007: el Administrador da de baja al interno (ya no tiene solicitudes abiertas)
    await salir(page);
    await entrarComo(page, "admin", "admin123");
    await page.goto("/dashboard/internos");
    await page.locator("tr", { hasText: apellidoInterno }).getByRole("link", { name: "Ver ficha" }).click();
    await page.getByRole("button", { name: "Dar de baja" }).click();
    await expect(page.locator(".exito")).toContainText("quedó de baja");
    await expect(page.getByRole("button", { name: "Reactivar" })).toBeVisible();
    await foto(page, "interno-de-baja");

    // Un interno de baja ya no aparece para crear solicitudes
    await salir(page);
    await entrarComo(page, "medicogeneral");
    await page.goto("/dashboard/solicitudes");
    await expect(campo(page, "Interno").locator("option", { hasText: apellidoInterno })).toHaveCount(0);
  });
});
