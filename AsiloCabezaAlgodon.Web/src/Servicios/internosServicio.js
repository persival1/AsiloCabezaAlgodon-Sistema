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

// AC-08: edición, alta o baja y corrección de la ficha
export async function editarInterno(idInterno, datos) {
  await api.put(`/Internos/${idInterno}`, datos);
}

export async function cambiarEstadoInterno(idInterno, estado) {
  await api.put(`/Internos/${idInterno}/estado`, { estado });
}

export async function editarPsicopatologia(idInterno, idPsicopatologia, nombre, fechaDiagnostico) {
  await api.put(`/Internos/${idInterno}/psicopatologias/${idPsicopatologia}`, { nombre, fechaDiagnostico });
}

export async function eliminarPsicopatologia(idInterno, idPsicopatologia) {
  await api.delete(`/Internos/${idInterno}/psicopatologias/${idPsicopatologia}`);
}

export async function editarMedicamentoCajon(idInterno, idMedicamento, nombre, dosis, frecuencia) {
  await api.put(`/Internos/${idInterno}/medicamentos-cajon/${idMedicamento}`, { nombre, dosis, frecuencia });
}

export async function eliminarMedicamentoCajon(idInterno, idMedicamento) {
  await api.delete(`/Internos/${idInterno}/medicamentos-cajon/${idMedicamento}`);
}
