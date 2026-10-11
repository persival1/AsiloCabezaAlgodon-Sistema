const ESTILOS_POR_ESTADO = {
  pendiente: "badge-alerta",
  asignada: "badge-info",
  atendida: "badge-exito",
  cancelada: "badge-peligro",
  activo: "badge-exito",
  inactivo: "badge-neutro",
  despachado: "badge-exito",
  resultadoregistrado: "badge-exito",
  pagada: "badge-exito",
  pendientepago: "badge-alerta",
  sincargos: "badge-neutro",
};

function Badge({ texto, estado }) {
  const clave = (estado || texto || "").toLowerCase().replace(/\s+/g, "");
  const clase = ESTILOS_POR_ESTADO[clave] || "badge-neutro";

  return <span className={`badge ${clase}`}>{texto}</span>;
}

export default Badge;
