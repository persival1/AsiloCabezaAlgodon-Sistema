# Respuesta a la solicitud de acciones correctivas SQA-ACM-01

| | |
|---|---|
| **Sistema** | Administración del Asilo de Ancianos Cabeza de Algodón |
| **Desarrollador** | Eraldo Josué Rojas Aguilar (3090-23-22967) |
| **Analista SQA** | Kenneth Alessandro Solórzano Gramajo (3090-22-9296) |
| **Solicitud** | SQA-ACM-01, del 10/11/2026 |
| **Versión entregada** | 1.0.0-rc2 (ramas `ac-*`, ver `docs/historial-de-versiones.md`) |

Los estados siguen la sección 10 de la solicitud. Ninguna acción se marca como «Cerrada», porque cerrarla requiere la validación de SQA. Lo hecho queda como **Pendiente de verificación**. Lo que todavía falta se dice con su motivo.

## Estado por acción

| AC | Hallazgo | Estado | Qué se hizo | Evidencia | Lo que falta |
|---|---|---|---|---|---|
| AC-01 | Saldo del familiar | Pendiente de verificación | Regla única: saldo = cargos médicos − pagos de visitas. No se acepta pagar más de lo pendiente. Convención Por pagar / Al día / A favor. Conciliación ajustada | `docs/regla-del-saldo.md`, `SaldoFamiliarTests`, `pruebas/evidencia-ac/ac-01-*` | Ejecutar `base-de-datos/conciliar-saldos.sql` en Azure (necesita aprobación) |
| AC-02 | Llave JWT | En corrección | Seis pruebas automáticas: un token firmado con otra llave, alterado, vencido, de otro emisor o sin firma recibe 401 | `PermisosPorRolTests` (`Token_*`) | Repetir en Azure y comprobar que la llave de producción no es la publicada (necesita aprobación) |
| AC-03 | Usuario de base con permisos mínimos | Abierta | El script está listo desde la rc1 | `despliegue/aplicar-usuario-minimo.ps1` | Aplicarlo en Azure (necesita aprobación) |
| AC-04 | Acceso horizontal y datos financieros | Pendiente de verificación | El especialista solo ve la ficha, el historial y los informes de sus pacientes. La cuota, el saldo y el estado de pago solo llegan al Administrador | `PermisosPorRolTests`; `docs/matriz-actores-y-roles.md` | — |
| AC-05 | Recuperación de contraseña | Pendiente de verificación / exclusión propuesta | Cambio de contraseña propia y contraseña temporal del Administrador. La recuperación por correo queda como exclusión propuesta | `docs/ac-05-contrasenas.md`, `OperacionesAc08Tests`, `e2e/operaciones.spec.js` | Firma de la exclusión (AC-12) |
| AC-06 | Portal de familiares | Exclusión propuesta | Justificación con el enunciado | `docs/ac-06-portal-familiares.md` | Firma de la exclusión (AC-12) |
| AC-07 | Pagos a la fundación | Pendiente de verificación | Tabla, registro con validaciones, historial e informe 4 con lo pagado y lo pendiente | `docs/pagos-a-la-fundacion.md`, `PagosFundacionTests`, `pruebas/evidencia-ac/ac-07-*` | Aplicar `migracion-ac-07-pagos-fundacion.sql` en Azure al publicar |
| AC-08 | Operaciones del cuadro 3.5 | Pendiente de verificación | Las seis operaciones implementadas: editar, alta y baja, corregir la ficha, editar y cancelar solicitudes, reprogramar y cancelar citas, usuarios | `docs/ac-08-operaciones.md`, `OperacionesAc08Tests`, `e2e/operaciones.spec.js` | — |
| AC-09 | Actores y roles | Pendiente de verificación | Matriz de correspondencia con justificación y permisos por rol | `docs/matriz-actores-y-roles.md` | Aprobación (AC-12) |
| AC-10 | Permisos de los informes | Pendiente de verificación | Informes clínicos (2, 6 y 7) para los médicos, por paciente; financieros solo para el Administrador | `PermisosPorRolTests` | — |
| AC-11 | Correo en Azure | Abierta | El sistema informa con honestidad si el correo no salió | `docs/errores-y-limitaciones.md` (LIM-01) | Configurar la cuenta de Gmail en Azure (lo hace el desarrollador con sus credenciales) |
| AC-12 | Supuestos y exclusiones sin aprobar | Abierta | Documentos listos para firmar | `docs/supuestos-para-aprobacion.md`, `ac-05`, `ac-06`, `matriz-actores-y-roles.md` | Firma del docente o de SQA |
| AC-13 | Interfaz | Pendiente de verificación | Los diez puntos de la sección 5, con capturas antes y después en tres resoluciones | `docs/ac-13-interfaz.md`, `e2e/navegacion.spec.js`, `pruebas/evidencia-ac/ac-13/` | Validación de SQA |
| AC-14 | Documentación | Pendiente de verificación (parcial) | 11 de los 13 documentos actualizados en el repositorio (tabla de abajo) | Ver tabla | Documento 1 (Word de la Fase de Análisis) y documento 8 (instalación en un equipo limpio) |
| AC-15 | Commits y versión | En corrección | Una rama por acción con commits descriptivos. Versión y commit visibles en la aplicación y en `/api/Version`. El despliegue pasa el commit | `docs/historial-de-versiones.md`, `docs/registro-de-cambios-sqa.md` | Pull requests revisados, etiqueta `v1.0.0-rc2` y publicación en Azure |
| AC-16 | Fechas | En corrección | La fecha por defecto del frontend era UTC; ahora es la local. Script con vista previa para las fechas antiguas de Azure | `base-de-datos/corregir-fechas-utc-ac-16.sql` | Ejecutarlo en Azure con las horas exactas (necesita aprobación) |
| AC-17 | Consola en sesión vencida | Pendiente de verificación | Sin errores sin manejar | `e2e/sesion-vencida.spec.js` | — |
| AC-18 | Regresión completa | Pendiente de verificación (local) | Plan único y regresión ejecutada en local con el formato de la sección 8 | `docs/plan-de-pruebas.md`, `pruebas/resultado-regresion-ac-18.md` | Repetir en Azure después de publicar |

## Documentos de la sección 7

| N° | Documento | Dónde | Estado |
|---|---|---|---|
| 1 | Análisis de requerimientos | Word «Fase de Análisis» (06/11) | **Pendiente:** agregar los CU-22 a CU-30 y las exclusiones. La aclaración «Nueva Vida» / «Cabeza de Algodón» ya está en el catálogo |
| 2 | Catálogo de casos de uso | `docs/catalogo-casos-de-uso.md` | Actualizado |
| 3 | Matriz de trazabilidad | `docs/matriz-de-trazabilidad.md` (sección H nueva) | Actualizado |
| 4 | Modelo entidad-relación | `AsiloCabezaAlgodon/modelo-er.html` (14 tablas) | Actualizado |
| 5 | Diagrama de componentes | `AsiloCabezaAlgodon/diagrama-componentes.html` | Actualizado |
| 6 | Manual técnico | `docs/manual-tecnico.md` | Nuevo |
| 7 | Manual de usuario por rol | `docs/manual-de-usuario.md` | Nuevo |
| 8 | Instalación y configuración | `README.md`, `despliegue/LEEME.md` | **Pendiente:** validarla en un equipo limpio |
| 9 | Errores y limitaciones | `docs/errores-y-limitaciones.md` | Nuevo |
| 10 | Plan de pruebas | `docs/plan-de-pruebas.md` | Nuevo |
| 11 | Resultados de pruebas | `pruebas/resultado-regresion-ac-18.md` | Nuevo, con el formato de la sección 8 |
| 12 | Registro de cambios | `docs/registro-de-cambios-sqa.md` (segunda parte) | Actualizado |
| 13 | Historial de versiones | `docs/historial-de-versiones.md` | Nuevo |

## Acciones que necesitan aprobación para tocar Azure

Ninguna de estas se ejecutó todavía:

1. Aplicar `app_asilo` (AC-03).
2. Prueba del token falsificado y revisión de la llave (AC-02).
3. Ejecutar `conciliar-saldos.sql` (AC-01) y `migracion-ac-07-pagos-fundacion.sql` (AC-07).
4. Corregir las fechas UTC (AC-16).
5. Publicar la 1.0.0-rc2 (AC-15) y repetir la regresión allí (AC-18).
