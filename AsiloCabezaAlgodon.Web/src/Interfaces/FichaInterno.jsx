import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { obtenerInterno, agregarPsicopatologia, agregarMedicamentoCajon } from "../Servicios/internosServicio";
import { listarVisitasPorInterno } from "../Servicios/visitasServicio";
import { mensajeDeError } from "../Servicios/api";
import Badge from "../componentes/Badge";
import { Saldo } from "../componentes/Saldo";

const HOY = new Date().toISOString().slice(0, 10);

// Trae el interno y sus visitas al mismo tiempo
function obtenerFicha(id) {
  return Promise.all([obtenerInterno(id), listarVisitasPorInterno(id)]);
}

function FichaInterno() {
  const { id } = useParams();
  const { rol } = useAutenticacion();
  const puedeAgregarFicha = ["Administrador", "MedicoGeneral", "Especialista"].includes(rol);

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
      await agregarPsicopatologia(id, nombrePsico, fechaPsico);
      setNombrePsico("");
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
      await agregarMedicamentoCajon(id, medicamento.nombre, medicamento.dosis, medicamento.frecuencia);
      setMedicamento({ nombre: "", dosis: "", frecuencia: "" });
      setMostrarMedicamento(false);
      await cargar();
    } catch (err) {
      setErrorMedicamento(mensajeDeError(err, "No se pudo guardar el medicamento."));
    } finally {
      setGuardando(false);
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
      </div>

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
            <button className="boton-secundario" onClick={() => setMostrarPsico((v) => !v)}>
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
            <li key={p.idPsicopatologia}>{p.nombre} — diagnosticada el {new Date(p.fechaDiagnostico).toLocaleDateString()}</li>
          ))}
        </ul>
      </div>

      <div className="tarjeta-formulario">
        <div className="encabezado-seccion" style={{ marginBottom: 10 }}>
          <h2 style={{ margin: 0 }}>Medicamentos de cajón</h2>
          {puedeAgregarFicha && (
            <button className="boton-secundario" onClick={() => setMostrarMedicamento((v) => !v)}>
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
            <li key={m.idMedicamentoCajon}>{m.nombre} — {m.dosis}, {m.frecuencia}</li>
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
