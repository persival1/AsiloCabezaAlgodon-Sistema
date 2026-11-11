import api from "./api";

export async function listarUsuarios() {
  const respuesta = await api.get("/Usuarios");
  return respuesta.data;
}

// AC-08 (CU-004) y AC-05: administración de usuarios
export async function crearUsuario(datos) {
  const respuesta = await api.post("/Usuarios", datos);
  return respuesta.data;
}

export async function editarUsuario(idUsuario, datos) {
  await api.put(`/Usuarios/${idUsuario}`, datos);
}

export async function cambiarEstadoUsuario(idUsuario, estado) {
  await api.put(`/Usuarios/${idUsuario}/estado`, { estado });
}

export async function restablecerClave(idUsuario, claveTemporal) {
  await api.put(`/Usuarios/${idUsuario}/clave`, { claveTemporal });
}
