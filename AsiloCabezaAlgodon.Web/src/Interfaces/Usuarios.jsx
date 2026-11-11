import { useEffect, useState } from "react";
import { listarUsuarios, crearUsuario, editarUsuario, cambiarEstadoUsuario, restablecerClave } from "../Servicios/usuariosServicio";
import { mensajeDeError } from "../Servicios/api";
import Badge from "../componentes/Badge";
import { FiltrosTabla, PaginacionTabla } from "../componentes/ControlesTabla";
import { useTabla } from "../utilidades/useTabla";

// AC-08 (CU-004) y AC-05: el Administrador crea, modifica, activa o desactiva usuarios y
// restablece contraseñas olvidadas con una temporal.
const ROLES = [
  { valor: "Administrador", texto: "Administrador" },
  { valor: "MedicoGeneral", texto: "Médico general" },
  { valor: "Especialista", texto: "Especialista" },
  { valor: "Enfermero", texto: "Enfermero(a)" },
  { valor: "Laboratorio", texto: "Laboratorio" },
  { valor: "Farmacia", texto: "Farmacia" },
];
const NOMBRE_ROL = Object.fromEntries(ROLES.map((r) => [r.valor, r.texto]));
const OPCIONES_TABLA = { campos: ["nombreUsuario", "nombreCompleto", "correo", "especialidad"], estado: (u) => u.rol };
const VACIO = { nombreUsuario: "", nombreCompleto: "", correo: "", rol: "", especialidad: "", clave: "" };

function Usuarios() {
  const [usuarios, setUsuarios] = useState([]);
  const [form, setForm] = useState(null); // null = cerrado; { ...datos, idUsuario? }
  const [claveDe, setClaveDe] = useState(null); // usuario al que se le restablece la clave
  const [claveTemporal, setClaveTemporal] = useState("");
  const [error, setError] = useState("");
  const [aviso, setAviso] = useState("");
  const [guardando, setGuardando] = useState(false);
  const tabla = useTabla(usuarios, OPCIONES_TABLA);

  async function cargar() {
    setUsuarios(await listarUsuarios());
  }

  useEffect(() => {
    listarUsuarios().then(setUsuarios);
  }, []);

  const cambiar = (campo, valor) => setForm((f) => ({ ...f, [campo]: valor }));

  function abrirNuevo() {
    setClaveDe(null); setError(""); setAviso("");
    setForm(VACIO);
  }

  function abrirEdicion(u) {
    setClaveDe(null); setError(""); setAviso("");
    setForm({ idUsuario: u.idUsuario, nombreUsuario: u.nombreUsuario, nombreCompleto: u.nombreCompleto, correo: u.correo, rol: u.rol, especialidad: u.especialidad || "" });
  }

  async function guardar(evento) {
    evento.preventDefault();
    setError(""); setGuardando(true);
    try {
      const especialidad = form.rol === "Especialista" ? form.especialidad : null;
      if (form.idUsuario) {
        await editarUsuario(form.idUsuario, { nombreCompleto: form.nombreCompleto, correo: form.correo, rol: form.rol, especialidad });
        setAviso(`Se guardaron los cambios de «${form.nombreUsuario}».`);
      } else {
        await crearUsuario({ ...form, especialidad });
        setAviso(`Usuario «${form.nombreUsuario.trim().toLowerCase()}» creado. Entregale la contraseña en persona.`);
      }
      setForm(null);
      await cargar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo guardar el usuario."));
    } finally {
      setGuardando(false);
    }
  }

  async function alternarEstado(u) {
    const nuevo = u.estado === "activo" ? "inactivo" : "activo";
    if (nuevo === "inactivo" && !window.confirm(`¿Desactivar a ${u.nombreCompleto}? Ya no podrá iniciar sesión ni recibir citas nuevas.`)) return;
    setError(""); setAviso("");
    try {
      await cambiarEstadoUsuario(u.idUsuario, nuevo);
      setAviso(`${u.nombreCompleto} quedó ${nuevo}.`);
      await cargar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo cambiar el estado."));
    }
  }

  async function guardarClave(evento) {
    evento.preventDefault();
    setError(""); setGuardando(true);
    try {
      await restablecerClave(claveDe.idUsuario, claveTemporal);
      setAviso(`Contraseña temporal asignada a «${claveDe.nombreUsuario}». Pedile que la cambie al entrar (menú «Cambiar contraseña»).`);
      setClaveDe(null); setClaveTemporal("");
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo restablecer la contraseña."));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Usuarios</h1>
          <p className="subtitulo">Quién entra al sistema y con qué rol</p>
        </div>
        <button className="boton-primario" onClick={() => (form ? setForm(null) : abrirNuevo())}>
          {form ? "Cancelar" : "Nuevo usuario"}
        </button>
      </div>

      {aviso && <p className="exito">{aviso}</p>}
      {error && !form && !claveDe && <p className="error">{error}</p>}

      {form && (
        <form onSubmit={guardar} className="tarjeta-formulario">
          <h2>{form.idUsuario ? `Editar «${form.nombreUsuario}»` : "Nuevo usuario"}</h2>
          <div className="fila-campos">
            <div className="campo">
              <label htmlFor="u-usuario">Usuario (para iniciar sesión)</label>
              <input id="u-usuario" value={form.nombreUsuario} onChange={(e) => cambiar("nombreUsuario", e.target.value)} required minLength={3} disabled={!!form.idUsuario} />
            </div>
            <div className="campo">
              <label htmlFor="u-nombre">Nombre completo</label>
              <input id="u-nombre" value={form.nombreCompleto} onChange={(e) => cambiar("nombreCompleto", e.target.value)} required />
            </div>
          </div>
          <div className="fila-campos">
            <div className="campo">
              <label htmlFor="u-correo">Correo</label>
              <input id="u-correo" type="email" value={form.correo} onChange={(e) => cambiar("correo", e.target.value)} required />
            </div>
            <div className="campo">
              <label htmlFor="u-rol">Rol</label>
              <select id="u-rol" value={form.rol} onChange={(e) => cambiar("rol", e.target.value)} required>
                <option value="">Elegí un rol</option>
                {ROLES.map((r) => <option key={r.valor} value={r.valor}>{r.texto}</option>)}
              </select>
            </div>
          </div>
          <div className="fila-campos">
            {form.rol === "Especialista" && (
              <div className="campo">
                <label htmlFor="u-especialidad">Especialidad</label>
                <input id="u-especialidad" value={form.especialidad} onChange={(e) => cambiar("especialidad", e.target.value)} required />
              </div>
            )}
            {!form.idUsuario && (
              <div className="campo">
                <label htmlFor="u-clave">Contraseña inicial</label>
                <input id="u-clave" type="password" autoComplete="new-password" minLength={8} value={form.clave} onChange={(e) => cambiar("clave", e.target.value)} required />
                <span className="ayuda-campo">Al menos 8 caracteres, con letras y números.</span>
              </div>
            )}
          </div>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="boton-primario" disabled={guardando}>{guardando ? "Guardando..." : "Guardar usuario"}</button>
        </form>
      )}

      {claveDe && (
        <form onSubmit={guardarClave} className="tarjeta-formulario">
          <h2>Contraseña temporal para «{claveDe.nombreUsuario}»</h2>
          <div className="campo">
            <label htmlFor="u-temporal">Contraseña temporal</label>
            <input id="u-temporal" type="password" autoComplete="new-password" minLength={8} value={claveTemporal} onChange={(e) => setClaveTemporal(e.target.value)} required />
            <span className="ayuda-campo">Entregásela en persona; la persona la cambia en «Cambiar contraseña».</span>
          </div>
          {error && <p className="error">{error}</p>}
          <div style={{ display: "flex", gap: 10 }}>
            <button type="submit" className="boton-primario" disabled={guardando}>Asignar contraseña</button>
            <button type="button" className="boton-secundario" onClick={() => setClaveDe(null)}>Cancelar</button>
          </div>
        </form>
      )}

      <FiltrosTabla tabla={tabla} placeholder="Usuario, nombre, correo o especialidad" estados={ROLES} etiquetaEstado="Rol" conFechas={false} />
      <table className="tabla">
        <thead>
          <tr>
            <th>Usuario</th>
            <th>Nombre</th>
            <th>Rol</th>
            <th>Correo</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {tabla.filas.map((u) => (
            <tr key={u.idUsuario}>
              <td>{u.nombreUsuario}</td>
              <td>{u.nombreCompleto}</td>
              <td>{NOMBRE_ROL[u.rol] || u.rol}{u.especialidad ? ` (${u.especialidad})` : ""}</td>
              <td>{u.correo}</td>
              <td><Badge texto={u.estado} /></td>
              <td className="acciones-fila">
                <button className="boton-secundario" onClick={() => abrirEdicion(u)}>Editar</button>
                <button className="boton-secundario" onClick={() => { setForm(null); setError(""); setAviso(""); setClaveTemporal(""); setClaveDe(u); }}>Contraseña</button>
                <button className="boton-secundario" onClick={() => alternarEstado(u)}>{u.estado === "activo" ? "Desactivar" : "Activar"}</button>
              </td>
            </tr>
          ))}
          {tabla.filas.length === 0 && (
            <tr><td colSpan="6">{tabla.total === 0 ? "No hay usuarios." : "Ningún usuario coincide con la búsqueda."}</td></tr>
          )}
        </tbody>
      </table>
      <PaginacionTabla tabla={tabla} nombre="usuarios" />
    </div>
  );
}

export default Usuarios;
