import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import "./index.css";
import App from "./App.jsx";
import { AutenticacionProveedor } from "./contexto/AutenticacionContexto";
import { instalarValidacionDeCampos } from "./utilidades/validacionCampos";

instalarValidacionDeCampos();

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <BrowserRouter>
      <AutenticacionProveedor>
        <App />
      </AutenticacionProveedor>
    </BrowserRouter>
  </StrictMode>
);