// AC-13 (punto 6): búsqueda, filtro por estado, rango de fechas y paginación de las listas.
// Funciones puras para poder probarlas sin navegador (ver __tests__/tabla.test.js).

export const TAMANO_PAGINA = 10;

// Quita mayúsculas y tildes para que "jose" encuentre "José".
export function normalizar(texto) {
  return String(texto ?? "")
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();
}

// "2026-11-05T14:30:00" -> "2026-11-05" (fecha local, como la escribe un <input type="date">)
export function fechaCorta(valor) {
  const d = new Date(valor);
  if (Number.isNaN(d.getTime())) return "";
  const dos = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${dos(d.getMonth() + 1)}-${dos(d.getDate())}`;
}

// filtros: { texto, estado, desde, hasta }
// opciones: { campos: [nombres de propiedades donde se busca el texto],
//             estado: (fila) => estado de la fila, fecha: (fila) => fecha de la fila }
export function filtrar(filas, filtros, opciones) {
  const texto = normalizar(filtros.texto);
  return filas.filter((fila) => {
    if (texto && !opciones.campos.some((c) => normalizar(fila[c]).includes(texto))) return false;
    if (filtros.estado && opciones.estado && normalizar(opciones.estado(fila)) !== normalizar(filtros.estado)) return false;
    if ((filtros.desde || filtros.hasta) && opciones.fecha) {
      const valor = opciones.fecha(fila);
      if (!valor) return false;
      const f = fechaCorta(valor);
      if (filtros.desde && f < filtros.desde) return false;
      if (filtros.hasta && f > filtros.hasta) return false;
    }
    return true;
  });
}

// Devuelve la página pedida (empieza en 1) y el total de páginas (mínimo 1).
export function paginar(filas, pagina, tamano = TAMANO_PAGINA) {
  const totalPaginas = Math.max(1, Math.ceil(filas.length / tamano));
  const actual = Math.min(Math.max(1, pagina), totalPaginas);
  return {
    filas: filas.slice((actual - 1) * tamano, actual * tamano),
    pagina: actual,
    totalPaginas,
  };
}
