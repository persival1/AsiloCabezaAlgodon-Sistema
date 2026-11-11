import api from "./api";

export async function listarSolicitudes() {
  const respuesta = await api.get("/Solicitudes");
  return respuesta.data;
}

export async function crearSolicitud(idInterno, motivo, especialidadRequerida) {
  const respuesta = await api.post("/Solicitudes", { idInterno, motivo, especialidadRequerida });
  return respuesta.data;
}

export async function asignarMedico(idSolicitud, idEspecialista, idEnfermero, fechaHoraCita) {
  const respuesta = await api.put(`/Solicitudes/${idSolicitud}/asignar`, {
    idEspecialista,
    idEnfermero,
    fechaHoraCita,
  });
  return respuesta.data;
}

// AC-08: el médico general corrige su solicitud pendiente; cancelar (médico o fundación).
// Reprogramar una cita usa asignarMedico sobre una solicitud Asignada.
export async function editarSolicitud(idSolicitud, motivo, especialidadRequerida) {
  await api.put(`/Solicitudes/${idSolicitud}`, { motivo, especialidadRequerida });
}

export async function cancelarSolicitud(idSolicitud) {
  const respuesta = await api.put(`/Solicitudes/${idSolicitud}/cancelar`);
  return respuesta.data;
}
