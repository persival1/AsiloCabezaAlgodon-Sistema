import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { obtenerInterno, agregarPsicopatologia, agregarMedicamentoCajon } from "../Servicios/internosServicio";
import { listarVisitasPorInterno } from "../Servicios/visitasServicio";
import Badge from "../componentes/Badge";

const HOY = new Date().toISOString().slice(0, 10);

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

  useEffect(() => {
    cargar();
  }, [id]);

  async function cargar() {
    setCargando(true);
    try {
      const [datosInterno, datosVisitas] = await Promise.all([
        obtenerInterno(id),
        listarVisitasPorInterno(id),
      ]);
      setInterno(datosInterno);
      setVisitas(datosVisitas);
    } finally {
      setCargando(false);
    }
  }

  async function guardarPsicopatologia(evento) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await agregarPsicopatologia(id, nombrePsico, fechaPsico);
      setNombrePsico("");
      setMostrarPsico(false);
      await cargar();
    } finally {
      setGuardando(false);
    }
  }

  async function guardarMedicamento(evento) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await agregarMedicamentoCajon(id, medicamento.nombre, medicamento.dosis, medicamento.frecuencia);
      setMedicamento({ nombre: "", dosis: "", frecuencia: "" });
      setMostrarMedicamento(false);
      await cargar();
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
            <p>Cuota mensual: Q{interno.familiar.cuotaMensual.toFixed(2)} — Saldo pendiente: Q{interno.familiar.saldoPendiente.toFixed(2)}</p>
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
              <input type="date" value={fechaPsico} onChange={(e) => setFechaPsico(e.target.value)} required />
            </div>
            <button type="submit" className="boton-primario" disabled={guardando}>Guardar</button>
          </form>
        )}

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
