import { useEffect, useState } from "react";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { listarSolicitudes, crearSolicitud, asignarMedico } from "../Servicios/solicitudesServicio";
import { listarInternos } from "../Servicios/internosServicio";
import { listarUsuarios } from "../Servicios/usuariosServicio";
import Badge from "../componentes/Badge";

function Solicitudes() {
  const { rol } = useAutenticacion();
  const [solicitudes, setSolicitudes] = useState([]);
  const [internos, setInternos] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [error, setError] = useState("");

  const [idInterno, setIdInterno] = useState("");
  const [motivo, setMotivo] = useState("");
  const [especialidadRequerida, setEspecialidadRequerida] = useState("");
  const [creando, setCreando] = useState(false);

  const [solicitudEnAsignacion, setSolicitudEnAsignacion] = useState(null);
  const [especialidadDeAsignacion, setEspecialidadDeAsignacion] = useState(null);
  const [idEspecialista, setIdEspecialista] = useState("");
  const [idEnfermero, setIdEnfermero] = useState("");
  const [fechaHoraCita, setFechaHoraCita] = useState("");
  const [asignando, setAsignando] = useState(false);

  const esMedicoGeneral = rol === "MedicoGeneral";
  const esAdministrador = rol === "Administrador";

  useEffect(() => {
    cargarSolicitudes();
    if (esMedicoGeneral) {
      listarInternos().then(setInternos);
    }
    if (esAdministrador) {
      listarUsuarios().then(setUsuarios);
    }
  }, []);

  async function cargarSolicitudes() {
    const datos = await listarSolicitudes();
    setSolicitudes(datos);
  }

  async function manejarCrear(evento) {
    evento.preventDefault();
    setError("");
    setCreando(true);
    try {
      await crearSolicitud(Number(idInterno), motivo, especialidadRequerida);
      setIdInterno("");
      setMotivo("");
      setEspecialidadRequerida("");
      await cargarSolicitudes();
    } catch (err) {
      setError("No se pudo crear la solicitud.");
    } finally {
      setCreando(false);
    }
  }

  function abrirAsignacion(solicitud) {
    setSolicitudEnAsignacion(solicitud.idSolicitud);
    setEspecialidadDeAsignacion(solicitud.especialidadRequerida);
    setIdEspecialista("");
    setIdEnfermero("");
    setFechaHoraCita("");
  }

  async function manejarAsignar(evento) {
    evento.preventDefault();
    setError("");
    setAsignando(true);
    try {
      await asignarMedico(
        solicitudEnAsignacion,
        Number(idEspecialista),
        idEnfermero ? Number(idEnfermero) : null,
        fechaHoraCita
      );
      setSolicitudEnAsignacion(null);
      await cargarSolicitudes();
    } catch (err) {
      setError("No se pudo asignar la solicitud.");
    } finally {
      setAsignando(false);
    }
  }

  const especialistas = usuarios.filter((u) => u.rol === "Especialista");
  const enfermeros = usuarios.filter((u) => u.rol === "Enfermero");

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Solicitudes</h1>
          <p className="subtitulo">
            {esMedicoGeneral && "Solicitudes que creaste para tus pacientes"}
            {esAdministrador && "Asigná especialista, enfermero(a) y horario a cada solicitud"}
            {!esMedicoGeneral && !esAdministrador && "Historial de solicitudes"}
          </p>
        </div>
      </div>

      {esMedicoGeneral && (
        <form onSubmit={manejarCrear} className="tarjeta-formulario">
          <h2>Nueva solicitud</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Interno</label>
              <select value={idInterno} onChange={(e) => setIdInterno(e.target.value)} required>
                <option value="">Seleccioná un interno</option>
                {internos.map((i) => (
                  <option key={i.idInterno} value={i.idInterno}>
                    {i.nombres} {i.apellidos}
                  </option>
                ))}
              </select>
            </div>
            <div className="campo">
              <label>Motivo</label>
              <input value={motivo} onChange={(e) => setMotivo(e.target.value)} required />
            </div>
          </div>
          <div className="campo">
            <label>Especialidad a la que referís al paciente</label>
            <input
              value={especialidadRequerida}
              onChange={(e) => setEspecialidadRequerida(e.target.value)}
              placeholder="Ej. Geriatría, Cardiología, Traumatología..."
            />
          </div>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="boton-primario" disabled={creando}>
            {creando ? "Creando..." : "Crear solicitud"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Interno</th>
            <th>Motivo</th>
            <th>Especialidad referida</th>
            <th>Fecha</th>
            <th>Estado</th>
            <th>Especialista</th>
            <th>Cita</th>
            {esAdministrador && <th></th>}
          </tr>
        </thead>
        <tbody>
          {solicitudes.map((s) => (
            <tr key={s.idSolicitud}>
              <td>{s.nombreInterno}</td>
              <td>{s.motivo}</td>
              <td>{s.especialidadRequerida || "—"}</td>
              <td>{new Date(s.fechaCreacion).toLocaleDateString()}</td>
              <td><Badge texto={s.estado} /></td>
              <td>{s.nombreEspecialista ? `${s.nombreEspecialista}${s.especialidadEspecialista ? ` (${s.especialidadEspecialista})` : ""}` : "—"}</td>
              <td>{s.fechaHoraCita ? new Date(s.fechaHoraCita).toLocaleString() : "—"}</td>
              {esAdministrador && (
                <td>
                  {s.estado === "Pendiente" && (
                    <button className="boton-secundario" onClick={() => abrirAsignacion(s)}>
                      Asignar
                    </button>
                  )}
                </td>
              )}
            </tr>
          ))}
          {solicitudes.length === 0 && (
            <tr>
              <td colSpan={esAdministrador ? 8 : 7}>No hay solicitudes todavía.</td>
            </tr>
          )}
        </tbody>
      </table>

      {solicitudEnAsignacion && (
        <form onSubmit={manejarAsignar} className="tarjeta-formulario">
          <h2>Asignar médico y horario</h2>
          {especialidadDeAsignacion && (
            <p className="subtitulo" style={{ marginBottom: 14 }}>
              El médico general refirió a este paciente a <strong>{especialidadDeAsignacion}</strong>.
            </p>
          )}
          <div className="fila-campos">
            <div className="campo">
              <label>Especialista</label>
              <select value={idEspecialista} onChange={(e) => setIdEspecialista(e.target.value)} required>
                <option value="">Seleccioná un especialista</option>
                {especialistas.map((u) => (
                  <option key={u.idUsuario} value={u.idUsuario}>
                    {u.nombreCompleto}{u.especialidad ? ` — ${u.especialidad}` : ""}
                  </option>
                ))}
              </select>
            </div>
            <div className="campo">
              <label>Enfermero(a) acompañante</label>
              <select value={idEnfermero} onChange={(e) => setIdEnfermero(e.target.value)}>
                <option value="">Sin acompañamiento</option>
                {enfermeros.map((u) => (
                  <option key={u.idUsuario} value={u.idUsuario}>{u.nombreCompleto}</option>
                ))}
              </select>
            </div>
          </div>
          <div className="campo">
            <label>Fecha y hora de la cita</label>
            <input type="datetime-local" value={fechaHoraCita} onChange={(e) => setFechaHoraCita(e.target.value)} required />
          </div>
          {error && <p className="error">{error}</p>}
          <div className="fila-campos">
            <button type="submit" className="boton-primario" disabled={asignando}>
              {asignando ? "Asignando..." : "Asignar y notificar al familiar"}
            </button>
            <button type="button" className="boton-secundario" onClick={() => setSolicitudEnAsignacion(null)}>
              Cancelar
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

export default Solicitudes;
