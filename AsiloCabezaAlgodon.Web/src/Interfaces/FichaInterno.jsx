import { fechaCorta } from "../utilidades/tabla";
import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import {
  obtenerInterno, agregarPsicopatologia, agregarMedicamentoCajon, cambiarEstadoInterno,
  editarPsicopatologia, eliminarPsicopatologia, editarMedicamentoCajon, eliminarMedicamentoCajon,
} from "../Servicios/internosServicio";
import { listarVisitasPorInterno } from "../Servicios/visitasServicio";
import { mensajeDeError } from "../Servicios/api";
import Badge from "../componentes/Badge";
import { Saldo } from "../componentes/Saldo";
import EditarInterno from "../componentes/EditarInterno";

// AC-16: fecha local (toISOString da la fecha UTC: después de las 18:00 en Guatemala ya es "mañana")
const HOY = fechaCorta(new Date());

// Trae el interno y sus visitas al mismo tiempo
function obtenerFicha(id) {
  return Promise.all([obtenerInterno(id), listarVisitasPorInterno(id)]);
}

function FichaInterno() {
  const { id } = useParams();
  const { rol } = useAutenticacion();
  const puedeAgregarFicha = ["Administrador", "MedicoGeneral", "Especialista"].includes(rol);
  // AC-08: editar datos (Administrador y médico general) y alta o baja (Administrador)
  const puedeEditarDatos = ["Administrador", "MedicoGeneral"].includes(rol);
  const puedeDarDeBaja = rol === "Administrador";
  const [editandoDatos, setEditandoDatos] = useState(false);
  const [avisoFicha, setAvisoFicha] = useState("");
  const [errorFicha, setErrorFicha] = useState("");
  // Registro de la ficha que se está corrigiendo (null = se agrega uno nuevo)
  const [idPsicoEditada, setIdPsicoEditada] = useState(null);
  const [idMedicamentoEditado, setIdMedicamentoEditado] = useState(null);

  const [interno, setInterno] = useState(null);
  const [visitas, setVisitas] = useState([]);
  const [cargando, setCargando] = useState(true);

  const [mostrarPsico, setMostrarPsico] = useState(false);
  const [nombrePsico, setNombrePsico] = useState("");
  const [fechaPsico, setFechaPsico] = useState(HOY);

  const [mostrarMedicamento, setMostrarMedicamento] = useState(false);
  const [medicamento, setMedicamento] = useState({ nombre: "", dosis: "", frecuencia: "" });

  const [guardando, setGuardando] = useState(false);
  // Antes, si guardar fallaba, la pantalla no mostraba nada. Ahora muestra el motivo.
  const [errorPsico, setErrorPsico] = useState("");
  const [errorMedicamento, setErrorMedicamento] = useState("");

  function mostrarFicha([datosInterno, datosVisitas]) {
    setInterno(datosInterno);
    setVisitas(datosVisitas);
  }

  useEffect(() => {
    obtenerFicha(id).then(mostrarFicha).finally(() => setCargando(false));
  }, [id]);

  // Para recargar la ficha después de agregar una psicopatología o un medicamento
  async function cargar() {
    mostrarFicha(await obtenerFicha(id));
  }

  async function guardarPsicopatologia(evento) {
    evento.preventDefault();
    setErrorPsico("");
    setGuardando(true);
    try {
      if (idPsicoEditada) await editarPsicopatologia(id, idPsicoEditada, nombrePsico, fechaPsico);
      else await agregarPsicopatologia(id, nombrePsico, fechaPsico);
      setNombrePsico("");
      setIdPsicoEditada(null);
      setMostrarPsico(false);
      await cargar();
    } catch (err) {
      setErrorPsico(mensajeDeError(err, "No se pudo guardar la psicopatología."));
    } finally {
      setGuardando(false);
    }
  }

  async function guardarMedicamento(evento) {
    evento.preventDefault();
    setErrorMedicamento("");
    setGuardando(true);
    try {
      if (idMedicamentoEditado) await editarMedicamentoCajon(id, idMedicamentoEditado, medicamento.nombre, medicamento.dosis, medicamento.frecuencia);
      else await agregarMedicamentoCajon(id, medicamento.nombre, medicamento.dosis, medicamento.frecuencia);
      setMedicamento({ nombre: "", dosis: "", frecuencia: "" });
      setIdMedicamentoEditado(null);
      setMostrarMedicamento(false);
      await cargar();
    } catch (err) {
      setErrorMedicamento(mensajeDeError(err, "No se pudo guardar el medicamento."));
    } finally {
      setGuardando(false);
    }
  }

  function corregirPsico(p) {
    setIdPsicoEditada(p.idPsicopatologia);
    setNombrePsico(p.nombre);
    setFechaPsico(String(p.fechaDiagnostico).slice(0, 10));
    setErrorPsico("");
    setMostrarPsico(true);
  }

  function corregirMedicamento(m) {
    setIdMedicamentoEditado(m.idMedicamentoCajon);
    setMedicamento({ nombre: m.nombre, dosis: m.dosis, frecuencia: m.frecuencia });
    setErrorMedicamento("");
    setMostrarMedicamento(true);
  }

  async function quitar(descripcion, accion) {
    if (!window.confirm(`¿Quitar «${descripcion}» de la ficha? Usalo solo si se registró por error.`)) return;
    setErrorFicha("");
    try {
      await accion();
      await cargar();
    } catch (err) {
      setErrorFicha(mensajeDeError(err, "No se pudo quitar el registro."));
    }
  }

  async function alternarBaja() {
    const nuevo = interno.estado === "activo" ? "baja" : "activo";
    if (nuevo === "baja" && !window.confirm(`¿Dar de baja a ${interno.nombres} ${interno.apellidos}? Su ficha y su historial se conservan, pero ya no se le podrán crear solicitudes.`)) return;
    setErrorFicha("");
    setAvisoFicha("");
    try {
      await cambiarEstadoInterno(id, nuevo);
      setAvisoFicha(nuevo === "baja" ? "El interno quedó de baja." : "El interno quedó activo de nuevo.");
      await cargar();
    } catch (err) {
      setErrorFicha(mensajeDeError(err, "No se pudo cambiar el estado del interno."));
    }
  }

  if (cargando) return <p>Cargando ficha médica...</p>;
  if (!interno) return <p>No se encontró el interno.</p>;

  return (
    <div>
      <Link to="/dashboard/internos" className="subtitulo" style={{ display: "inline-block", marginBottom: 12, textDecoration: "none" }}>
        ← Volver a Internos
      </Link>

      <div className="encabezado-seccion">
        <div>
          <h1>{interno.nombres} {interno.apellidos}</h1>
          <p className="subtitulo">Ficha médica completa — ingreso {new Date(interno.fechaIngreso).toLocaleDateString()}</p>
        </div>
        <div className="acciones-fila">
          {puedeEditarDatos && !editandoDatos && (
            <button className="boton-secundario" onClick={() => { setAvisoFicha(""); setEditandoDatos(true); }}>Editar datos</button>
          )}
          {puedeDarDeBaja && (
            <button className={interno.estado === "activo" ? "boton-secundario boton-peligro" : "boton-secundario"} onClick={alternarBaja}>
              {interno.estado === "activo" ? "Dar de baja" : "Reactivar"}
            </button>
          )}
        </div>
      </div>

      {avisoFicha && <p className="exito">{avisoFicha}</p>}
      {errorFicha && <p className="error">{errorFicha}</p>}
      {editandoDatos && (
        <EditarInterno
          interno={interno}
          onCancelar={() => setEditandoDatos(false)}
          onGuardado={async () => { setEditandoDatos(false); setAvisoFicha("Se guardaron los datos del interno."); await cargar(); }}
        />
      )}

      <div className="tarjeta-formulario">
        <h2>Datos generales</h2>
        <p><strong>Fecha de nacimiento:</strong> {new Date(interno.fechaNacimiento).toLocaleDateString()}</p>
        <p><strong>Motivo de ingreso:</strong> {interno.motivoIngreso}</p>
        <p><strong>Estado:</strong> <Badge texto={interno.estado} /></p>

        {interno.familiar && (
          <>
            <h2>Familiar responsable</h2>
            <p><strong>{interno.familiar.nombres} {interno.familiar.apellidos}</strong> — {interno.familiar.parentesco}</p>
            <p>{interno.familiar.telefono} · {interno.familiar.correo}</p>
            {interno.familiar.cuotaMensual != null && (
              <p>Cuota mensual: Q{interno.familiar.cuotaMensual.toFixed(2)} · Saldo de cargos médicos: <Saldo valor={interno.familiar.saldoPendiente} /></p>
            )}
          </>
        )}
      </div>

      <div className="tarjeta-formulario">
        <div className="encabezado-seccion" style={{ marginBottom: 10 }}>
          <h2 style={{ margin: 0 }}>Psicopatologías</h2>
          {puedeAgregarFicha && (
            <button className="boton-secundario" onClick={() => { setIdPsicoEditada(null); setNombrePsico(""); setMostrarPsico((v) => !v); }}>
              {mostrarPsico ? "Cancelar" : "Agregar"}
            </button>
          )}
        </div>

        {mostrarPsico && (
          <form onSubmit={guardarPsicopatologia} className="fila-campos" style={{ marginBottom: 16 }}>
            <div className="campo">
              <label>Nombre</label>
              <input value={nombrePsico} onChange={(e) => setNombrePsico(e.target.value)} required />
            </div>
            <div className="campo">
              <label>Fecha de diagnóstico</label>
              <input type="date" value={fechaPsico} max={HOY} onChange={(e) => setFechaPsico(e.target.value)} required />
            </div>
            <button type="submit" className="boton-primario" disabled={guardando}>Guardar</button>
          </form>
        )}
        {mostrarPsico && errorPsico && <p className="error">{errorPsico}</p>}

        <ul>
          {interno.psicopatologias.length === 0 && <li>Ninguna registrada</li>}
          {interno.psicopatologias.map((p) => (
            <li key={p.idPsicopatologia}>
              {p.nombre} — diagnosticada el {new Date(p.fechaDiagnostico).toLocaleDateString()}
              {puedeAgregarFicha && (
                <span className="acciones-item">
                  <button type="button" className="boton-enlace" onClick={() => corregirPsico(p)}>Corregir</button>
                  <button type="button" className="boton-enlace peligro" onClick={() => quitar(p.nombre, () => eliminarPsicopatologia(id, p.idPsicopatologia))}>Quitar</button>
                </span>
              )}
            </li>
          ))}
        </ul>
      </div>

      <div className="tarjeta-formulario">
        <div className="encabezado-seccion" style={{ marginBottom: 10 }}>
          <h2 style={{ margin: 0 }}>Medicamentos de cajón</h2>
          {puedeAgregarFicha && (
            <button className="boton-secundario" onClick={() => { setIdMedicamentoEditado(null); setMedicamento({ nombre: "", dosis: "", frecuencia: "" }); setMostrarMedicamento((v) => !v); }}>
              {mostrarMedicamento ? "Cancelar" : "Agregar"}
            </button>
          )}
        </div>

        {mostrarMedicamento && (
          <form onSubmit={guardarMedicamento} className="fila-campos" style={{ marginBottom: 16 }}>
            <div className="campo">
              <label>Medicamento</label>
              <input value={medicamento.nombre} onChange={(e) => setMedicamento((m) => ({ ...m, nombre: e.target.value }))} required />
            </div>
            <div className="campo">
              <label>Dosis</label>
              <input value={medicamento.dosis} onChange={(e) => setMedicamento((m) => ({ ...m, dosis: e.target.value }))} required />
            </div>
            <div className="campo">
              <label>Frecuencia</label>
              <input value={medicamento.frecuencia} onChange={(e) => setMedicamento((m) => ({ ...m, frecuencia: e.target.value }))} required />
            </div>
            <button type="submit" className="boton-primario" disabled={guardando}>Guardar</button>
          </form>
        )}
        {mostrarMedicamento && errorMedicamento && <p className="error">{errorMedicamento}</p>}

        <ul>
          {interno.medicamentosCajon.length === 0 && <li>Ninguno registrado</li>}
          {interno.medicamentosCajon.map((m) => (
            <li key={m.idMedicamentoCajon}>
              {m.nombre} — {m.dosis}, {m.frecuencia}
              {puedeAgregarFicha && (
                <span className="acciones-item">
                  <button type="button" className="boton-enlace" onClick={() => corregirMedicamento(m)}>Corregir</button>
                  <button type="button" className="boton-enlace peligro" onClick={() => quitar(m.nombre, () => eliminarMedicamentoCajon(id, m.idMedicamentoCajon))}>Quitar</button>
                </span>
              )}
            </li>
          ))}
        </ul>
      </div>

      <div className="tarjeta-formulario">
        <h2>Historial de visitas médicas</h2>
        {visitas.length === 0 && <p>Sin visitas registradas todavía.</p>}
        {visitas.map((v) => (
          <div key={v.idVisitaMedica} style={{ borderTop: "1px solid var(--color-borde)", paddingTop: 12, marginTop: 12 }}>
            <p>
              <strong>{new Date(v.fechaVisita).toLocaleDateString()}</strong> — {v.nombreEspecialista}
              {v.especialidadEspecialista ? ` (${v.especialidadEspecialista})` : ""}
            </p>
            <p><strong>Motivo:</strong> {v.motivo}</p>
            <p><strong>Diagnóstico:</strong> {v.diagnostico}</p>
            <p><strong>Observaciones:</strong> {v.observaciones || "—"}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

export default FichaInterno;
