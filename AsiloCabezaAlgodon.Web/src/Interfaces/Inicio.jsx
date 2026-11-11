import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import { listarSolicitudes } from "../Servicios/solicitudesServicio";
import { listarVisitas } from "../Servicios/visitasServicio";
import { listarExamenes } from "../Servicios/examenesServicio";
import { listarPrescripciones } from "../Servicios/prescripcionesServicio";
import { listarDonaciones } from "../Servicios/donacionesServicio";
import { listarCobros } from "../Servicios/cobrosServicio";
import { reportePagosFundacion } from "../Servicios/reportesServicio";
import { indicadoresInicio, rangoDelMes } from "../utilidades/indicadores";

// AC-13 (punto 1): la pantalla de inicio muestra lo que cada rol tiene que atender,
// con el período de cada número y un enlace directo al módulo.
const SALUDO_POR_ROL = {
  Administrador: "Lo que está pendiente en el asilo y el movimiento de caja del mes.",
  MedicoGeneral: "El avance de las solicitudes que creaste para tus internos.",
  Especialista: "Las citas que la fundación te asignó.",
  Enfermero: "Los acompañamientos que tenés asignados.",
  Laboratorio: "Los exámenes que esperan su resultado.",
  Farmacia: "Los medicamentos que esperan despacho.",
};

// Qué consulta cada rol (solo endpoints que su rol tiene permitidos en la API)
const DATOS_POR_ROL = {
  Administrador: async () => {
    const mes = rangoDelMes(new Date());
    const [solicitudes, visitas, donaciones, cobros, fundacion] = await Promise.all([
      listarSolicitudes(), listarVisitas(), listarDonaciones(), listarCobros(), reportePagosFundacion(mes.desde, mes.hasta),
    ]);
    return { solicitudes, visitas, donaciones, cobros, fundacion };
  },
  MedicoGeneral: async () => ({ solicitudes: await listarSolicitudes() }),
  Especialista: async () => ({ solicitudes: await listarSolicitudes() }),
  Enfermero: async () => ({ solicitudes: await listarSolicitudes() }),
  Laboratorio: async () => ({ examenes: await listarExamenes() }),
  Farmacia: async () => ({ prescripciones: await listarPrescripciones() }),
};

const MAXIMO_PENDIENTES = 5;

function Indicador({ t }) {
  return (
    <Link to={t.ruta} className={`tarjeta-stat tarjeta-enlace${t.alerta ? " tarjeta-alerta" : ""}`}>
      <div className="stat-periodo">{t.periodo}</div>
      <div className="stat-valor">{t.valor}</div>
      <div className="stat-etiqueta">{t.etiqueta}</div>
      <div className="stat-ir">Abrir →</div>
    </Link>
  );
}

function Inicio() {
  const { rol } = useAutenticacion();
  const [resultado, setResultado] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const cargar = DATOS_POR_ROL[rol];
    if (!cargar) return;
    cargar()
      .then((datos) => setResultado(indicadoresInicio(rol, datos)))
      .catch(() => setError("No se pudieron cargar los indicadores. Probá recargar la página."));
  }, [rol]);

  const pendientes = resultado?.pendientes;

  return (
    <div>
      <div className="bienvenida">
        <h1>Bienvenido(a)</h1>
        <p>{SALUDO_POR_ROL[rol] || "Estás conectado al sistema."}</p>
      </div>

      {error && <p className="error">{error}</p>}
      {!resultado && !error && <p className="nota-tabla">Cargando…</p>}

      {resultado && (
        <>
          <div className="tarjetas-stat">
            {resultado.tarjetas.map((t) => <Indicador key={t.etiqueta} t={t} />)}
          </div>

          {pendientes && (
            <section className="tarjeta-formulario panel-pendientes">
              <div className="panel-pendientes-encabezado">
                <h2>{pendientes.titulo}</h2>
                <Link to={pendientes.ruta}>Ver todo</Link>
              </div>
              {pendientes.filas.length === 0 ? (
                <p className="nota-tabla">No hay nada pendiente. 🎉</p>
              ) : (
                <ul className="lista-pendientes">
                  {pendientes.filas.slice(0, MAXIMO_PENDIENTES).map((f) => (
                    <li key={f.id}>
                      <Link to={pendientes.ruta}>
                        <strong>{f.principal}</strong>
                        <span>{f.detalle}</span>
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
              {pendientes.filas.length > MAXIMO_PENDIENTES && (
                <p className="nota-tabla">Y {pendientes.filas.length - MAXIMO_PENDIENTES} más.</p>
              )}
            </section>
          )}
        </>
      )}
    </div>
  );
}

export default Inicio;
