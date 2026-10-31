import { useEffect, useState } from "react";
import { listarPrescripciones, despacharPrescripcion } from "../Servicios/prescripcionesServicio";
import { mensajeDeError } from "../Servicios/api";
import Badge from "../componentes/Badge";

function Prescripciones() {
  const [prescripciones, setPrescripciones] = useState([]);
  const [idEnEdicion, setIdEnEdicion] = useState(null);
  const [costo, setCosto] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");

  async function cargar() {
    const datos = await listarPrescripciones();
    setPrescripciones(datos);
  }

  useEffect(() => {
    listarPrescripciones().then(setPrescripciones);
  }, []);

  function abrirDespacho(prescripcion) {
    setIdEnEdicion(prescripcion.idPrescripcion);
    setCosto("");
    setError("");
  }

  async function guardarDespacho(evento) {
    evento.preventDefault();
    setError("");
    setGuardando(true);
    try {
      await despacharPrescripcion(idEnEdicion, Number(costo));
      setIdEnEdicion(null);
      await cargar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo despachar el medicamento."));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Prescripciones</h1>
          <p className="subtitulo">Despachá el medicamento y registrá el costo con descuento</p>
        </div>
      </div>
      <table className="tabla">
        <thead>
          <tr>
            <th>Interno</th>
            <th>Medicamento</th>
            <th>Dosis</th>
            <th>Indicaciones</th>
            <th>Estado</th>
            <th>Costo</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {prescripciones.map((p) => (
            <tr key={p.idPrescripcion}>
              <td>{p.nombreInterno}</td>
              <td>{p.nombreMedicamento}</td>
              <td>{p.dosis}</td>
              <td>{p.indicaciones}</td>
              <td><Badge texto={p.estado === "Pendiente" ? "Pendiente" : "Despachado"} estado={p.estado} /></td>
              <td>{p.costo != null ? `Q${p.costo}` : "—"}</td>
              <td>
                {p.estado === "Pendiente" && (
                  <button className="boton-secundario" onClick={() => abrirDespacho(p)}>
                    Despachar
                  </button>
                )}
              </td>
            </tr>
          ))}
          {prescripciones.length === 0 && (
            <tr>
              <td colSpan="7">No hay prescripciones todavía.</td>
            </tr>
          )}
        </tbody>
      </table>

      {idEnEdicion && (
        <form onSubmit={guardarDespacho} className="tarjeta-formulario">
          <h2>Despachar medicamento</h2>
          <div className="campo">
            <label>Costo (Q)</label>
            <input type="number" step="0.01" min="0" value={costo} onChange={(e) => setCosto(e.target.value)} required />
          </div>
          {error && <p className="error">{error}</p>}
          <div className="fila-campos">
            <button type="submit" className="boton-primario" disabled={guardando}>
              {guardando ? "Guardando..." : "Confirmar despacho"}
            </button>
            <button type="button" className="boton-secundario" onClick={() => setIdEnEdicion(null)}>
              Cancelar
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

export default Prescripciones;
