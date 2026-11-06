// PU-FE-06 y PU-FE-07: componente de ruta protegida
// Pruebas escritas por SQA (plan de pruebas, Anexo A) e incorporadas al repositorio.
import { describe, it, expect, beforeEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router-dom";
import { AutenticacionProveedor } from "../contexto/AutenticacionContexto.jsx";
import RutaProtegida from "../componentes/RutaProtegida.jsx";

function montar() {
  return render(
    <AutenticacionProveedor>
      <MemoryRouter initialEntries={["/dashboard"]}>
        <Routes>
          <Route path="/" element={<p>Pantalla de login</p>} />
          <Route path="/dashboard" element={<RutaProtegida><p>Contenido protegido</p></RutaProtegida>} />
        </Routes>
      </MemoryRouter>
    </AutenticacionProveedor>
  );
}

describe("RutaProtegida", () => {
  beforeEach(() => {
    cleanup();
    localStorage.clear();
  });

  it("PU-FE-06: sin token redirige al login", () => {
    montar();
    expect(screen.getByText("Pantalla de login")).toBeTruthy();
    expect(screen.queryByText("Contenido protegido")).toBeNull();
  });

  it("PU-FE-07: con token muestra el contenido", () => {
    localStorage.setItem("token", "token-de-prueba");
    localStorage.setItem("rol", "Farmacia");
    montar();
    expect(screen.getByText("Contenido protegido")).toBeTruthy();
  });
});
