import { useEffect, useState } from "react";
import { listarPagosFundacion, registrarPagoFundacion } from "../Servicios/pagosFundacionServicio";
import { reportePagosFundacion } from "../Servicios/reportesServicio";
import { mensajeDeError } from "../Servicios/api";

// AC-07: el asilo registra lo que le paga a la fundación por consultas, exámenes y
// medicamentos. Lo adeudado baja con cada pago y no se puede pagar de más.
function PagosFundacion() {
  const [pagos, setPagos] = useState([]);
  const [resumen, setResumen] = useState(null);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [monto, setMonto] = useState("");
  const [numeroComprobante, setNumeroComprobante] = useState("");
  const [descripcion, setDescripcion] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");
  const [aviso, setAviso] = useState("");

  async function cargar() {
    const [p, r] = await Promise.all([listarPagosFundacion(), reportePagosFundacion()]);
    setPagos(p);
    setResumen(r);
  }

  useEffect(() => {
    listarPagosFundacion().then(setPagos);
    reportePagosFundacion().then(setResumen);
  }, []);

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setError("");
    setAviso("");
    setGuardando(true);
    try {
      await registrarPagoFundacion(Number(monto), numeroComprobante, descripcion);
      setAviso(`Pago de Q${Number(monto).toFixed(2)} registrado.`);
      setMonto("");
      setNumeroComprobante("");
      setDescripcion("");
      setMostrarFormulario(false);
      await cargar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo registrar el pago."));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Pagos a la fundación</h1>
          <p className="subtitulo">Lo que el asilo le debe a la fundación por consultas, exámenes y medicamentos, y lo que ya le pagó</p>
        </div>
        <button className="boton-primario" onClick={() => setMostrarFormulario((v) => !v)}>
          {mostrarFormulario ? "Cancelar" : "Registrar pago"}
        </button>
      </div>

      {resumen && (
        <div className="tarjetas-stat">
          <div className="tarjeta-stat">
            <div className="stat-valor">Q{resumen.adeudadoAcumulado.toFixed(2)}</div>
            <div className="stat-etiqueta">Cargos médicos hasta hoy</div>
          </div>
          <div className="tarjeta-stat">
            <div className="stat-valor">Q{resumen.pagadoAcumulado.toFixed(2)}</div>
            <div className="stat-etiqueta">Pagado a la fundación</div>
          </div>
          <div className="tarjeta-stat">
            <div className={`stat-valor ${resumen.saldoPendiente > 0 ? "saldo-debe" : "saldo-al-dia"}`}>Q{resumen.saldoPendiente.toFixed(2)}</div>
            <div className="stat-etiqueta">Pendiente de pagar</div>
          </div>
        </div>
      )}

      {aviso && <p className="exito">{aviso}</p>}

      {mostrarFormulario && (
        <form onSubmit={manejarEnvio} className="tarjeta-formulario">
          <h2>Registrar pago</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Monto (Q) <span className="obligatorio">*</span></label>
              <input type="number" step="0.01" min="0.01" max={resumen ? resumen.saldoPendiente : undefined} value={monto} onChange={(e) => setMonto(e.target.value)} required />
              {resumen && <small className="ayuda-campo">Máximo Q{resumen.saldoPendiente.toFixed(2)}, lo que falta pagar.</small>}
            </div>
            <div className="campo">
              <label>Número de comprobante <span className="obligatorio">*</span></label>
              <input value={numeroComprobante} onChange={(e) => setNumeroComprobante(e.target.value)} placeholder="Recibo o boleta de la fundación" required />
            </div>
          </div>
          <div className="campo">
            <label>Descripción</label>
            <input value={descripcion} onChange={(e) => setDescripcion(e.target.value)} placeholder="Por ejemplo: cargos médicos de octubre" />
          </div>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Registrar pago"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Comprobante</th>
            <th>Descripción</th>
            <th>Monto</th>
          </tr>
        </thead>
        <tbody>
          {pagos.map((p) => (
            <tr key={p.idPagoFundacion}>
              <td>{new Date(p.fecha).toLocaleDateString()}</td>
              <td>{p.numeroComprobante}</td>
              <td>{p.descripcion || "—"}</td>
              <td>Q{p.monto.toFixed(2)}</td>
            </tr>
          ))}
          {pagos.length === 0 && (
            <tr>
              <td colSpan="4">Todavía no se ha registrado ningún pago a la fundación.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default PagosFundacion;
