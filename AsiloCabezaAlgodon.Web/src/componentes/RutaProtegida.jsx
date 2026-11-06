import { Navigate } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";

function RutaProtegida({ children }) {
  const { token } = useAutenticacion();

  if (!token) {
    return <Navigate to="/" />;
  }

  return children;
}

export default RutaProtegida;