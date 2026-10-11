import { useEffect, useState } from "react";
import { listarVisitas } from "../Servicios/visitasServicio";
import Badge from "../componentes/Badge";

function VisitasMedicas() {
  const [visitas, setVisitas] = useState([]);

  useEffect(() => {
    listarVisitas().then(setVisitas);
  }, []);

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Visitas médicas</h1>
          <p className="subtitulo">Historial de diagnósticos, exámenes y medicamentos por visita</p>
        </div>
      </div>
      {visitas.map((v) => (
        <div key={v.idVisitaMedica} className="tarjeta-formulario">
          <h2>{v.nombreInterno} — {new Date(v.fechaVisita).toLocaleString()}</h2>
          <p><strong>Especialista:</strong> {v.nombreEspecialista}{v.especialidadEspecialista ? ` (${v.especialidadEspecialista})` : ""}</p>
          <p><strong>Motivo de la visita:</strong> {v.motivo}</p>
          <p><strong>Diagnóstico:</strong> {v.diagnostico}</p>
          <p><strong>Observaciones:</strong> {v.observaciones || "—"}</p>
          <p><strong>Costo de consulta:</strong> Q{v.costoConsulta.toFixed(2)}</p>
          <p>
            <strong>Estado de pago:</strong> <Badge texto={v.estadoPago === "Pagada" ? "Pagada" : v.estadoPago === "PendientePago" ? "Pendiente de pago" : "Sin cargos"} estado={v.estadoPago} />
            {" "}(Q{v.totalCobrado.toFixed(2)} de Q{v.totalCargos.toFixed(2)})
          </p>

          <p><strong>Exámenes solicitados:</strong></p>
          <ul>
            {v.examenes.length === 0 && <li>Ninguno</li>}
            {v.examenes.map((e) => (
              <li key={e.idExamenSolicitado}>
                {e.nombreExamen} — <Badge texto={e.estado === "Pendiente" ? "Pendiente" : "Resultado registrado"} estado={e.estado} />
                {e.estado !== "Pendiente" && (
                  <span> — {e.resultado} ({e.costo != null ? `Q${e.costo}` : "sin costo registrado"})</span>
                )}
              </li>
            ))}
          </ul>

          <p><strong>Medicamentos recetados:</strong></p>
          <ul>
            {v.prescripciones.length === 0 && <li>Ninguno</li>}
            {v.prescripciones.map((p) => (
              <li key={p.idPrescripcion}>
                {p.nombreMedicamento} ({p.dosis}) — <Badge texto={p.estado === "Pendiente" ? "Pendiente" : "Despachado"} estado={p.estado} />
                {p.estado !== "Pendiente" && <span> — Q{p.costo}</span>}
              </li>
            ))}
          </ul>
        </div>
      ))}
      {visitas.length === 0 && <p>No hay visitas registradas todavía.</p>}
    </div>
  );
}

export default VisitasMedicas;
