import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { iniciarSesion } from "../servicios/autenticacionServicio";
import { useAutenticacion } from "../contexto/AutenticacionContexto";

function Login() {
  const [nombreUsuario, setNombreUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

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
      setError("Usuario o contraseña incorrectos.");
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

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={cargando}>
          {cargando ? "Ingresando..." : "Ingresar"}
        </button>
      </form>
    </div>
  );
}

export default Login;