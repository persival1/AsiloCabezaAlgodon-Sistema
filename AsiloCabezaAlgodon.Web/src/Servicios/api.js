import axios from "axios";

// CORREGIDO (observación SQA #8 - despliegue): la dirección de la API ya no está fija.
// Vite lee la variable VITE_API_URL del archivo .env (ver .env.example). Si no existe,
// se usa la dirección del entorno local de desarrollo.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "https://localhost:7022/api",
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// CORREGIDO (hallazgo de la revisión): si el servidor responde 401, el token ya no sirve
// (venció a las 8 horas o se firmó con otra llave). Antes la pantalla se quedaba con
// errores en segundo plano; ahora se cierra la sesión y se vuelve al login con un aviso.
// El login mismo también responde 401 cuando la contraseña es incorrecta: ese caso se
// excluye para que la pantalla de login pueda mostrar su mensaje de error.
api.interceptors.response.use(
  (respuesta) => respuesta,
  (error) => {
    const esLogin = error.config?.url?.includes("/Auth/login");
    if (error.response?.status === 401 && !esLogin) {
      localStorage.removeItem("token");
      localStorage.removeItem("rol");
      sessionStorage.setItem("sesionExpirada", "1");
      window.location.href = "/";
    }
    return Promise.reject(error);
  }
);

// Cuando el servidor rechaza una operación (400 datos inválidos, 409 conflicto) explica el
// motivo en { mensaje }. Esta función lo saca para mostrarlo; si no viene, usa el genérico.
export function mensajeDeError(error, porDefecto) {
  return error.response?.data?.mensaje || porDefecto;
}

export default api;