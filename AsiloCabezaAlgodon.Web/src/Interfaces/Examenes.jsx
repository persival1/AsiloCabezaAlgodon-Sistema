import { useEffect, useState } from "react";
import { listarExamenes, registrarResultado } from "../Servicios/examenesServicio";
import Badge from "../componentes/Badge";

function Examenes() {
  const [examenes, setExamenes] = useState([]);
  const [idEnEdicion, setIdEnEdicion] = useState(null);
  const [resultado, setResultado] = useState("");
  const [costo, setCosto] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    cargar();
  }, []);

  async function cargar() {
    const datos = await listarExamenes();
    setExamenes(datos);
  }

  function abrirResultado(examen) {
    setIdEnEdicion(examen.idExamenSolicitado);
    setResultado("");
    setCosto("");
  }

  async function guardarResultado(evento) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await registrarResultado(idEnEdicion, resultado, Number(costo) || 0);
      setIdEnEdicion(null);
      await cargar();
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Exámenes solicitados</h1>
          <p className="subtitulo">Registrá el resultado y el costo con descuento ya aplicado</p>
        </div>
      </div>
      <table className="tabla">
        <thead>
          <tr>
            <th>Interno</th>
            <th>Examen</th>
            <th>Fecha de visita</th>
            <th>Estado</th>
            <th>Resultado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {examenes.map((e) => (
            <tr key={e.idExamenSolicitado}>
              <td>{e.nombreInterno}</td>
              <td>{e.nombreExamen}</td>
              <td>{new Date(e.fechaVisita).toLocaleDateString()}</td>
              <td><Badge texto={e.estado === "Pendiente" ? "Pendiente" : "Resultado registrado"} estado={e.estado} /></td>
              <td>{e.resultado || "—"}</td>
              <td>
                {e.estado === "Pendiente" && (
                  <button className="boton-secundario" onClick={() => abrirResultado(e)}>
                    Registrar resultado
                  </button>
                )}
              </td>
            </tr>
          ))}
          {examenes.length === 0 && (
            <tr>
              <td colSpan="6">No hay exámenes solicitados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>

      {idEnEdicion && (
        <form onSubmit={guardarResultado} className="tarjeta-formulario">
          <h2>Registrar resultado</h2>
          <div className="campo">
            <label>Resultado</label>
            <input value={resultado} onChange={(e) => setResultado(e.target.value)} required />
          </div>
          <div className="campo">
            <label>Costo (Q, ya con el descuento de la fundación)</label>
            <input type="number" step="0.01" value={costo} onChange={(e) => setCosto(e.target.value)} required />
          </div>
          <div className="fila-campos">
            <button type="submit" className="boton-primario" disabled={guardando}>
              {guardando ? "Guardando..." : "Guardar resultado"}
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

export default Examenes;
