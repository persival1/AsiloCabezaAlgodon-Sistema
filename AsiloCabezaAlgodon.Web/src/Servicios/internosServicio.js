import api from "./api";

export async function listarInternos() {
  const respuesta = await api.get("/Internos");
  return respuesta.data;
}

export async function obtenerInterno(id) {
  const respuesta = await api.get(`/Internos/${id}`);
  return respuesta.data;
}

export async function crearInterno(datos) {
  const respuesta = await api.post("/Internos", datos);
  return respuesta.data;
}

export async function agregarPsicopatologia(idInterno, nombre, fechaDiagnostico) {
  const respuesta = await api.post(`/Internos/${idInterno}/psicopatologias`, { nombre, fechaDiagnostico });
  return respuesta.data;
}

export async function agregarMedicamentoCajon(idInterno, nombre, dosis, frecuencia) {
  const respuesta = await api.post(`/Internos/${idInterno}/medicamentos-cajon`, { nombre, dosis, frecuencia });
  return respuesta.data;
}
