import api from "./api";

// AC-07: pagos que el asilo le hace a la fundación
export async function listarPagosFundacion() {
  const respuesta = await api.get("/PagosFundacion");
  return respuesta.data;
}

export async function registrarPagoFundacion(monto, numeroComprobante, descripcion) {
  const respuesta = await api.post("/PagosFundacion", { monto, numeroComprobante, descripcion });
  return respuesta.data;
}
