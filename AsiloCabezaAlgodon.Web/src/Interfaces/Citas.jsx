import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listarSolicitudes } from "../Servicios/solicitudesServicio";
import { registrarVisita } from "../Servicios/visitasServicio";
import Badge from "../componentes/Badge";

const FORM_VACIO = { diagnostico: "", observaciones: "", costoConsulta: "" };

function Citas() {
  const [solicitudes, setSolicitudes] = useState([]);
  const [idSolicitudEnVisita, setIdSolicitudEnVisita] = useState(null);
  const [form, setForm] = useState(FORM_VACIO);
  const [examenes, setExamenes] = useState([]);
  const [prescripciones, setPrescripciones] = useState([]);
  const [nuevoExamen, setNuevoExamen] = useState("");
  const [nuevoMedicamento, setNuevoMedicamento] = useState({ nombreMedicamento: "", dosis: "", indicaciones: "" });
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    cargarSolicitudes();
  }, []);

  async function cargarSolicitudes() {
    const datos = await listarSolicitudes();
    setSolicitudes(datos);
  }

  function abrirRegistroVisita(idSolicitud) {
    setIdSolicitudEnVisita(idSolicitud);
    setForm(FORM_VACIO);
    setExamenes([]);
    setPrescripciones([]);
    setError("");
  }

  function agregarExamen() {
    if (!nuevoExamen.trim()) return;
    setExamenes((prev) => [...prev, { nombreExamen: nuevoExamen }]);
    setNuevoExamen("");
  }

  function quitarExamen(indice) {
    setExamenes((prev) => prev.filter((_, i) => i !== indice));
  }

  function agregarMedicamento() {
    if (!nuevoMedicamento.nombreMedicamento.trim()) return;
    setPrescripciones((prev) => [...prev, nuevoMedicamento]);
    setNuevoMedicamento({ nombreMedicamento: "", dosis: "", indicaciones: "" });
  }

  function quitarMedicamento(indice) {
    setPrescripciones((prev) => prev.filter((_, i) => i !== indice));
  }

  async function manejarGuardarVisita(evento) {
    evento.preventDefault();
    setError("");
    setGuardando(true);
    try {
      await registrarVisita(idSolicitudEnVisita, form.diagnostico, form.observaciones, Number(form.costoConsulta) || 0, examenes, prescripciones);
      setIdSolicitudEnVisita(null);
      await cargarSolicitudes();
    } catch (err) {
      setError("No se pudo registrar la visita.");
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Mis citas</h1>
          <p className="subtitulo">Solicitudes que la fundación te asignó</p>
        </div>
      </div>
      <table className="tabla">
        <thead>
          <tr>
            <th>Interno</th>
            <th>Motivo</th>
            <th>Estado</th>
            <th>Fecha y hora de cita</th>
            <th>Enfermero(a) acompañante</th>
            <th></th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {solicitudes.map((s) => (
            <tr key={s.idSolicitud}>
              <td>{s.nombreInterno}</td>
              <td>{s.motivo}</td>
              <td><Badge texto={s.estado} /></td>
              <td>{s.fechaHoraCita ? new Date(s.fechaHoraCita).toLocaleString() : "—"}</td>
              <td>{s.nombreEnfermero || "—"}</td>
              <td>
                <Link to={`/dashboard/internos/${s.idInterno}`} className="boton-secundario" style={{ textDecoration: "none", display: "inline-block" }}>
                  Ver ficha
                </Link>
              </td>
              <td>
                {s.estado === "Asignada" && (
                  <button className="boton-secundario" onClick={() => abrirRegistroVisita(s.idSolicitud)}>
                    Registrar visita
                  </button>
                )}
              </td>
            </tr>
          ))}
          {solicitudes.length === 0 && (
            <tr>
              <td colSpan="7">No tenés citas asignadas.</td>
            </tr>
          )}
        </tbody>
      </table>

      {idSolicitudEnVisita && (
        <form onSubmit={manejarGuardarVisita} className="tarjeta-formulario">
          <h2>Registrar visita médica</h2>
          <div className="campo">
            <label>Diagnóstico</label>
            <input
              value={form.diagnostico}
              onChange={(e) => setForm((f) => ({ ...f, diagnostico: e.target.value }))}
              required
            />
          </div>
          <div className="campo">
            <label>Observaciones</label>
            <input
              value={form.observaciones}
              onChange={(e) => setForm((f) => ({ ...f, observaciones: e.target.value }))}
            />
          </div>
          <div className="campo">
            <label>Costo de la consulta (Q, ya con el descuento de la fundación)</label>
            <input
              type="number"
              step="0.01"
              value={form.costoConsulta}
              onChange={(e) => setForm((f) => ({ ...f, costoConsulta: e.target.value }))}
              required
            />
          </div>

          <h2>Exámenes de laboratorio a solicitar</h2>
          <div className="fila-campos">
            <div className="campo">
              <input
                placeholder="Nombre del examen"
                value={nuevoExamen}
                onChange={(e) => setNuevoExamen(e.target.value)}
              />
            </div>
            <button type="button" className="boton-secundario" onClick={agregarExamen}>Agregar</button>
          </div>
          <ul>
            {examenes.map((ex, i) => (
              <li key={i}>
                {ex.nombreExamen}{" "}
                <button type="button" className="boton-secundario" onClick={() => quitarExamen(i)}>Quitar</button>
              </li>
            ))}
          </ul>

          <h2>Medicamentos a recetar</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Medicamento</label>
              <input
                value={nuevoMedicamento.nombreMedicamento}
                onChange={(e) => setNuevoMedicamento((m) => ({ ...m, nombreMedicamento: e.target.value }))}
              />
            </div>
            <div className="campo">
              <label>Dosis</label>
              <input
                value={nuevoMedicamento.dosis}
                onChange={(e) => setNuevoMedicamento((m) => ({ ...m, dosis: e.target.value }))}
              />
            </div>
            <div className="campo">
              <label>Indicaciones</label>
              <input
                value={nuevoMedicamento.indicaciones}
                onChange={(e) => setNuevoMedicamento((m) => ({ ...m, indicaciones: e.target.value }))}
              />
            </div>
          </div>
          <button type="button" className="boton-secundario" onClick={agregarMedicamento}>Agregar medicamento</button>
          <ul>
            {prescripciones.map((p, i) => (
              <li key={i}>
                {p.nombreMedicamento} — {p.dosis} ({p.indicaciones}){" "}
                <button type="button" className="boton-secundario" onClick={() => quitarMedicamento(i)}>Quitar</button>
              </li>
            ))}
          </ul>

          {error && <p className="error">{error}</p>}

          <div className="fila-campos">
            <button type="submit" className="boton-primario" disabled={guardando}>
              {guardando ? "Guardando..." : "Guardar visita"}
            </button>
            <button type="button" className="boton-secundario" onClick={() => setIdSolicitudEnVisita(null)}>
              Cancelar
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

export default Citas;
