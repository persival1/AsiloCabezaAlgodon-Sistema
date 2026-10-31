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

// Es normal que el proveedor y su hook vivan juntos. Esta regla solo afecta la recarga
// en caliente mientras se programa (no al sistema publicado), por eso se desactiva aquí.
// eslint-disable-next-line react-refresh/only-export-components
export function useAutenticacion() {
  return useContext(AutenticacionContexto);
}