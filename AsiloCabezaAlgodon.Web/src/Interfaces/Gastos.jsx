import { useEffect, useState } from "react";
import { listarGastos, crearGasto } from "../Servicios/gastosServicio";

function Gastos() {
  const [gastos, setGastos] = useState([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [concepto, setConcepto] = useState("");
  const [monto, setMonto] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    cargar();
  }, []);

  async function cargar() {
    const datos = await listarGastos();
    setGastos(datos);
  }

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await crearGasto(concepto, Number(monto));
      setConcepto("");
      setMonto("");
      setMostrarFormulario(false);
      await cargar();
    } finally {
      setGuardando(false);
    }
  }

  const total = gastos.reduce((suma, g) => suma + g.monto, 0);

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Gastos</h1>
          <p className="subtitulo">Egresos operativos del asilo (luz, agua, insumos, etc.)</p>
        </div>
        <button className="boton-primario" onClick={() => setMostrarFormulario((v) => !v)}>
          {mostrarFormulario ? "Cancelar" : "Nuevo gasto"}
        </button>
      </div>

      <div className="tarjetas-stat">
        <div className="tarjeta-stat">
          <div className="stat-valor">Q{total.toFixed(2)}</div>
          <div className="stat-etiqueta">Total de gastos registrados</div>
        </div>
      </div>

      {mostrarFormulario && (
        <form onSubmit={manejarEnvio} className="tarjeta-formulario">
          <h2>Nuevo gasto</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Concepto</label>
              <input value={concepto} onChange={(e) => setConcepto(e.target.value)} required />
            </div>
            <div className="campo">
              <label>Monto (Q)</label>
              <input type="number" step="0.01" value={monto} onChange={(e) => setMonto(e.target.value)} required />
            </div>
          </div>
          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Registrar gasto"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Concepto</th>
            <th>Monto</th>
          </tr>
        </thead>
        <tbody>
          {gastos.map((g) => (
            <tr key={g.idGasto}>
              <td>{new Date(g.fecha).toLocaleDateString()}</td>
              <td>{g.concepto}</td>
              <td>Q{g.monto.toFixed(2)}</td>
            </tr>
          ))}
          {gastos.length === 0 && (
            <tr>
              <td colSpan="3">No hay gastos registrados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Gastos;
