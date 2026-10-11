import { useEffect, useState } from "react";
import { listarSolicitudes } from "../Servicios/solicitudesServicio";
import Badge from "../componentes/Badge";

function Acompanamientos() {
  const [solicitudes, setSolicitudes] = useState([]);

  useEffect(() => {
    listarSolicitudes().then(setSolicitudes);
  }, []);

  return (
    <div>
      <div className="encabezado-seccion">
        <div>
          <h1>Mis acompañamientos</h1>
          <p className="subtitulo">Internos que vas a acompañar a su cita médica</p>
        </div>
      </div>
      <table className="tabla">
        <thead>
          <tr>
            <th>Interno</th>
            <th>Motivo</th>
            <th>Estado</th>
            <th>Fecha y hora de cita</th>
            <th>Especialista</th>
          </tr>
        </thead>
        <tbody>
          {solicitudes.map((s) => (
            <tr key={s.idSolicitud}>
              <td>{s.nombreInterno}</td>
              <td>{s.motivo}</td>
              <td><Badge texto={s.estado} /></td>
              <td>{s.fechaHoraCita ? new Date(s.fechaHoraCita).toLocaleString() : "—"}</td>
              <td>{s.nombreEspecialista ? `${s.nombreEspecialista}${s.especialidadEspecialista ? ` (${s.especialidadEspecialista})` : ""}` : "—"}</td>
            </tr>
          ))}
          {solicitudes.length === 0 && (
            <tr>
              <td colSpan="5">No tenés acompañamientos asignados.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Acompanamientos;
