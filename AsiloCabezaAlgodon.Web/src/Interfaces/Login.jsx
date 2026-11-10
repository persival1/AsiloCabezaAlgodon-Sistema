import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { iniciarSesion } from "../Servicios/autenticacionServicio";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import VersionApp from "../componentes/VersionApp";

function Login() {
  const [nombreUsuario, setNombreUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  // Si api.js cerró la sesión por un 401, deja una marca para avisar al usuario por qué
  // volvió al login. Se lee una vez y se borra.
  const [aviso] = useState(() =>
    sessionStorage.getItem("sesionExpirada")
      ? "Tu sesión expiró o ya no es válida. Iniciá sesión de nuevo."
      : ""
  );
  useEffect(() => {
    sessionStorage.removeItem("sesionExpirada");
  }, []);

  const navigate = useNavigate();
  const { guardarSesion } = useAutenticacion();

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setError("");
    setCargando(true);

    try {
      const resultado = await iniciarSesion(nombreUsuario, password);
      guardarSesion(resultado.token, resultado.rol);
      navigate("/dashboard");
    } catch (err) {
      // Solo un 401 significa usuario o contraseña incorrectos. Cualquier otro error (por
      // ejemplo, la base de datos despertando) no debe hacerle creer al usuario que se equivocó.
      // 429 = demasiados intentos en un minuto (semana 4).
      const estado = err.response?.status;
      if (estado === 401) setError("Usuario o contraseña incorrectos.");
      else if (estado === 429) setError("Demasiados intentos. Esperá un minuto e intentá de nuevo.");
      else setError("No se pudo conectar con el servidor. Esperá unos segundos e intentá de nuevo.");
    } finally {
      setCargando(false);
    }
  }

  return (
    <div className="login-contenedor">
      <form onSubmit={manejarEnvio} className="login-formulario">
        <span className="marca-icono" style={{ marginBottom: 16 }}>🌿</span>
        <h1>Asilo Cabeza de Algodón</h1>
        <p>Sistema de administración — iniciá sesión para continuar</p>

        <div className="campo">
          <label>Usuario</label>
          <input
            type="text"
            value={nombreUsuario}
            onChange={(e) => setNombreUsuario(e.target.value)}
            required
          />
        </div>

        <div className="campo">
          <label>Contraseña</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        {aviso && !error && <p className="error">{aviso}</p>}
        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={cargando}>
          {cargando ? "Ingresando..." : "Ingresar"}
        </button>
      </form>
      <VersionApp />
    </div>
  );
}

export default Login;