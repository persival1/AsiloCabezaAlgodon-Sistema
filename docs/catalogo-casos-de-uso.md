# Catálogo de casos de uso con estado de implementación (AC-14, documento 2)

Sistema para Administración del Asilo de Ancianos **Cabeza de Algodón**.

## Aclaración de nombres («Nueva Vida» y «Cabeza de Algodón»)

El análisis que revisó SQA se titulaba «Nueva Vida». Era un trabajo de Análisis de Sistemas I con 8 actores, 64 casos de uso y 22 tablas, y **no describe este sistema**. El análisis vigente es la *Fase de Análisis* de Cabeza de Algodón (06/11/2026), con los casos CU-01 a CU-21.

SQA cita los números del catálogo «Nueva Vida», con tres cifras (CU-004, CU-006…). Para no confundirlos, aquí se escriben como **NV-CU-xxx**. Los casos nuevos de este sistema siguen la numeración propia desde CU-22.

Estados posibles:

- **Implementado:** está en el código y tiene pruebas.
- **Excluido (propuesto):** necesita firma del docente o de SQA (AC-12).
- **Pendiente:** todavía no está hecho.

## Casos de uso del análisis vigente

| CU | Nombre | Rol | Estado | Pantalla | Endpoint |
|---|---|---|---|---|---|
| CU-01 | Iniciar sesión | Todos | Implementado | Login | `POST /api/Auth/login` |
| CU-02 | Cerrar sesión | Todos | Implementado | Menú, «Cerrar sesión» | (en el navegador) |
| CU-03 | Registrar interno y su familiar | Administrador, Médico general | Implementado; desde AC-08 rechaza duplicados | Internos | `POST /api/Internos` |
| CU-04 | Consultar internos | Administrador, Médico general, Especialista (solo sus pacientes) | Implementado | Internos / Mis pacientes | `GET /api/Internos` |
| CU-05 | Consultar ficha médica e historial | Los mismos tres; el especialista solo sus pacientes | Implementado | Ficha del interno | `GET /api/Internos/{id}`, `GET /api/VisitasMedicas?idInterno=` |
| CU-06 | Registrar psicopatología | Los mismos tres | Implementado | Ficha | `POST /api/Internos/{id}/psicopatologias` |
| CU-07 | Registrar medicamento de cajón | Los mismos tres | Implementado | Ficha | `POST /api/Internos/{id}/medicamentos-cajon` |
| CU-08 | Consultar visitas médicas | Administrador, Médico general, Especialista (las suyas) | Implementado | Visitas médicas | `GET /api/VisitasMedicas` |
| CU-09 | Crear solicitud de atención | Médico general | Implementado | Solicitudes | `POST /api/Solicitudes` |
| CU-10 | Asignar especialista, enfermero(a) y horario | Administrador (fundación) | Implementado | Solicitudes | `PUT /api/Solicitudes/{id}/asignar` |
| CU-11 | Enviar correo al familiar | Sistema | Implementado y probado en Azure con un envío real (AC-11, 10/11) | — | (dentro de CU-10, CU-27 y CU-28) |
| CU-12 | Consultar mis citas | Especialista | Implementado | Mis citas | `GET /api/Solicitudes` |
| CU-13 | Registrar visita médica | Especialista (solo sus citas) | Implementado | Mis citas | `POST /api/VisitasMedicas` |
| CU-14 | Consultar mis acompañamientos | Enfermero(a) | Implementado | Mis acompañamientos | `GET /api/Solicitudes` |
| CU-15 | Registrar resultado de examen | Laboratorio | Implementado | Exámenes solicitados | `PUT /api/Examenes/{id}/resultado` |
| CU-16 | Despachar medicamento | Farmacia | Implementado | Prescripciones | `PUT /api/Prescripciones/{id}/despachar` |
| CU-17 | Cargar costo a la cuenta del familiar | Sistema | Implementado, con la regla del saldo de AC-01 | — | (dentro de CU-13, CU-15 y CU-16) |
| CU-18 | Registrar cobro al familiar | Administrador | Implementado; no acepta pagar más de lo pendiente de la visita (AC-01) | Cobros | `POST /api/Cobros` |
| CU-19 | Registrar donación | Administrador | Implementado | Donaciones | `POST /api/Donaciones` |
| CU-20 | Registrar gasto | Administrador | Implementado | Gastos | `POST /api/Gastos` |
| CU-21 | Generar informe | Administrador (los 7). Médico general y especialista: informes 2, 6 y 7 (AC-10) | Implementado | Reportes | `GET /api/Reportes/...` |

## Casos agregados por la solicitud SQA-ACM-01

| CU | Nombre | Equivale a | Acción | Estado | Endpoint |
|---|---|---|---|---|---|
| CU-22 | Registrar pago a la fundación | — (supuesto 7) | AC-07 | Implementado | `POST /api/PagosFundacion` |
| CU-23 | Editar datos del interno | NV-CU-006 | AC-08 | Implementado | `PUT /api/Internos/{id}` |
| CU-24 | Dar de baja o reactivar a un interno | NV-CU-007 | AC-08 | Implementado | `PUT /api/Internos/{id}/estado` |
| CU-25 | Corregir o quitar psicopatologías y medicamentos de cajón | NV-CU-009, NV-CU-010 | AC-08 | Implementado | `PUT` y `DELETE /api/Internos/{id}/...` |
| CU-26 | Corregir una solicitud pendiente | NV-CU-013 | AC-08 | Implementado | `PUT /api/Solicitudes/{id}` |
| CU-27 | Cancelar una solicitud o cita | NV-CU-014, NV-CU-019 | AC-08 | Implementado | `PUT /api/Solicitudes/{id}/cancelar` |
| CU-28 | Reprogramar una cita | NV-CU-018 | AC-08 | Implementado (reutiliza CU-10) | `PUT /api/Solicitudes/{id}/asignar` |
| CU-29 | Administrar usuarios | NV-CU-004 | AC-08 | Implementado | `POST`, `PUT /api/Usuarios...` |
| CU-30 | Cambiar contraseña / contraseña temporal | — | AC-05 | Implementado | `PUT /api/Auth/cambiar-clave`, `PUT /api/Usuarios/{id}/clave` |

## Funciones excluidas (propuestas)

| Función | Origen | Acción | Estado | Justificación |
|---|---|---|---|---|
| Portal de familiares (rol Familiar) | Análisis «Nueva Vida» | AC-06 | Excluido (propuesto) | `docs/ac-06-portal-familiares.md` |
| Recuperación de contraseña por enlace en el correo | Revisión SQA 3.2 | AC-05 | Excluido (propuesto) | `docs/ac-05-contrasenas.md` |
| Rol Director | Análisis «Nueva Vida» | AC-09 | Excluido (propuesto) | `docs/matriz-actores-y-roles.md` |
| Coordinador de agenda como rol aparte | Análisis «Nueva Vida» | AC-09 | Integrado en Administrador (supuesto 6) | `docs/matriz-actores-y-roles.md` |
