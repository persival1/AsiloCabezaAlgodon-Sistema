import { useMemo, useState } from "react";
import { filtrar, paginar, TAMANO_PAGINA } from "./tabla";

const SIN_FILTROS = { texto: "", estado: "", desde: "", hasta: "" };

// Estado de búsqueda, filtros y página de una lista. Al cambiar un filtro se vuelve a la página 1.
export function useTabla(filas, opciones, tamano = TAMANO_PAGINA) {
  const [filtros, setFiltros] = useState(SIN_FILTROS);
  const [pagina, setPagina] = useState(1);

  const filtradas = useMemo(() => filtrar(filas, filtros, opciones), [filas, filtros, opciones]);
  const resultado = paginar(filtradas, pagina, tamano);

  return {
    filtros,
    cambiarFiltro: (campo, valor) => { setFiltros((f) => ({ ...f, [campo]: valor })); setPagina(1); },
    limpiar: () => { setFiltros(SIN_FILTROS); setPagina(1); },
    hayFiltros: Object.values(filtros).some(Boolean),
    total: filas.length,
    encontradas: filtradas.length,
    filas: resultado.filas,
    pagina: resultado.pagina,
    totalPaginas: resultado.totalPaginas,
    irAPagina: setPagina,
  };
}
