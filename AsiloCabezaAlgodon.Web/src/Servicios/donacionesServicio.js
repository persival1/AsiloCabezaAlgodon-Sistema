import api from "./api";

export async function listarDonaciones() {
  const respuesta = await api.get("/Donaciones");
  return respuesta.data;
}

export async function crearDonacion(tipoDonante, nombreDonante, monto, descripcion) {
  const respuesta = await api.post("/Donaciones", { tipoDonante, nombreDonante, monto, descripcion });
  return respuesta.data;
}
