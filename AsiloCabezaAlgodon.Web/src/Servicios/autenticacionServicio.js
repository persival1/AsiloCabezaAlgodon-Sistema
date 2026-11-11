import api from "./api";

export async function iniciarSesion(nombreUsuario, password) {
  const respuesta = await api.post("/Auth/login", {
    nombreUsuario,
    password,
  });
  return respuesta.data;
}
// AC-05: el usuario con sesión cambia su propia contraseña
export async function cambiarClave(claveActual, claveNueva) {
  await api.put("/Auth/cambiar-clave", { claveActual, claveNueva });
}
