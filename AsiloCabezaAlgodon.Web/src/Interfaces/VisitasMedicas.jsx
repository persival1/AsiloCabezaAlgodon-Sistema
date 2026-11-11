import { Fragment, useEffect, useState } from "react";
import { listarVisitas } from "../Servicios/visitasServicio";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import Badge from "../componentes/Badge";
import { FiltrosTabla, PaginacionTabla } from "../componentes/ControlesTabla";
import { useTabla } from "../utilidades/useTabla";

// AC-13 (punto 6): las visitas se presentan como tabla, con búsqueda, filtros y paginación;
// el detalle (exámenes y medicamentos) se abre en la misma fila.
const OPCIONES_TABLA = {
  campos: ["nombreInterno", "nombreEspecialista", "motivo", "diagnostico"],
  estado: (v) => v.estadoPago,
  fecha: (v) => v.fechaVisita,
};
const ESTADOS_PAGO = [
  { valor: "PendientePago", texto: "Pendiente de pago" },
  { valor: "Pagada", texto: "Pagada" },
  { valor: "SinCargos", texto: "Sin cargos" },
];
const TEXTO_PAGO = { Pagada: "Pagada", PendientePago: "Pendiente de pago", SinCargos: "Sin cargos" };

function DetalleVisita({ v, veFinanzas }) {
  return (
    <div className="detalle-visita">
      <p><strong>Motivo:</strong> {v.motivo}</p>
      <p><strong>Observaciones:</strong> {v.observaciones || "—"}</p>
      {veFinanzas && (
        <p><strong>Pagado:</strong> Q{v.totalCobrado.toFixed(2)} de Q{v.totalCargos.toFixed(2)} en cargos</p>
      )}
      <div className="detalle-columnas">
        <div>
          <h3>Exámenes solicitados</h3>
          <ul>
            {v.examenes.length === 0 && <li>Ninguno</li>}
            {v.examenes.map((e) => (
              <li key={e.idExamenSolicitado}>
                {e.nombreExamen} <Badge texto={e.estado === "Pendiente" ? "Pendiente" : "Resultado registrado"} estado={e.estado} />
                {e.estado !== "Pendiente" && (
                  <span> — {e.resultado} ({e.costo != null ? `Q${e.costo}` : "sin costo registrado"})</span>
                )}
              </li>
            ))}
          </ul>
        </div>
        <div>
          <h3>Medicamentos recetados</h3>
          <ul>
            {v.prescripciones.length === 0 && <li>Ninguno</li>}
            {v.prescripciones.map((p) => (
              <li key={p.idPrescripcion}>
                {p.nombreMedicamento} ({p.dosis}) <Badge texto={p.estado === "Pendiente" ? "Pendiente" : "Despachado"} estado={p.estado} />
                {p.estado !== "Pendiente" && <span> — Q{p.costo}</span>}
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}

function VisitasMedicas() {
  const { rol } = useAutenticacion();
  // AC-04: el estado de pago solo llega al Administrador; los roles clínicos no lo ven
  const veFinanzas = rol === "Administrador";
  const [visitas, setVisitas] = useState([]);
  const [abierta, setAbierta] = useState(null);
  const tabla = useTabla(visitas, OPCIONES_TABLA);
  const columnas = veFinanzas ? 7 : 6;

  useEffect(() => {
    listarVisitas().then(setVisitas);
  }, []);

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Visitas médicas</h1>
          <p className="subtitulo">Diagnóstico de cada visita; abrí una fila para ver sus exámenes y medicamentos</p>
        </div>
      </div>

      <FiltrosTabla
        tabla={tabla}
        placeholder="Interno, especialista, motivo o diagnóstico"
        estados={veFinanzas ? ESTADOS_PAGO : null}
        etiquetaFecha="Visita"
      />
      <table className="tabla">
        <thead>
          <tr>
            <th>Fecha</th>
            <th>Interno</th>
            <th>Especialista</th>
            <th>Diagnóstico</th>
            <th>Consulta</th>
            {veFinanzas && <th>Pago</th>}
            <th></th>
          </tr>
        </thead>
        <tbody>
          {tabla.filas.map((v) => (
            <Fragment key={v.idVisitaMedica}>
              <tr className={abierta === v.idVisitaMedica ? "fila-abierta" : undefined}>
                <td>{new Date(v.fechaVisita).toLocaleString()}</td>
                <td>{v.nombreInterno}</td>
                <td>{v.nombreEspecialista}{v.especialidadEspecialista ? ` (${v.especialidadEspecialista})` : ""}</td>
                <td>{v.diagnostico}</td>
                <td>Q{v.costoConsulta.toFixed(2)}</td>
                {veFinanzas && <td><Badge texto={TEXTO_PAGO[v.estadoPago] || v.estadoPago} estado={v.estadoPago} /></td>}
                <td>
                  <button
                    type="button"
                    className="boton-secundario"
                    aria-expanded={abierta === v.idVisitaMedica}
                    onClick={() => setAbierta(abierta === v.idVisitaMedica ? null : v.idVisitaMedica)}
                  >
                    {abierta === v.idVisitaMedica ? "Ocultar" : "Ver detalle"}
                  </button>
                </td>
              </tr>
              {abierta === v.idVisitaMedica && (
                <tr className="fila-detalle">
                  <td colSpan={columnas}><DetalleVisita v={v} veFinanzas={veFinanzas} /></td>
                </tr>
              )}
            </Fragment>
          ))}
          {tabla.filas.length === 0 && (
            <tr>
              <td colSpan={columnas}>{tabla.total === 0 ? "No hay visitas registradas todavía." : "Ninguna visita coincide con la búsqueda."}</td>
            </tr>
          )}
        </tbody>
      </table>
      <PaginacionTabla tabla={tabla} nombre="visitas" />
    </div>
  );
}

export default VisitasMedicas;
