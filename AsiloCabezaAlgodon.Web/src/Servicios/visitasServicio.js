import api from "./api";

export async function listarVisitas() {
  const respuesta = await api.get("/VisitasMedicas");
  return respuesta.data;
}

export async function listarVisitasPorInterno(idInterno) {
  const respuesta = await api.get("/VisitasMedicas", { params: { idInterno } });
  return respuesta.data;
}

export async function registrarVisita(idSolicitud, diagnostico, observaciones, costoConsulta, examenes, prescripciones) {
  const respuesta = await api.post("/VisitasMedicas", {
    idSolicitud,
    diagnostico,
    observaciones,
    costoConsulta,
    examenes,
    prescripciones,
  });
  return respuesta.data;
}
