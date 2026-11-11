import { useEffect, useState } from "react";
import { listarCobros, crearCobro } from "../Servicios/cobrosServicio";
import { listarInternos } from "../Servicios/internosServicio";
import { listarVisitasPorInterno } from "../Servicios/visitasServicio";
import { mensajeDeError } from "../Servicios/api";
import { reporteSaldosFamiliares } from "../Servicios/reportesServicio";
import { Saldo } from "../componentes/Saldo";

function Cobros() {
  const [cobros, setCobros] = useState([]);
  const [saldos, setSaldos] = useState([]);
  const [internos, setInternos] = useState([]);
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [idInterno, setIdInterno] = useState("");
  const [concepto, setConcepto] = useState("Cuota mensual");
  const [monto, setMonto] = useState("");
  const [visitasDelInterno, setVisitasDelInterno] = useState([]);
  const [idVisitaMedica, setIdVisitaMedica] = useState("");
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState("");

  async function cargar() {
    const [datos, s] = await Promise.all([listarCobros(), reporteSaldosFamiliares()]);
    setCobros(datos);
    setSaldos(s);
  }

  useEffect(() => {
    listarCobros().then(setCobros);
    reporteSaldosFamiliares().then(setSaldos);
    listarInternos().then(setInternos);
  }, []);

  // Al cambiar el interno o el concepto se limpia la visita elegida y, si es un cargo
  // médico, se traen las visitas de ese interno.
  function cambiarSeleccion(nuevoIdInterno, nuevoConcepto) {
    setIdInterno(nuevoIdInterno);
    setConcepto(nuevoConcepto);
    setIdVisitaMedica("");
    setVisitasDelInterno([]);
    if (nuevoConcepto === "Cargo médico" && nuevoIdInterno) {
      listarVisitasPorInterno(nuevoIdInterno).then(setVisitasDelInterno);
    }
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
      cambiarSeleccion("", concepto);
      setMonto("");
      setMostrarFormulario(false);
      await cargar();
    } catch (err) {
      setError(mensajeDeError(err, "No se pudo registrar el cobro."));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Cobros a familiares</h1>
          <p className="subtitulo">Cuotas mensuales y pagos de cargos médicos de cada familiar</p>
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
              <select value={idInterno} onChange={(e) => cambiarSeleccion(e.target.value, concepto)} required>
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
              <select value={concepto} onChange={(e) => cambiarSeleccion(idInterno, e.target.value)}>
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
            <input type="number" step="0.01" min="0.01" value={monto} onChange={(e) => setMonto(e.target.value)} required />
          </div>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="boton-primario" disabled={guardando}>
            {guardando ? "Guardando..." : "Registrar cobro"}
          </button>
        </form>
      )}

      <h2 className="titulo-seccion">Saldo de cargos médicos por familiar</h2>
      <p className="nota-tabla">
        Saldo = consultas, exámenes y medicamentos cargados − pagos de esas visitas. La cuota mensual paga la estadía
        y no cambia este saldo. <span className="saldo-debe">Por pagar</span> = el familiar debe ·{" "}
        <span className="saldo-al-dia">Al día</span> = no debe nada · <span className="saldo-favor">A favor</span> = pagó de más.
      </p>
      <table className="tabla">
        <thead>
          <tr>
            <th>Familiar</th>
            <th>Interno</th>
            <th>Cargos médicos</th>
            <th>Pagado</th>
            <th>Saldo</th>
          </tr>
        </thead>
        <tbody>
          {saldos.map((s) => (
            <tr key={s.idFamiliar}>
              <td>{s.nombreFamiliar}</td>
              <td>{s.nombreInterno}</td>
              <td>Q{s.totalCargos.toFixed(2)}</td>
              <td>Q{s.totalCobros.toFixed(2)}</td>
              <td><Saldo valor={s.saldoGuardado} /></td>
            </tr>
          ))}
          {saldos.length === 0 && (
            <tr>
              <td colSpan="5">No hay familiares registrados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>

      <h2 className="titulo-seccion">Cobros registrados</h2>
      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Familiar</th>
            <th>Interno</th>
            <th>Concepto</th>
            <th>Monto</th>
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
            </tr>
          ))}
          {cobros.length === 0 && (
            <tr>
              <td colSpan="5">No hay cobros registrados todavía.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Cobros;
