import { useEffect, useState } from "react";
import { listarInternos } from "../Servicios/internosServicio";
import {
  reporteCostosPorCita,
  reporteFichaMedica,
  reporteCobrosPorPaciente,
  reportePagosFundacion,
  reporteEntradas,
  reporteExamenesPorPaciente,
  reporteMedicamentosPorPaciente,
} from "../Servicios/reportesServicio";

const REPORTES = [
  { valor: "costos-por-cita", texto: "1. Costos por cita (consulta + laboratorio + medicamentos)" },
  { valor: "ficha-medica", texto: "2. Análisis médico / ficha médica por paciente" },
  { valor: "cobros-por-paciente", texto: "3. Cobros por paciente por rango de fecha" },
  { valor: "pagos-fundacion", texto: "4. Pagos a la fundación" },
  { valor: "entradas", texto: "5. Entradas: donaciones y cobros" },
  { valor: "examenes-por-paciente", texto: "6. Exámenes médicos realizados por paciente" },
  { valor: "medicamentos-por-paciente", texto: "7. Medicamentos aplicados por paciente" },
];

const HOY = new Date().toISOString().slice(0, 10);

function Reportes() {
  const [reporteSeleccionado, setReporteSeleccionado] = useState(REPORTES[0].valor);
  const [internos, setInternos] = useState([]);
  const [idInterno, setIdInterno] = useState("");
  const [fechaInicio, setFechaInicio] = useState(HOY);
  const [fechaFin, setFechaFin] = useState(HOY);
  const [resultado, setResultado] = useState(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    listarInternos().then(setInternos);
  }, []);

  const necesitaInterno = ["ficha-medica", "cobros-por-paciente", "examenes-por-paciente", "medicamentos-por-paciente"].includes(reporteSeleccionado);
  const necesitaRango = ["cobros-por-paciente", "pagos-fundacion", "entradas"].includes(reporteSeleccionado);

  async function generar() {
    setError("");
    setResultado(null);

    if (necesitaInterno && !idInterno) {
      setError("Seleccioná un paciente.");
      return;
    }

    setCargando(true);
    try {
      let datos;
      switch (reporteSeleccionado) {
        case "costos-por-cita":
          datos = await reporteCostosPorCita();
          break;
        case "ficha-medica":
          datos = await reporteFichaMedica(idInterno);
          break;
        case "cobros-por-paciente":
          datos = await reporteCobrosPorPaciente(idInterno, fechaInicio, fechaFin);
          break;
        case "pagos-fundacion":
          datos = await reportePagosFundacion(fechaInicio, fechaFin);
          break;
        case "entradas":
          datos = await reporteEntradas(fechaInicio, fechaFin);
          break;
        case "examenes-por-paciente":
          datos = await reporteExamenesPorPaciente(idInterno);
          break;
        case "medicamentos-por-paciente":
          datos = await reporteMedicamentosPorPaciente(idInterno);
          break;
        default:
          break;
      }
      setResultado(datos);
    } catch (err) {
      setError("No se pudo generar el reporte.");
    } finally {
      setCargando(false);
    }
  }

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Reportes</h1>
          <p className="subtitulo">Los 7 informes que exige el enunciado, listos para exportar</p>
        </div>
      </div>

      <div className="tarjeta-formulario">
        <div className="fila-campos">
          <div className="campo">
            <label>Reporte</label>
            <select value={reporteSeleccionado} onChange={(e) => { setReporteSeleccionado(e.target.value); setResultado(null); }}>
              {REPORTES.map((r) => (
                <option key={r.valor} value={r.valor}>{r.texto}</option>
              ))}
            </select>
          </div>

          {necesitaInterno && (
            <div className="campo">
              <label>Paciente</label>
              <select value={idInterno} onChange={(e) => setIdInterno(e.target.value)}>
                <option value="">Seleccioná un paciente</option>
                {internos.map((i) => (
                  <option key={i.idInterno} value={i.idInterno}>{i.nombres} {i.apellidos}</option>
                ))}
              </select>
            </div>
          )}
        </div>

        {necesitaRango && (
          <div className="fila-campos">
            <div className="campo">
              <label>Desde</label>
              <input type="date" value={fechaInicio} onChange={(e) => setFechaInicio(e.target.value)} />
            </div>
            <div className="campo">
              <label>Hasta</label>
              <input type="date" value={fechaFin} onChange={(e) => setFechaFin(e.target.value)} />
            </div>
          </div>
        )}

        {error && <p className="error">{error}</p>}

        <button className="boton-primario" onClick={generar} disabled={cargando}>
          {cargando ? "Generando..." : "Generar reporte"}
        </button>
      </div>

      {resultado && (
        <div className="tarjeta-formulario">
          {reporteSeleccionado === "costos-por-cita" && <ReporteCostosPorCita datos={resultado} />}
          {reporteSeleccionado === "ficha-medica" && <ReporteFichaMedica datos={resultado} />}
          {reporteSeleccionado === "cobros-por-paciente" && <ReporteCobrosPorPaciente datos={resultado} />}
          {reporteSeleccionado === "pagos-fundacion" && <ReportePagosFundacion datos={resultado} />}
          {reporteSeleccionado === "entradas" && <ReporteEntradas datos={resultado} />}
          {reporteSeleccionado === "examenes-por-paciente" && <ReporteExamenesPorPaciente datos={resultado} />}
          {reporteSeleccionado === "medicamentos-por-paciente" && <ReporteMedicamentosPorPaciente datos={resultado} />}
        </div>
      )}
    </div>
  );
}

function ReporteCostosPorCita({ datos }) {
  return (
    <table className="tabla">
      <thead>
        <tr><th>Paciente</th><th>Fecha</th><th>Consulta</th><th>Exámenes</th><th>Medicamentos</th><th>Total</th></tr>
      </thead>
      <tbody>
        {datos.map((d) => (
          <tr key={d.idVisitaMedica}>
            <td>{d.nombreInterno}</td>
            <td>{new Date(d.fechaVisita).toLocaleDateString()}</td>
            <td>Q{d.costoConsulta.toFixed(2)}</td>
            <td>Q{d.costoExamenes.toFixed(2)}</td>
            <td>Q{d.costoMedicamentos.toFixed(2)}</td>
            <td><strong>Q{d.costoTotal.toFixed(2)}</strong></td>
          </tr>
        ))}
        {datos.length === 0 && <tr><td colSpan="6">Sin datos.</td></tr>}
      </tbody>
    </table>
  );
}

function ReporteFichaMedica({ datos }) {
  return (
    <div>
      <h2>{datos.nombreCompleto}</h2>
      <p><strong>Fecha de ingreso:</strong> {new Date(datos.fechaIngreso).toLocaleDateString()}</p>
      <p><strong>Motivo de ingreso:</strong> {datos.motivoIngreso}</p>
      <p><strong>Psicopatologías:</strong> {datos.psicopatologias.length ? datos.psicopatologias.join(", ") : "Ninguna"}</p>
      <p><strong>Medicamentos de cajón:</strong> {datos.medicamentosCajon.length ? datos.medicamentosCajon.join(", ") : "Ninguno"}</p>
      <p><strong>Historial de visitas médicas:</strong></p>
      <table className="tabla">
        <thead><tr><th>Fecha</th><th>Motivo</th><th>Diagnóstico</th><th>Especialista</th><th>Medicamentos recetados</th></tr></thead>
        <tbody>
          {datos.visitas.map((v, i) => (
            <tr key={i}>
              <td>{new Date(v.fechaVisita).toLocaleDateString()}</td>
              <td>{v.motivo}</td>
              <td>{v.diagnostico}</td>
              <td>{v.nombreEspecialista}{v.especialidadEspecialista ? ` (${v.especialidadEspecialista})` : ""}</td>
              <td>{v.medicamentosRecetados.length ? v.medicamentosRecetados.join(", ") : "—"}</td>
            </tr>
          ))}
          {datos.visitas.length === 0 && <tr><td colSpan="5">Sin visitas registradas.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}

function ReporteCobrosPorPaciente({ datos }) {
  return (
    <div>
      <h2>{datos.nombreInterno}</h2>
      <p><strong>Total cobrado:</strong> Q{datos.totalCobrado.toFixed(2)} — <strong>Total cargos médicos:</strong> Q{datos.totalCargosMedicos.toFixed(2)}</p>
      <p><strong>Pagos (cobros) en el rango:</strong></p>
      <table className="tabla">
        <thead><tr><th>Fecha</th><th>Concepto</th><th>Monto</th></tr></thead>
        <tbody>
          {datos.cobros.map((c, i) => <tr key={i}><td>{new Date(c.fecha).toLocaleDateString()}</td><td>{c.concepto}</td><td>Q{c.monto.toFixed(2)}</td></tr>)}
          {datos.cobros.length === 0 && <tr><td colSpan="3">Sin pagos en el rango.</td></tr>}
        </tbody>
      </table>
      <p><strong>Detalle de gastos médicos (cargos) en el rango:</strong></p>
      <table className="tabla">
        <thead><tr><th>Fecha</th><th>Concepto</th><th>Monto</th></tr></thead>
        <tbody>
          {datos.cargosMedicos.map((c, i) => <tr key={i}><td>{new Date(c.fecha).toLocaleDateString()}</td><td>{c.concepto}</td><td>Q{c.monto.toFixed(2)}</td></tr>)}
          {datos.cargosMedicos.length === 0 && <tr><td colSpan="3">Sin cargos en el rango.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}

function ReportePagosFundacion({ datos }) {
  return (
    <table className="tabla">
      <thead><tr><th>Concepto</th><th>Monto</th></tr></thead>
      <tbody>
        <tr><td>Consultas médicas</td><td>Q{datos.totalConsultas.toFixed(2)}</td></tr>
        <tr><td>Laboratorio</td><td>Q{datos.totalLaboratorio.toFixed(2)}</td></tr>
        <tr><td>Farmacia</td><td>Q{datos.totalFarmacia.toFixed(2)}</td></tr>
        <tr><td><strong>Total a la fundación</strong></td><td><strong>Q{datos.totalGeneral.toFixed(2)}</strong></td></tr>
      </tbody>
    </table>
  );
}

function ReporteEntradas({ datos }) {
  return (
    <div>
      <p>
        <strong>Total donaciones:</strong> Q{datos.totalDonaciones.toFixed(2)} —{" "}
        <strong>Total cobros:</strong> Q{datos.totalCobros.toFixed(2)} —{" "}
        <strong>Total general:</strong> Q{datos.totalGeneral.toFixed(2)}
      </p>
      <table className="tabla">
        <thead><tr><th>Fecha</th><th>Tipo</th><th>Origen</th><th>Monto</th></tr></thead>
        <tbody>
          {datos.entradas.map((e, i) => (
            <tr key={i}><td>{new Date(e.fecha).toLocaleDateString()}</td><td>{e.tipo}</td><td>{e.origen}</td><td>Q{e.monto.toFixed(2)}</td></tr>
          ))}
          {datos.entradas.length === 0 && <tr><td colSpan="4">Sin entradas en el rango.</td></tr>}
        </tbody>
      </table>
    </div>
  );
}

function ReporteExamenesPorPaciente({ datos }) {
  return (
    <table className="tabla">
      <thead><tr><th>Examen</th><th>Fecha</th><th>Estado</th><th>Resultado</th><th>Costo</th></tr></thead>
      <tbody>
        {datos.map((e, i) => (
          <tr key={i}>
            <td>{e.nombreExamen}</td>
            <td>{new Date(e.fechaVisita).toLocaleDateString()}</td>
            <td>{e.estado === "Pendiente" ? "Pendiente" : "Resultado registrado"}</td>
            <td>{e.resultado || "—"}</td>
            <td>{e.costo != null ? `Q${e.costo.toFixed(2)}` : "—"}</td>
          </tr>
        ))}
        {datos.length === 0 && <tr><td colSpan="5">Sin exámenes registrados.</td></tr>}
      </tbody>
    </table>
  );
}

function ReporteMedicamentosPorPaciente({ datos }) {
  return (
    <table className="tabla">
      <thead><tr><th>Medicamento</th><th>Dosis</th><th>Indicaciones</th><th>Fecha</th><th>Estado</th><th>Costo</th></tr></thead>
      <tbody>
        {datos.map((m, i) => (
          <tr key={i}>
            <td>{m.nombreMedicamento}</td>
            <td>{m.dosis}</td>
            <td>{m.indicaciones}</td>
            <td>{new Date(m.fechaVisita).toLocaleDateString()}</td>
            <td>{m.estado === "Pendiente" ? "Pendiente de despacho" : "Despachado"}</td>
            <td>{m.costo != null ? `Q${m.costo.toFixed(2)}` : "—"}</td>
          </tr>
        ))}
        {datos.length === 0 && <tr><td colSpan="6">Sin medicamentos registrados.</td></tr>}
      </tbody>
    </table>
  );
}

export default Reportes;
