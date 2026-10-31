// Configuración de las pruebas automatizadas del frontend (Vitest).
// Tomada del Anexo A del plan de pruebas de SQA (25/10/2026).
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    include: ["src/__tests__/**/*.test.{js,jsx}"],
  },
});
