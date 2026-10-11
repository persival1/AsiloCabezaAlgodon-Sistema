import api from "./api";

export async function listarUsuarios() {
  const respuesta = await api.get("/Usuarios");
  return respuesta.data;
}
