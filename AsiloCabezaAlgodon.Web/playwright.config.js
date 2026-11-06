// Plan de pruebas SQA, semana 5 (fase 5 de automatización): recorridos completos en el
// navegador con Playwright. Usa el Edge que ya está instalado en Windows (channel "msedge"),
// así no hay que descargar navegadores.
//
// Al correr "npm run e2e" se levantan solos la API (https://localhost:7022) y el frontend
// (http://localhost:5173); si ya están corriendo, se usan esos. Las pruebas CREAN datos
// (un interno, una solicitud, una visita y dos cobros por ejecución): solo en local, nunca en Azure.
import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  timeout: 120_000,
  expect: { timeout: 10_000 },
  workers: 1,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL: "http://localhost:5173",
    channel: "msedge",
    // El certificado de desarrollo de la API es local: no lo firma una entidad pública
    ignoreHTTPSErrors: true,
    trace: "retain-on-failure",
  },
  webServer: [
    {
      command: "dotnet run --project ../AsiloCabezaAlgodon/AsiloCabezaAlgodon.API --launch-profile https",
      url: "https://localhost:7022/swagger/index.html",
      ignoreHTTPSErrors: true,
      reuseExistingServer: true,
      timeout: 180_000,
    },
    {
      command: "npm run dev",
      url: "http://localhost:5173",
      reuseExistingServer: true,
    },
  ],
});
