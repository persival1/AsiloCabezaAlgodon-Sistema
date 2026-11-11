// AC-13 (punto 6): barra de búsqueda y filtros, y paginación, iguales en todas las listas.
// "tabla" es lo que devuelve useTabla.

export function FiltrosTabla({ tabla, placeholder, estados, conFechas = true, etiquetaFecha = "Fecha" }) {
  const { filtros, cambiarFiltro, limpiar, hayFiltros } = tabla;
  return (
    <div className="filtros-tabla" role="search">
      <div className="campo campo-busqueda">
        <label htmlFor="filtro-texto">Buscar</label>
        <input
          id="filtro-texto"
          type="search"
          placeholder={placeholder}
          value={filtros.texto}
          onChange={(e) => cambiarFiltro("texto", e.target.value)}
        />
      </div>
      {estados && (
        <div className="campo">
          <label htmlFor="filtro-estado">Estado</label>
          <select id="filtro-estado" value={filtros.estado} onChange={(e) => cambiarFiltro("estado", e.target.value)}>
            <option value="">Todos</option>
            {estados.map((e) => <option key={e.valor} value={e.valor}>{e.texto}</option>)}
          </select>
        </div>
      )}
      {conFechas && (
        <>
          <div className="campo">
            <label htmlFor="filtro-desde">{etiquetaFecha} desde</label>
            <input id="filtro-desde" type="date" value={filtros.desde} onChange={(e) => cambiarFiltro("desde", e.target.value)} />
          </div>
          <div className="campo">
            <label htmlFor="filtro-hasta">{etiquetaFecha} hasta</label>
            <input id="filtro-hasta" type="date" value={filtros.hasta} min={filtros.desde || undefined} onChange={(e) => cambiarFiltro("hasta", e.target.value)} />
          </div>
        </>
      )}
      {hayFiltros && (
        <button type="button" className="boton-secundario" onClick={limpiar}>Limpiar filtros</button>
      )}
    </div>
  );
}

export function PaginacionTabla({ tabla, nombre = "registros" }) {
  const { pagina, totalPaginas, irAPagina, encontradas, total } = tabla;
  return (
    <div className="paginacion">
      <span className="paginacion-resumen">
        {encontradas === total ? `${total} ${nombre}` : `${encontradas} de ${total} ${nombre}`}
      </span>
      {totalPaginas > 1 && (
        <div className="paginacion-botones">
          <button type="button" className="boton-secundario" disabled={pagina === 1} onClick={() => irAPagina(pagina - 1)}>
            Anterior
          </button>
          <span>Página {pagina} de {totalPaginas}</span>
          <button type="button" className="boton-secundario" disabled={pagina === totalPaginas} onClick={() => irAPagina(pagina + 1)}>
            Siguiente
          </button>
        </div>
      )}
    </div>
  );
}
