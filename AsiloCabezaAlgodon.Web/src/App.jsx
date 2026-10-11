import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./Interfaces/Login";
import Inicio from "./Interfaces/Inicio";
import Internos from "./Interfaces/Internos";
import FichaInterno from "./Interfaces/FichaInterno";
import Solicitudes from "./Interfaces/Solicitudes";
import Citas from "./Interfaces/Citas";
import Acompanamientos from "./Interfaces/Acompanamientos";
import VisitasMedicas from "./Interfaces/VisitasMedicas";
import Examenes from "./Interfaces/Examenes";
import Prescripciones from "./Interfaces/Prescripciones";
import Donaciones from "./Interfaces/Donaciones";
import Gastos from "./Interfaces/Gastos";
import Cobros from "./Interfaces/Cobros";
import Reportes from "./Interfaces/Reportes";
import Layout from "./componentes/Layout";
import RutaProtegida from "./componentes/RutaProtegida";

function App() {
  return (
    <Routes>
      <Route path="/" element={<Login />} />

      <Route
        path="/dashboard"
        element={
          <RutaProtegida>
            <Layout />
          </RutaProtegida>
        }
      >
        <Route index element={<Inicio />} />
        <Route path="internos" element={<Internos />} />
        <Route path="internos/:id" element={<FichaInterno />} />
        <Route path="solicitudes" element={<Solicitudes />} />
        <Route path="citas" element={<Citas />} />
        <Route path="visitas" element={<VisitasMedicas />} />
        <Route path="acompanamientos" element={<Acompanamientos />} />
        <Route path="examenes" element={<Examenes />} />
        <Route path="prescripciones" element={<Prescripciones />} />
        <Route path="donaciones" element={<Donaciones />} />
        <Route path="gastos" element={<Gastos />} />
        <Route path="cobros" element={<Cobros />} />
        <Route path="reportes" element={<Reportes />} />
      </Route>

      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
}

export default App;