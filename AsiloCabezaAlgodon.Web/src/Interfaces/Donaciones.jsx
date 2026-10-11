import { useEffect, useState } from "react";
import { listarDonaciones, crearDonacion } from "../Servicios/donacionesServicio";

const TIPOS_DONANTE = ["EmpresaNacional", "EmpresaInternacional", "Gobierno", "Particular"];
const ETIQUETA_TIPO_DONANTE = {
  EmpresaNacional: "Empresa nacional",
  EmpresaInternacional: "Empresa internacional",
  Gobierno: "Gobierno",
  Particular: "Particular",
};

function Donaciones() {
  const [donaciones, setDonaciones] = useState([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [tipoDonante, setTipoDonante] = useState(TIPOS_DONANTE[0]);
  const [nombreDonante, setNombreDonante] = useState("");
  const [monto, setMonto] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    cargar();
  }, []);

  async function cargar() {
    const datos = await listarDonaciones();
    setDonaciones(datos);
  }

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setGuardando(true);
    try {
      await crearDonacion(tipoDonante, nombreDonante, Number(monto), descripcion);
      setNombreDonante("");
      setMonto("");
      setDescripcion("");
      setMostrarFormulario(false);
      await cargar();
    } finally {
      setGuardando(false);
    }
  }

  const total = donaciones.reduce((suma, d) => suma + d.monto, 0);

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Donaciones</h1>
          <p className="subtitulo">Empresas, gobierno y particulares que apoyan al asilo</p>
        </div>
        <button className="boton-primario" onClick={() => setMostrarFormulario((v) => !v)}>
          {mostrarFormulario ? "Cancelar" : "Nueva donación"}
        </button>
      </div>

      <div className="tarjetas-stat">
        <div className="tarjeta-stat">
          <div className="stat-valor">Q{total.toFixed(2)}</div>
          <div className="stat-etiqueta">Total recaudado en donaciones</div>
        </div>
      </div>

      {mostrarFormulario && (
        <form onSubmit={manejarEnvio} className="tarjeta-formulario">
          <h2>Nueva donación</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Tipo de donante</label>
              <select value={tipoDonante} onChange={(e) => setTipoDonante(e.target.value)}>
                <option value="EmpresaNacional">Empresa nacional</option>
                <option value="EmpresaInternacional">Empresa internacional</option>
                <option value="Gobierno">Gobierno</option>
                <option value="Particular">Particular</option>
              </select>
            </div>
            <div className="campo">
              <label>Nombre del donante</label>
              <input value={nombreDonante} onChange={(e) => setNombreDonante(e.target.value)} required />
            </div>
          </div>
          <div className="fila-campos">
            <div className="campo">
              <label>Monto (Q)</label>
              <input type="number" step="0.01" value={monto} onChange={(e) => setMonto(e.target.value)} required />
            </div>
            <div className="campo">
              <label>Descripción</label>
              <input value={descripcion} onChange={(e) => setDescripcion(e.target.value)} />
            </div>
          </div>
          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Registrar donación"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Tipo de donante</th>
            <th>Donante</th>
            <th>Monto</th>
            <th>Descripción</th>
          </tr>
        </thead>
        <tbody>
          {donaciones.map((d) => (
            <tr key={d.idDonacion}>
              <td>{new Date(d.fecha).toLocaleDateString()}</td>
              <td>{ETIQUETA_TIPO_DONANTE[d.tipoDonante] || d.tipoDonante}</td>
              <td>{d.nombreDonante}</td>
              <td>Q{d.monto.toFixed(2)}</td>
              <td>{d.descripcion || "—"}</td>
            </tr>
          ))}
          {donaciones.length === 0 && (
            <tr>
              <td colSpan="5">No hay donaciones registradas todavía.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Donaciones;
