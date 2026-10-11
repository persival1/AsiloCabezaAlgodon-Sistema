import { useEffect, useState } from "react";
import { listarCobros, crearCobro } from "../Servicios/cobrosServicio";
import { listarInternos } from "../Servicios/internosServicio";
import { listarVisitasPorInterno } from "../Servicios/visitasServicio";

function Cobros() {
  const [cobros, setCobros] = useState([]);
  const [internos, setInternos] = useState([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [idInterno, setIdInterno] = useState("");
  const [concepto, setConcepto] = useState("Cuota mensual");
  const [monto, setMonto] = useState("");
  const [visitasDelInterno, setVisitasDelInterno] = useState([]);
  const [idVisitaMedica, setIdVisitaMedica] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    cargar();
    listarInternos().then(setInternos);
  }, []);

  useEffect(() => {
    if (concepto === "Cargo médico" && idInterno) {
      listarVisitasPorInterno(idInterno).then(setVisitasDelInterno);
    } else {
      setVisitasDelInterno([]);
      setIdVisitaMedica("");
    }
  }, [concepto, idInterno]);

  async function cargar() {
    const datos = await listarCobros();
    setCobros(datos);
  }

  async function manejarEnvio(evento) {
    evento.preventDefault();
    setError("");
    const interno = internos.find((i) => i.idInterno === Number(idInterno));

    if (!interno?.idFamiliar) {
      setError("Ese interno no tiene familiar responsable registrado.");
      return;
    }

    setGuardando(true);
    try {
      await crearCobro(interno.idFamiliar, concepto, Number(monto), idVisitaMedica ? Number(idVisitaMedica) : null);
      setIdInterno("");
      setMonto("");
      setIdVisitaMedica("");
      setMostrarFormulario(false);
      await cargar();
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Cobros a familiares</h1>
          <p className="subtitulo">Cuotas mensuales y cargos médicos cobrados a cada familiar</p>
        </div>
        <button className="boton-primario" onClick={() => setMostrarFormulario((v) => !v)}>
          {mostrarFormulario ? "Cancelar" : "Nuevo cobro"}
        </button>
      </div>

      {mostrarFormulario && (
        <form onSubmit={manejarEnvio} className="tarjeta-formulario">
          <h2>Registrar cobro</h2>
          <div className="fila-campos">
            <div className="campo">
              <label>Interno</label>
              <select value={idInterno} onChange={(e) => setIdInterno(e.target.value)} required>
                <option value="">Seleccioná un interno</option>
                {internos.map((i) => (
                  <option key={i.idInterno} value={i.idInterno}>
                    {i.nombres} {i.apellidos} {i.nombreFamiliar ? `(${i.nombreFamiliar})` : "(sin familiar)"}
                  </option>
                ))}
              </select>
            </div>
            <div className="campo">
              <label>Concepto</label>
              <select value={concepto} onChange={(e) => setConcepto(e.target.value)}>
                <option value="Cuota mensual">Cuota mensual</option>
                <option value="Cargo médico">Cargo médico</option>
              </select>
            </div>
          </div>
          {concepto === "Cargo médico" && (
            <div className="campo">
              <label>Visita a la que corresponde el cargo</label>
              <select value={idVisitaMedica} onChange={(e) => setIdVisitaMedica(e.target.value)} required>
                <option value="">Seleccioná una visita</option>
                {visitasDelInterno.map((v) => (
                  <option key={v.idVisitaMedica} value={v.idVisitaMedica}>
                    {new Date(v.fechaVisita).toLocaleDateString()} — {v.diagnostico} (cargos: Q{v.totalCargos.toFixed(2)}, pagado: Q{v.totalCobrado.toFixed(2)})
                  </option>
                ))}
              </select>
            </div>
          )}
          <div className="campo">
            <label>Monto (Q)</label>
            <input type="number" step="0.01" value={monto} onChange={(e) => setMonto(e.target.value)} required />
          </div>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Registrar cobro"}
          </button>
        </form>
      )}

      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Familiar</th>
            <th>Interno</th>
            <th>Concepto</th>
            <th>Monto</th>
            <th>Saldo pendiente actual</th>
          </tr>
        </thead>
        <tbody>
          {cobros.map((c) => (
            <tr key={c.idCobro}>
              <td>{new Date(c.fecha).toLocaleDateString()}</td>
              <td>{c.nombreFamiliar}</td>
              <td>{c.nombreInterno}</td>
              <td>{c.concepto}</td>
              <td>Q{c.monto.toFixed(2)}</td>
              <td style={{ color: c.saldoPendienteFamiliar > 0 ? "var(--color-peligro)" : "var(--color-exito)", fontWeight: 600 }}>
                Q{c.saldoPendienteFamiliar.toFixed(2)}
              </td>
            </tr>
          ))}
          {cobros.length === 0 && (
            <tr>
              <td colSpan="6">No hay cobros registrados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Cobros;
