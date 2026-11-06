// Plan de pruebas SQA, semana 5: escenarios E1 y E2 en el navegador.
// Van en orden (serial): E2 cobra la visita que crea E1. Cada ejecución usa nombres nuevos,
// así no choca con datos de ejecuciones anteriores.
import { test, expect } from "@playwright/test";
import { campo, elegirOpcion, entrarComo, mananaALas10 } from "./ayudantes";

const sello = Date.now().toString().slice(-6);
const nombreInterno = `Prueba E2E ${sello}`;
const motivo = `Control E2E ${sello}`;
const diagnostico = `Diagnóstico E2E ${sello}`;
const examen = `Hemograma E2E ${sello}`;
const medicamento = `Paracetamol E2E ${sello}`;

test.describe.serial("Atención médica y pago de la cita", () => {
  test("E1. Atención médica completa: interno, solicitud, asignación, visita, resultado, despacho e informes", async ({ page }) => {
    // RES-01: el médico general registra al interno con su familiar
    await entrarComo(page, "medicogeneral");
    await page.goto("/dashboard/internos");
    await page.getByRole("button", { name: "Nuevo interno" }).click();
    await campo(page, "Nombres").nth(0).fill("Prueba");
    await campo(page, "Apellidos").nth(0).fill(`E2E ${sello}`);
    await campo(page, "Fecha de nacimiento").fill("1945-03-10");
    await campo(page, "Fecha de ingreso").fill("2024-01-15");
    await campo(page, "Motivo de ingreso").fill("Prueba automática de extremo a extremo");
    await campo(page, "Nombres").nth(1).fill("Familiar");
    await campo(page, "Apellidos").nth(1).fill(`E2E ${sello}`);
    await campo(page, "Parentesco").fill("Hija");
    await campo(page, "Teléfono").fill("55551234");
    await campo(page, "Correo (para notificaciones)").fill("familiar.e2e@prueba.test");
    await campo(page, "Cuota mensual (Q)").fill("500");
    await page.getByRole("button", { name: "Guardar interno" }).click();
    await expect(page.locator("tr", { hasText: nombreInterno })).toBeVisible();

    // EXP-04: el médico general pide la cita
    await page.goto("/dashboard/solicitudes");
    await elegirOpcion(campo(page, "Interno"), nombreInterno);
    await campo(page, "Motivo").fill(motivo);
    await campo(page, "Especialidad a la que referís al paciente").fill("Geriatría");
    await page.getByRole("button", { name: "Crear solicitud" }).click();
    await expect(page.locator("tr", { hasText: motivo })).toContainText("Pendiente");

    // EXP-05: el administrador asigna especialista y fecha
    await entrarComo(page, "admin", "admin123");
    await page.goto("/dashboard/solicitudes");
    await page.locator("tr", { hasText: motivo }).getByRole("button", { name: "Asignar" }).click();
    await elegirOpcion(campo(page, "Especialista"), "Dr. Especialista");
    await campo(page, "Fecha y hora de la cita").fill(mananaALas10());
    await page.getByRole("button", { name: "Asignar y notificar al familiar" }).click();
    await expect(page.getByText(/^Solicitud asignada/)).toBeVisible();
    await expect(page.locator("tr", { hasText: motivo })).toContainText("Asignada");

    // EXP-08: el especialista asignado registra la visita con un examen y una receta
    await entrarComo(page, "especialista");
    await page.goto("/dashboard/citas");
    await page.locator("tr", { hasText: motivo }).getByRole("button", { name: "Registrar visita" }).click();
    await campo(page, "Diagnóstico").fill(diagnostico);
    await campo(page, "Observaciones").fill("Prueba automática");
    await campo(page, /^Costo de la consulta/).fill("150");
    await page.getByPlaceholder("Nombre del examen").fill(examen);
    await page.getByRole("button", { name: "Agregar", exact: true }).click();
    await campo(page, "Medicamento").fill(medicamento);
    await campo(page, "Dosis").fill("500 mg");
    await campo(page, "Indicaciones").fill("Cada 8 horas por 3 días");
    await page.getByRole("button", { name: "Agregar medicamento" }).click();
    await page.getByRole("button", { name: "Guardar visita" }).click();
    await expect(page.locator("tr", { hasText: motivo })).toContainText("Atendida");

    // LAB-02: laboratorio registra el resultado (Q30.00)
    await entrarComo(page, "laboratorio");
    await page.goto("/dashboard/examenes");
    await page.locator("tr", { hasText: examen }).getByRole("button", { name: "Registrar resultado" }).click();
    await campo(page, "Resultado").fill("Valores dentro de rango");
    await campo(page, /^Costo \(Q,/).fill("30");
    await page.getByRole("button", { name: "Guardar resultado" }).click();
    await expect(page.locator("tr", { hasText: examen })).toContainText("Valores dentro de rango");

    // FAR-02: farmacia despacha la receta (Q12.50)
    await entrarComo(page, "farmacia");
    await page.goto("/dashboard/prescripciones");
    await page.locator("tr", { hasText: medicamento }).getByRole("button", { name: "Despachar" }).click();
    await campo(page, "Costo (Q)").fill("12.50");
    await page.getByRole("button", { name: "Confirmar despacho" }).click();
    await expect(page.locator("tr", { hasText: medicamento })).toContainText("Despachado");

    // REP-01 y REP-02: informes de costos por cita y ficha médica
    await entrarComo(page, "admin", "admin123");
    await page.goto("/dashboard/reportes");
    await page.getByRole("button", { name: "Generar reporte" }).click(); // el informe 1 viene elegido
    await expect(page.locator("tr", { hasText: nombreInterno })).toContainText("Q192.50"); // 150 + 30 + 12.50

    await campo(page, "Reporte").selectOption("ficha-medica");
    await elegirOpcion(campo(page, "Paciente"), nombreInterno);
    await page.getByRole("button", { name: "Generar reporte" }).click();
    await expect(page.getByRole("heading", { name: nombreInterno })).toBeVisible();
    await expect(page.locator("tr", { hasText: diagnostico })).toContainText(medicamento);
  });

  test("E2. Pago parcial y total de la cita, estado de pago y conciliación", async ({ page }) => {
    const tarjetaVisita = page.locator(".tarjeta-formulario", { hasText: diagnostico });

    async function cobrarCargoMedico(monto) {
      await page.goto("/dashboard/cobros");
      await page.getByRole("button", { name: "Nuevo cobro" }).click();
      await elegirOpcion(campo(page, "Interno"), nombreInterno);
      await campo(page, "Concepto").selectOption("Cargo médico");
      await elegirOpcion(campo(page, "Visita a la que corresponde el cargo"), diagnostico);
      await campo(page, "Monto (Q)").fill(monto);
      await page.getByRole("button", { name: "Registrar cobro" }).click();
      await expect(page.getByRole("button", { name: "Registrar cobro" })).toBeHidden();
    }

    await entrarComo(page, "admin", "admin123");

    // FIN-02: antes de pagar, la visita está pendiente de pago por Q192.50
    await page.goto("/dashboard/visitas");
    await expect(tarjetaVisita).toContainText("Pendiente de pago");
    await expect(tarjetaVisita).toContainText("(Q0.00 de Q192.50)");

    // Pago parcial: sigue pendiente
    await cobrarCargoMedico("100");
    await page.goto("/dashboard/visitas");
    await expect(tarjetaVisita).toContainText("Pendiente de pago");
    await expect(tarjetaVisita).toContainText("(Q100.00 de Q192.50)");

    // Pago del resto: queda pagada
    await cobrarCargoMedico("92.50");
    await page.goto("/dashboard/visitas");
    await expect(tarjetaVisita).toContainText("Pagada");
    await expect(tarjetaVisita).toContainText("(Q192.50 de Q192.50)");

    // FIN-03: el saldo del familiar queda en Q0.00 en la lista de cobros
    await page.goto("/dashboard/cobros");
    await expect(page.locator("tr", { hasText: nombreInterno }).first()).toContainText("Q0.00");

    // REP-08: la conciliación cuadra para este familiar (no tiene pantalla: se consulta la API
    // con la sesión del administrador)
    const token = await page.evaluate(() => localStorage.getItem("token"));
    const respuesta = await page.request.get("https://localhost:7022/api/Reportes/conciliacion-saldos", {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(respuesta.ok()).toBeTruthy();
    const fila = (await respuesta.json()).find((f) => f.nombreInterno === nombreInterno);
    expect(fila.saldoGuardado).toBe(0);
    expect(fila.cuadra).toBe(true);
  });
});
