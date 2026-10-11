import api from "./api";

export async function listarCobros() {
  const respuesta = await api.get("/Cobros");
  return respuesta.data;
}

export async function crearCobro(idFamiliar, concepto, monto, idVisitaMedica) {
  const respuesta = await api.post("/Cobros", { idFamiliar, concepto, monto, idVisitaMedica });
  return respuesta.data;
}
