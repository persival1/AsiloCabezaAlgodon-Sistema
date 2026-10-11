import api from "./api";

export async function reporteCostosPorCita() {
  const respuesta = await api.get("/Reportes/costos-por-cita");
  return respuesta.data;
}

export async function reporteFichaMedica(idInterno) {
  const respuesta = await api.get(`/Reportes/ficha-medica/${idInterno}`);
  return respuesta.data;
}

export async function reporteCobrosPorPaciente(idInterno, fechaInicio, fechaFin) {
  const respuesta = await api.get("/Reportes/cobros-por-paciente", {
    params: { idInterno, fechaInicio, fechaFin },
  });
  return respuesta.data;
}

export async function reportePagosFundacion(fechaInicio, fechaFin) {
  const respuesta = await api.get("/Reportes/pagos-fundacion", {
    params: { fechaInicio, fechaFin },
  });
  return respuesta.data;
}

export async function reporteEntradas(fechaInicio, fechaFin) {
  const respuesta = await api.get("/Reportes/entradas", {
    params: { fechaInicio, fechaFin },
  });
  return respuesta.data;
}

export async function reporteExamenesPorPaciente(idInterno) {
  const respuesta = await api.get(`/Reportes/examenes-por-paciente/${idInterno}`);
  return respuesta.data;
}

export async function reporteMedicamentosPorPaciente(idInterno) {
  const respuesta = await api.get(`/Reportes/medicamentos-por-paciente/${idInterno}`);
  return respuesta.data;
}
