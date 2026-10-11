import api from "./api";

export async function iniciarSesion(nombreUsuario, password) {
  const respuesta = await api.post("/Auth/login", {
    nombreUsuario,
    password,
  });
  return respuesta.data;
}