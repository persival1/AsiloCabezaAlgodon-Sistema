import { useEffect, useState } from "react";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { listarSolicitudes, crearSolicitud, asignarMedico, editarSolicitud, cancelarSolicitud } from "../Servicios/solicitudesServicio";
import { listarInternos } from "../Servicios/internosServicio";
import { listarUsuarios } from "../Servicios/usuariosServicio";
import { mensajeDeError } from "../Servicios/api";
import Badge from "../componentes/Badge";
import { FiltrosTabla, PaginacionTabla } from "../componentes/ControlesTabla";
import { useTabla } from "../utilidades/useTabla";

// Búsqueda por interno, motivo o especialista; filtros por estado y fecha de creación (AC-13)
const OPCIONES_TABLA = {
  campos: ["nombreInterno", "motivo", "especialidadRequerida", "nombreEspecialista"],
  estado: (s) => s.estado,
  fecha: (s) => s.fechaCreacion,
};
const ESTADOS = ["Pendiente", "Asignada", "Atendida", "Cancelada"].map((e) => ({ valor: e, texto: e }));

function Solicitudes() {
  const { rol } = useAutenticacion();
  const [solicitudes, setSolicitudes] = useState([]);
  const [internos, setInternos] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [error, setError] = useState("");
  // Mensaje del servidor después de asignar: dice si el correo al familiar salió o no (COR-04).
  const [aviso, setAviso] = useState(null);

  const [idInterno, setIdInterno] = useState("");
  const [motivo, setMotivo] = useState("");
  const [especialidadRequerida, setEspecialidadRequerida] = useState("");
  const [creando, setCreando] = useState(false);
  // AC-08: solicitud que el médico general está corrigiendo (null = crea una nueva)
  const [idEditando, setIdEditando] = useState(null);

  const [solicitudEnAsignacion, setSolicitudEnAsignacion] = useState(null);
  const [especialidadDeAsignacion, setEspecialidadDeAsignacion] = useState(null);
  const [idEspecialista, setIdEspecialista] = useState("");
  const [idEnfermero, setIdEnfermero] = useState("");
  const [fechaHoraCita, setFechaHoraCita] = useState("");
  const [asignando, setAsignando] = useState(false);

  const tabla = useTabla(solicitudes, OPCIONES_TABLA);
  const esMedicoGeneral = rol === "MedicoGeneral";
  const esAdministrador = rol === "Administrador";

  async function cargarSolicitudes() {
    const datos = await listarSolicitudes();
    setSolicitudes(datos);
  }

  useEffect(() => {
    listarSolicitudes().then(setSolicitudes);
    if (esMedicoGeneral) {
      // AC-08: a un interno dado de baja no se le crean solicitudes
      listarInternos().then((lista) => setInternos(lista.filter((i) => i.estado === "activo")));
    }
    if (esAdministrador) {
      listarUsuarios().then(setUsuarios);
    }
  }, [esMedicoGeneral, esAdministrador]);

  async function manejarCrear(evento) {
    evento.preventDefault();
    setError("");
    setCreando(true);
    try {
      if (idEditando) await editarSolicitud(idEditando, motivo, especialidadRequerida);
      else await crearSolicitud(Number(idInterno), motivo, especialidadRequerida);
      setIdEditando(null);
      setIdInterno("");
      setMotivo("");
      setEspecialidadRequerida("");
      await cargarSolicitudes();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo crear la solicitud."));
    } finally {
      setCreando(false);
    }
  }

  // Asignar una solicitud Pendiente, o reprogramar una cita Asignada (CU-018): mismo formulario,
  // con los datos actuales de la cita ya puestos.
  function abrirAsignacion(solicitud) {
    setSolicitudEnAsignacion(solicitud.idSolicitud);
    setEspecialidadDeAsignacion(solicitud.especialidadRequerida);
    setIdEspecialista(solicitud.idEspecialista ? String(solicitud.idEspecialista) : "");
    setIdEnfermero(solicitud.idEnfermero ? String(solicitud.idEnfermero) : "");
    setFechaHoraCita(solicitud.fechaHoraCita ? String(solicitud.fechaHoraCita).slice(0, 16) : "");
  }

  function abrirEdicion(solicitud) {
    setIdEditando(solicitud.idSolicitud);
    setIdInterno(String(solicitud.idInterno));
    setMotivo(solicitud.motivo);
    setEspecialidadRequerida(solicitud.especialidadRequerida || "");
    setError("");
    window.scrollTo(0, 0);
  }

  function cancelarEdicion() {
    setIdEditando(null);
    setIdInterno("");
    setMotivo("");
    setEspecialidadRequerida("");
  }

  async function manejarCancelar(solicitud) {
    const conCita = solicitud.estado === "Asignada";
    const pregunta = conCita
      ? `¿Cancelar la cita de ${solicitud.nombreInterno}? Se le avisará al familiar por correo.`
      : `¿Cancelar la solicitud de ${solicitud.nombreInterno}?`;
    if (!window.confirm(pregunta)) return;
    setError("");
    setAviso(null);
    try {
      const respuesta = await cancelarSolicitud(solicitud.idSolicitud);
      setAviso(conCita
        ? { correoEnviado: respuesta.correoEnviado, mensaje: respuesta.correoEnviado ? "Cita cancelada y aviso enviado al familiar." : "Cita cancelada, pero el correo al familiar no se pudo enviar." }
        : { correoEnviado: true, mensaje: "Solicitud cancelada." });
      await cargarSolicitudes();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo cancelar la solicitud."));
    }
  }

  async function manejarAsignar(evento) {
    evento.preventDefault();
    setError("");
    setAviso(null);
    setAsignando(true);
    try {
      const respuesta = await asignarMedico(
        solicitudEnAsignacion,
        Number(idEspecialista),
        idEnfermero ? Number(idEnfermero) : null,
        fechaHoraCita
      );
      setAviso(respuesta);
      setSolicitudEnAsignacion(null);
      await cargarSolicitudes();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo asignar la solicitud."));
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
          <h2>{idEditando ? "Corregir solicitud" : "Nueva solicitud"}</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Interno</label>
              <select value={idInterno} onChange={(e) => setIdInterno(e.target.value)} required disabled={!!idEditando}>
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
          <div style={{ display: "flex", gap: 10 }}>
            <button type="submit" className="boton-primario" disabled={creando}>
              {creando ? "Guardando..." : idEditando ? "Guardar corrección" : "Crear solicitud"}
            </button>
            {idEditando && <button type="button" className="boton-secundario" onClick={cancelarEdicion}>No corregir</button>}
          </div>
        </form>
      )}

      {aviso && (
        <p className={aviso.correoEnviado ? "aviso-exito" : "aviso-alerta"}>{aviso.mensaje}</p>
      )}

      <FiltrosTabla tabla={tabla} placeholder="Interno, motivo o especialista" estados={ESTADOS} etiquetaFecha="Creada" />
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
            {(esAdministrador || esMedicoGeneral) && <th></th>}
          </tr>
        </thead>
        <tbody>
          {tabla.filas.map((s) => (
            <tr key={s.idSolicitud}>
              <td>{s.nombreInterno}</td>
              <td>{s.motivo}</td>
              <td>{s.especialidadRequerida || "—"}</td>
              <td>{new Date(s.fechaCreacion).toLocaleDateString()}</td>
              <td><Badge texto={s.estado} /></td>
              <td>{s.nombreEspecialista ? `${s.nombreEspecialista}${s.especialidadEspecialista ? ` (${s.especialidadEspecialista})` : ""}` : "—"}</td>
              <td>{s.fechaHoraCita ? new Date(s.fechaHoraCita).toLocaleString() : "—"}</td>
              {esAdministrador && (
                <td className="acciones-fila">
                  {s.estado === "Pendiente" && (
                    <button className="boton-secundario" onClick={() => abrirAsignacion(s)}>
                      Asignar
                    </button>
                  )}
                  {s.estado === "Asignada" && (
                    <button className="boton-secundario" onClick={() => abrirAsignacion(s)}>
                      Reprogramar
                    </button>
                  )}
                  {(s.estado === "Pendiente" || s.estado === "Asignada") && (
                    <button className="boton-secundario boton-peligro" onClick={() => manejarCancelar(s)}>
                      Cancelar
                    </button>
                  )}
                </td>
              )}
              {esMedicoGeneral && (
                <td className="acciones-fila">
                  {s.estado === "Pendiente" && (
                    <>
                      <button className="boton-secundario" onClick={() => abrirEdicion(s)}>Corregir</button>
                      <button className="boton-secundario boton-peligro" onClick={() => manejarCancelar(s)}>Cancelar</button>
                    </>
                  )}
                </td>
              )}
            </tr>
          ))}
          {tabla.filas.length === 0 && (
            <tr>
              <td colSpan={esAdministrador || esMedicoGeneral ? 8 : 7}>{tabla.total === 0 ? "No hay solicitudes todavía." : "Ninguna solicitud coincide con la búsqueda."}</td>
            </tr>
          )}
        </tbody>
      </table>
      <PaginacionTabla tabla={tabla} nombre="solicitudes" />

      {solicitudEnAsignacion && (
        <form onSubmit={manejarAsignar} className="tarjeta-formulario">
          <h2>{solicitudes.find((x) => x.idSolicitud === solicitudEnAsignacion)?.estado === "Asignada" ? "Reprogramar cita" : "Asignar médico y horario"}</h2>
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
