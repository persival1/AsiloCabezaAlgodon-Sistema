import { useEffect, useState } from "react";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { listarInternos } from "../Servicios/internosServicio";
import { listarSolicitudes } from "../Servicios/solicitudesServicio";
import { listarVisitas } from "../Servicios/visitasServicio";
import { listarExamenes } from "../Servicios/examenesServicio";
import { listarPrescripciones } from "../Servicios/prescripcionesServicio";
import { listarDonaciones } from "../Servicios/donacionesServicio";

const SALUDO_POR_ROL = {
  Administrador: "Un vistazo general al asilo, en un solo lugar.",
  MedicoGeneral: "Tus internos y las solicitudes que has creado.",
  Especialista: "Tus citas asignadas y tu historial de visitas.",
  Enfermero: "Los acompañamientos que tenés asignados.",
  Laboratorio: "Los exámenes que el asilo te ha solicitado.",
  Farmacia: "Las prescripciones pendientes de despacho.",
};

function Tarjeta({ valor, etiqueta }) {
  return (
    <div className="tarjeta-stat">
      <div className="stat-valor">{valor}</div>
      <div className="stat-etiqueta">{etiqueta}</div>
    </div>
  );
}

// Consulta la API y devuelve las tarjetas que le corresponden a cada rol
async function obtenerTarjetas(rol) {
  if (rol === "Administrador") {
    const [internos, solicitudes, donaciones] = await Promise.all([
      listarInternos(), listarSolicitudes(), listarDonaciones(),
    ]);
    const totalEntradas = donaciones.reduce((s, d) => s + d.monto, 0);
    return [
      { valor: internos.length, etiqueta: "Internos registrados" },
      { valor: solicitudes.filter((s) => s.estado === "Pendiente").length, etiqueta: "Solicitudes pendientes" },
      { valor: solicitudes.filter((s) => s.estado === "Asignada").length, etiqueta: "Citas asignadas" },
      { valor: `Q${totalEntradas.toFixed(2)}`, etiqueta: "Donaciones recibidas" },
    ];
  }
  if (rol === "MedicoGeneral") {
    const [internos, solicitudes] = await Promise.all([listarInternos(), listarSolicitudes()]);
    return [
      { valor: internos.length, etiqueta: "Internos registrados" },
      { valor: solicitudes.filter((s) => s.estado === "Pendiente").length, etiqueta: "Mis solicitudes pendientes" },
      { valor: solicitudes.length, etiqueta: "Solicitudes creadas en total" },
    ];
  }
  if (rol === "Especialista") {
    const [solicitudes, visitas] = await Promise.all([listarSolicitudes(), listarVisitas()]);
    return [
      { valor: solicitudes.filter((s) => s.estado === "Asignada").length, etiqueta: "Citas por atender" },
      { valor: visitas.length, etiqueta: "Visitas registradas" },
    ];
  }
  if (rol === "Enfermero") {
    const solicitudes = await listarSolicitudes();
    return [
      { valor: solicitudes.filter((s) => s.estado === "Asignada").length, etiqueta: "Acompañamientos próximos" },
      { valor: solicitudes.length, etiqueta: "Acompañamientos en total" },
    ];
  }
  if (rol === "Laboratorio") {
    const examenes = await listarExamenes();
    return [
      { valor: examenes.filter((e) => e.estado === "Pendiente").length, etiqueta: "Exámenes pendientes" },
      { valor: examenes.length, etiqueta: "Exámenes en total" },
    ];
  }
  if (rol === "Farmacia") {
    const prescripciones = await listarPrescripciones();
    return [
      { valor: prescripciones.filter((p) => p.estado === "Pendiente").length, etiqueta: "Prescripciones pendientes" },
      { valor: prescripciones.length, etiqueta: "Prescripciones en total" },
    ];
  }
  return [];
}

function Inicio() {
  const { rol } = useAutenticacion();
  const [tarjetas, setTarjetas] = useState([]);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    obtenerTarjetas(rol).then(setTarjetas).finally(() => setCargando(false));
  }, [rol]);

  return (
    <div>
      <div className="bienvenida">
        <h1>Bienvenido(a)</h1>
        <p>{SALUDO_POR_ROL[rol] || "Estás conectado al sistema."}</p>
      </div>

      {!cargando && tarjetas.length > 0 && (
        <div className="tarjetas-stat">
          {tarjetas.map((t, i) => <Tarjeta key={i} valor={t.valor} etiqueta={t.etiqueta} />)}
        </div>
      )}
    </div>
  );
}

export default Inicio;
