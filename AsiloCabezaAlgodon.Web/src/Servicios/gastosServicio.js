import api from "./api";

export async function listarGastos() {
  const respuesta = await api.get("/Gastos");
  return respuesta.data;
}

export async function crearGasto(concepto, monto) {
  const respuesta = await api.post("/Gastos", { concepto, monto });
  return respuesta.data;
}
