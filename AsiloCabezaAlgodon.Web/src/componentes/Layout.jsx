import { useState } from "react";
import { Outlet, useNavigate, useLocation, Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import {
  IconoInicio, IconoInternos, IconoSolicitudes, IconoVisitas, IconoCitas,
  IconoAcompanamientos, IconoExamenes, IconoPrescripciones, IconoDonaciones,
  IconoCobros, IconoGastos, IconoReportes,
} from "./Iconos";
import VersionApp from "./VersionApp";

const NOMBRES_ROL = {
  Administrador: "Administrador",
  MedicoGeneral: "Médico General",
  Especialista: "Especialista",
  Enfermero: "Enfermero(a)",
  Laboratorio: "Laboratorio",
  Farmacia: "Farmacia",
};

// AC-13 (punto 3): opciones agrupadas por área, con los mismos nombres en todos los roles.
// Cada rol ve solo lo que la API le permite (matriz de docs/matriz-actores-y-roles.md).
const INICIO = { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio };
const REPORTES = { titulo: "Reportes", opciones: [{ texto: "Reportes", ruta: "/dashboard/reportes", icono: IconoReportes }] };

const MENUS_POR_ROL = {
  Administrador: [
    { opciones: [INICIO] },
    { titulo: "Atención médica", opciones: [
      { texto: "Internos", ruta: "/dashboard/internos", icono: IconoInternos },
      { texto: "Solicitudes", ruta: "/dashboard/solicitudes", icono: IconoSolicitudes },
      { texto: "Visitas médicas", ruta: "/dashboard/visitas", icono: IconoVisitas },
    ] },
    { titulo: "Caja", opciones: [
      { texto: "Cobros", ruta: "/dashboard/cobros", icono: IconoCobros },
      { texto: "Donaciones", ruta: "/dashboard/donaciones", icono: IconoDonaciones },
      { texto: "Gastos", ruta: "/dashboard/gastos", icono: IconoGastos },
      { texto: "Pagos a la fundación", ruta: "/dashboard/pagos-fundacion", icono: IconoCobros },
    ] },
    REPORTES,
  ],
  MedicoGeneral: [
    { opciones: [INICIO] },
    { titulo: "Atención médica", opciones: [
      { texto: "Internos", ruta: "/dashboard/internos", icono: IconoInternos },
      { texto: "Solicitudes", ruta: "/dashboard/solicitudes", icono: IconoSolicitudes },
    ] },
    REPORTES,
  ],
  Especialista: [
    { opciones: [INICIO] },
    { titulo: "Atención médica", opciones: [
      { texto: "Mis citas", ruta: "/dashboard/citas", icono: IconoCitas },
      { texto: "Visitas médicas", ruta: "/dashboard/visitas", icono: IconoVisitas },
      { texto: "Mis pacientes", ruta: "/dashboard/internos", icono: IconoInternos },
    ] },
    REPORTES,
  ],
  Enfermero: [
    { opciones: [INICIO] },
    { titulo: "Atención médica", opciones: [
      { texto: "Mis acompañamientos", ruta: "/dashboard/acompanamientos", icono: IconoAcompanamientos },
    ] },
  ],
  Laboratorio: [
    { opciones: [INICIO] },
    { titulo: "Laboratorio", opciones: [
      { texto: "Exámenes solicitados", ruta: "/dashboard/examenes", icono: IconoExamenes },
    ] },
  ],
  Farmacia: [
    { opciones: [INICIO] },
    { titulo: "Farmacia", opciones: [
      { texto: "Prescripciones", ruta: "/dashboard/prescripciones", icono: IconoPrescripciones },
    ] },
  ],
};

function Layout() {
  const { rol, cerrarSesion } = useAutenticacion();
  const navigate = useNavigate();
  const location = useLocation();

  const gruposMenu = MENUS_POR_ROL[rol] || [];

  const [tema, setTema] = useState(document.documentElement.dataset.tema || "oscuro");

  function cambiarTema() {
    const nuevo = tema === "claro" ? "oscuro" : "claro";
    document.documentElement.dataset.tema = nuevo;
    try { localStorage.setItem("tema", nuevo); } catch { /* sin almacenamiento */ }
    setTema(nuevo);
  }

  function manejarCerrarSesion() {
    cerrarSesion();
    navigate("/");
  }

  return (
    <div className="layout-contenedor">
      <aside className="layout-barra-lateral">
        <div className="layout-marca">
          <span className="marca-icono">🌿</span>
          Asilo Cabeza de Algodón
        </div>

        <nav className="layout-menu" aria-label="Menú principal">
          {gruposMenu.map((grupo, g) => (
            <div key={grupo.titulo || g} className="layout-grupo">
              {grupo.titulo && <div className="layout-grupo-titulo">{grupo.titulo}</div>}
              {grupo.opciones.map((opcion) => {
                const Icono = opcion.icono;
                const activo = opcion.ruta === "/dashboard"
                  ? location.pathname === opcion.ruta
                  : location.pathname.startsWith(opcion.ruta);
                return (
                  <Link
                    key={opcion.ruta}
                    to={opcion.ruta}
                    className={`layout-enlace${activo ? " activo" : ""}`}
                    aria-current={activo ? "page" : undefined}
                  >
                    <Icono />
                    {opcion.texto}
                  </Link>
                );
              })}
            </div>
          ))}
        </nav>

        <div className="layout-pie">
          <div className="layout-usuario">
            Conectado como
            <strong>{NOMBRES_ROL[rol] || rol}</strong>
          </div>
          <button onClick={cambiarTema} className="boton-tema">
            {tema === "claro" ? "🌙 Modo oscuro" : "☀️ Modo claro"}
          </button>
          <button onClick={manejarCerrarSesion} className="layout-boton-salir">
            Cerrar sesión
          </button>
          <VersionApp />
        </div>
      </aside>

      <div className="layout-contenido">
        <header className="layout-encabezado">
          <span>Asilo de Ancianos Cabeza de Algodón</span>
          <span className="rol-chip">{NOMBRES_ROL[rol] || rol}</span>
        </header>

        <main className="layout-principal">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default Layout;
