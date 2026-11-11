import { useState } from "react";
import { cambiarClave } from "../Servicios/autenticacionServicio";
import { mensajeDeError } from "../Servicios/api";

// AC-05: cualquier usuario cambia su contraseña. Si la olvidó, el Administrador le asigna
// una temporal desde «Usuarios» y aquí la cambia por una propia.
function CambiarClave() {
  const [actual, setActual] = useState("");
  const [nueva, setNueva] = useState("");
  const [repetida, setRepetida] = useState("");
  const [error, setError] = useState("");
  const [exito, setExito] = useState("");
  const [guardando, setGuardando] = useState(false);

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setError("");
    setExito("");
    if (nueva !== repetida) {
      setError("La contraseña nueva y su confirmación no coinciden.");
      return;
    }
    setGuardando(true);
    try {
      await cambiarClave(actual, nueva);
      setActual(""); setNueva(""); setRepetida("");
      setExito("Listo: tu contraseña cambió. Usala la próxima vez que inicies sesión.");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo cambiar la contraseña."));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Cambiar contraseña</h1>
          <p className="subtitulo">Al menos 8 caracteres, con letras y números</p>
        </div>
      </div>
      <form onSubmit={manejarEnvio} className="tarjeta-formulario" style={{ maxWidth: 480 }}>
        <h2>Tu contraseña</h2>
        <div className="campo">
          <label htmlFor="clave-actual">Contraseña actual</label>
          <input id="clave-actual" type="password" autoComplete="current-password" value={actual} onChange={(e) => setActual(e.target.value)} required />
        </div>
        <div className="campo">
          <label htmlFor="clave-nueva">Contraseña nueva</label>
          <input id="clave-nueva" type="password" autoComplete="new-password" minLength={8} value={nueva} onChange={(e) => setNueva(e.target.value)} required />
        </div>
        <div className="campo">
          <label htmlFor="clave-repetida">Repetí la contraseña nueva</label>
          <input id="clave-repetida" type="password" autoComplete="new-password" minLength={8} value={repetida} onChange={(e) => setRepetida(e.target.value)} required />
        </div>
        {error && <p className="error">{error}</p>}
        {exito && <p className="exito">{exito}</p>}
        <button type="submit" className="boton-primario" disabled={guardando}>
          {guardando ? "Guardando..." : "Cambiar contraseña"}
        </button>
      </form>
    </div>
  );
}

export default CambiarClave;
