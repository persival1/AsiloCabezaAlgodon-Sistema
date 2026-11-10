// AC-13 (puntos 1 y 2): qué indicadores ve cada rol en su pantalla de inicio.
// Solo los respaldados por un requisito del enunciado (cuadro de la sección 5 de SQA-ACM-01).
// Funciones puras: reciben las listas que devuelve la API y calculan los valores, así se
// prueba que cada número coincide con los registros (__tests__/indicadores.test.js).
import { fechaCorta } from "./tabla";

const MESES = ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

export function nombreMes(hoy) {
  return `${MESES[hoy.getMonth()]} de ${hoy.getFullYear()}`;
}

// Primer y último día del mes de "hoy", en el formato de <input type="date">
export function rangoDelMes(hoy) {
  const inicio = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
  const fin = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0);
  return { desde: fechaCorta(inicio), hasta: fechaCorta(fin) };
}

const delMes = (fecha, rango) => {
  const f = fechaCorta(fecha);
  return f >= rango.desde && f <= rango.hasta;
};
const suma = (filas) => filas.reduce((t, x) => t + x.monto, 0);
const quetzales = (n) => `Q${n.toFixed(2)}`;
const porFecha = (campo) => (a, b) => new Date(a[campo]) - new Date(b[campo]);
const fechaHora = (f) => new Date(f).toLocaleString([], { dateStyle: "medium", timeStyle: "short" });

const HOY = "Al día de hoy";

// datos: las listas que necesita el rol (ver DATOS_POR_ROL en Inicio.jsx).
// Devuelve { tarjetas: [{ valor, etiqueta, periodo, ruta, requisito, alerta }], pendientes: { titulo, ruta, filas } }
export function indicadoresInicio(rol, datos, hoy = new Date()) {
  const mes = rangoDelMes(hoy);
  const periodoMes = `En ${nombreMes(hoy)}`;

  if (rol === "Administrador") {
    const porAsignar = datos.solicitudes.filter((s) => s.estado === "Pendiente");
    const porPagar = datos.visitas.filter((v) => v.estadoPago === "PendientePago");
    return {
      tarjetas: [
        { valor: porAsignar.length, etiqueta: "Solicitudes por asignar", periodo: HOY, ruta: "/dashboard/solicitudes", requisito: "Requisito 6", alerta: porAsignar.length > 0 },
        { valor: porPagar.length, etiqueta: "Visitas pendientes de pago", periodo: HOY, ruta: "/dashboard/cobros", requisito: "Requisito 14", alerta: porPagar.length > 0 },
        { valor: quetzales(datos.fundacion.saldoPendiente), etiqueta: "Adeudado a la fundación", periodo: HOY, ruta: "/dashboard/pagos-fundacion", requisito: "Requisito 13", alerta: datos.fundacion.saldoPendiente > 0 },
        { valor: quetzales(suma(datos.donaciones.filter((d) => delMes(d.fecha, mes)))), etiqueta: "Donaciones recibidas", periodo: periodoMes, ruta: "/dashboard/donaciones", requisito: "Requisito 15" },
        { valor: quetzales(suma(datos.cobros.filter((c) => delMes(c.fecha, mes)))), etiqueta: "Cobros a familiares", periodo: periodoMes, ruta: "/dashboard/cobros", requisito: "Requisito 15" },
      ],
      pendientes: {
        titulo: "Solicitudes que esperan especialista y horario",
        ruta: "/dashboard/solicitudes",
        filas: [...porAsignar].sort(porFecha("fechaCreacion")).map((s) => ({
          id: s.idSolicitud, principal: s.nombreInterno, detalle: `${s.motivo} · creada el ${new Date(s.fechaCreacion).toLocaleDateString()}`,
        })),
      },
    };
  }

  if (rol === "MedicoGeneral") {
    const porAsignar = datos.solicitudes.filter((s) => s.estado === "Pendiente");
    const programadas = datos.solicitudes.filter((s) => s.estado === "Asignada");
    return {
      tarjetas: [
        { valor: porAsignar.length, etiqueta: "Mis solicitudes sin asignar", periodo: HOY, ruta: "/dashboard/solicitudes", requisito: "Requisitos 4 y 6", alerta: porAsignar.length > 0 },
        { valor: programadas.length, etiqueta: "Citas programadas de mis solicitudes", periodo: HOY, ruta: "/dashboard/solicitudes", requisito: "Requisito 6" },
      ],
      pendientes: {
        titulo: "Próximas citas de mis internos",
        ruta: "/dashboard/solicitudes",
        filas: [...programadas].sort(porFecha("fechaHoraCita")).map((s) => ({
          id: s.idSolicitud, principal: s.nombreInterno, detalle: `${fechaHora(s.fechaHoraCita)} · ${s.nombreEspecialista || "especialista por confirmar"}`,
        })),
      },
    };
  }

  if (rol === "Especialista" || rol === "Enfermero") {
    const citas = datos.solicitudes.filter((s) => s.estado === "Asignada");
    const esEspecialista = rol === "Especialista";
    const ruta = esEspecialista ? "/dashboard/citas" : "/dashboard/acompanamientos";
    return {
      tarjetas: [
        { valor: citas.length, etiqueta: esEspecialista ? "Citas asignadas por atender" : "Acompañamientos asignados", periodo: HOY, ruta, requisito: "Requisitos 5 a 7", alerta: citas.length > 0 },
      ],
      pendientes: {
        titulo: esEspecialista ? "Mis próximas citas" : "Mis próximos acompañamientos",
        ruta,
        filas: [...citas].sort(porFecha("fechaHoraCita")).map((s) => ({
          id: s.idSolicitud, principal: s.nombreInterno, detalle: `${fechaHora(s.fechaHoraCita)} · ${s.motivo}`,
        })),
      },
    };
  }

  if (rol === "Laboratorio") {
    const pendientes = datos.examenes.filter((e) => e.estado === "Pendiente");
    return {
      tarjetas: [
        { valor: pendientes.length, etiqueta: "Exámenes pendientes de resultado", periodo: HOY, ruta: "/dashboard/examenes", requisito: "Requisito 10", alerta: pendientes.length > 0 },
      ],
      pendientes: {
        titulo: "Exámenes por registrar (los más antiguos primero)",
        ruta: "/dashboard/examenes",
        filas: [...pendientes].sort(porFecha("fechaVisita")).map((e) => ({
          id: e.idExamenSolicitado, principal: e.nombreExamen, detalle: `${e.nombreInterno} · pedido el ${new Date(e.fechaVisita).toLocaleDateString()}`,
        })),
      },
    };
  }

  if (rol === "Farmacia") {
    const pendientes = datos.prescripciones.filter((p) => p.estado === "Pendiente");
    return {
      tarjetas: [
        { valor: pendientes.length, etiqueta: "Prescripciones pendientes de despacho", periodo: HOY, ruta: "/dashboard/prescripciones", requisito: "Requisito 12", alerta: pendientes.length > 0 },
      ],
      pendientes: {
        titulo: "Medicamentos por despachar (los más antiguos primero)",
        ruta: "/dashboard/prescripciones",
        filas: [...pendientes].sort(porFecha("fechaVisita")).map((p) => ({
          id: p.idPrescripcion, principal: `${p.nombreMedicamento} (${p.dosis})`, detalle: `${p.nombreInterno} · recetado el ${new Date(p.fechaVisita).toLocaleDateString()}`,
        })),
      },
    };
  }

  return { tarjetas: [], pendientes: null };
}
