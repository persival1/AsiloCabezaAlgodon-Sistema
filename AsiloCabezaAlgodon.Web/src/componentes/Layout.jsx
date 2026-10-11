import { useState } from "react";
import { Outlet, useNavigate, useLocation, Link } from "react-router-dom";
import { useAutenticacion } from "../contexto/AutenticacionContexto";
import {
  IconoInicio, IconoInternos, IconoSolicitudes, IconoVisitas, IconoCitas,
  IconoAcompanamientos, IconoExamenes, IconoPrescripciones, IconoDonaciones,
  IconoCobros, IconoGastos, IconoReportes,
} from "./Iconos";

const NOMBRES_ROL = {
  Administrador: "Administrador",
  MedicoGeneral: "Médico General",
  Especialista: "Especialista",
  Enfermero: "Enfermero(a)",
  Laboratorio: "Laboratorio",
  Farmacia: "Farmacia",
};

const MENUS_POR_ROL = {
  Administrador: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Internos", ruta: "/dashboard/internos", icono: IconoInternos },
    { texto: "Solicitudes", ruta: "/dashboard/solicitudes", icono: IconoSolicitudes },
    { texto: "Visitas médicas", ruta: "/dashboard/visitas", icono: IconoVisitas },
    { texto: "Donaciones", ruta: "/dashboard/donaciones", icono: IconoDonaciones },
    { texto: "Cobros", ruta: "/dashboard/cobros", icono: IconoCobros },
    { texto: "Gastos", ruta: "/dashboard/gastos", icono: IconoGastos },
    { texto: "Reportes", ruta: "/dashboard/reportes", icono: IconoReportes },
  ],
  MedicoGeneral: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Internos", ruta: "/dashboard/internos", icono: IconoInternos },
    { texto: "Solicitudes", ruta: "/dashboard/solicitudes", icono: IconoSolicitudes },
  ],
  Especialista: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Mis citas", ruta: "/dashboard/citas", icono: IconoCitas },
    { texto: "Visitas médicas", ruta: "/dashboard/visitas", icono: IconoVisitas },
    { texto: "Internos", ruta: "/dashboard/internos", icono: IconoInternos },
  ],
  Enfermero: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Mis acompañamientos", ruta: "/dashboard/acompanamientos", icono: IconoAcompanamientos },
  ],
  Laboratorio: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Exámenes solicitados", ruta: "/dashboard/examenes", icono: IconoExamenes },
  ],
  Farmacia: [
    { texto: "Inicio", ruta: "/dashboard", icono: IconoInicio },
    { texto: "Prescripciones", ruta: "/dashboard/prescripciones", icono: IconoPrescripciones },
  ],
};

function Layout() {
  const { rol, cerrarSesion } = useAutenticacion();
  const navigate = useNavigate();
  const location = useLocation();

  const opcionesMenu = MENUS_POR_ROL[rol] || [];

  const [tema, setTema] = useState(document.documentElement.dataset.tema || "oscuro");

  function cambiarTema() {
    const nuevo = tema === "claro" ? "oscuro" : "claro";
    document.documentElement.dataset.tema = nuevo;
    try { localStorage.setItem("tema", nuevo); } catch (e) { /* sin almacenamiento */ }
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

        <nav className="layout-menu">
          {opcionesMenu.map((opcion) => {
            const Icono = opcion.icono;
            const activo = opcion.ruta === "/dashboard"
              ? location.pathname === opcion.ruta
              : location.pathname.startsWith(opcion.ruta);
            return (
              <Link
                key={opcion.ruta}
                to={opcion.ruta}
                className={`layout-enlace${activo ? " activo" : ""}`}
              >
                <Icono />
                {opcion.texto}
              </Link>
            );
          })}
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
