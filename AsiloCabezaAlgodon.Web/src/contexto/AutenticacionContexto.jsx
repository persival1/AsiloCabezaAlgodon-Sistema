import { createContext, useContext, useState } from "react";

const AutenticacionContexto = createContext();

export function AutenticacionProveedor({ children }) {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [rol, setRol] = useState(localStorage.getItem("rol"));

  function guardarSesion(nuevoToken, nuevoRol) {
    localStorage.setItem("token", nuevoToken);
    localStorage.setItem("rol", nuevoRol);
    setToken(nuevoToken);
    setRol(nuevoRol);
  }

  function cerrarSesion() {
    localStorage.removeItem("token");
    localStorage.removeItem("rol");
    setToken(null);
    setRol(null);
  }

  return (
    <AutenticacionContexto.Provider
      value={{ token, rol, guardarSesion, cerrarSesion }}
    >
      {children}
    </AutenticacionContexto.Provider>
  );
}

export function useAutenticacion() {
  return useContext(AutenticacionContexto);
}