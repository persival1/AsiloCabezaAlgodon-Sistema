import api from "./api";

export async function listarExamenes() {
  const respuesta = await api.get("/Examenes");
  return respuesta.data;
}

export async function registrarResultado(idExamenSolicitado, resultado, costo) {
  const respuesta = await api.put(`/Examenes/${idExamenSolicitado}/resultado`, { resultado, costo });
  return respuesta.data;
}
