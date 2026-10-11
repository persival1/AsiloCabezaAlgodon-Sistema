import api from "./api";

export async function listarPrescripciones() {
  const respuesta = await api.get("/Prescripciones");
  return respuesta.data;
}

export async function despacharPrescripcion(idPrescripcion, costo) {
  const respuesta = await api.put(`/Prescripciones/${idPrescripcion}/despachar`, { costo });
  return respuesta.data;
}
