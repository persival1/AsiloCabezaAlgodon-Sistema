# Respuesta a la solicitud de acciones correctivas SQA-ACM-01

| | |
|---|---|
| **Sistema** | Administración del Asilo de Ancianos Cabeza de Algodón |
| **Desarrollador** | Eraldo Josué Rojas Aguilar (3090-23-22967) |
| **Analista SQA** | Kenneth Alessandro Solórzano Gramajo (3090-22-9296) |
| **Solicitud** | SQA-ACM-01, del 10/11/2026 |
| **Versión entregada** | 1.0.0-rc2, commit `675011f`, publicada en Azure el 10/11/2026 (ver `docs/historial-de-versiones.md`) |

Los estados siguen la sección 10 de la solicitud. Ninguna acción se marca como «Cerrada», porque cerrarla requiere la validación de SQA. Lo hecho queda como **Pendiente de verificación**. Lo que todavía falta se dice con su motivo.

## Estado por acción

| AC | Hallazgo | Estado | Qué se hizo | Evidencia | Lo que falta |
|---|---|---|---|---|---|
| AC-01 | Saldo del familiar | Pendiente de verificación | Regla única: saldo = cargos médicos − pagos de visitas. No se acepta pagar más de lo pendiente. Convención Por pagar / Al día / A favor. Conciliación ajustada en las bases locales y en Azure, donde el familiar 1 pasó de −Q349.50 a Q150.50 y ahora cuadra | `docs/regla-del-saldo.md`, `SaldoFamiliarTests`, `pruebas/evidencia-ac/ac-01-*` (incluye `ac-01-ajuste-azure.txt`) | — |
| AC-02 | Llave JWT | Pendiente de verificación | En local hay seis pruebas automáticas: un token firmado con otra llave, alterado, vencido, de otro emisor o sin firma recibe 401. En Azure se comprobó que la llave de producción tiene 64 caracteres y **no coincide** con ninguna de las 2 llaves del historial de Git. También se enviaron 7 tokens de Administrador falsificados (firmados con esas 2 llaves, con una inventada, sin firma, vencido, basura y sin token), y los 7 recibieron 401. Por el criterio de la propia solicitud, la prioridad baja de crítica a media | `PermisosPorRolTests` (`Token_*`), `pruebas/evidencia-ac/ac-02-tokens-falsos-azure.md` | — |
| AC-03 | Usuario de base con permisos mínimos | Pendiente de verificación | Aplicado en Azure el 10/11. El script creó `app_asilo` con `db_datareader` y `db_datawriter`, comprobó que lee y que **no** puede crear tablas, y cambió la conexión del App Service. La cadena anterior se guardó fuera del repositorio. También se borró la regla vieja del firewall `temporal-carga-datos` | Salida del script (sin claves); permisos 256 de 256 en Azure ya con `app_asilo` (`pruebas/resultados-permisos-azure.md`) | — |
| AC-04 | Acceso horizontal y datos financieros | Pendiente de verificación | El especialista solo ve la ficha, el historial y los informes de sus pacientes. La cuota, el saldo y el estado de pago solo llegan al Administrador | `PermisosPorRolTests`; `docs/matriz-actores-y-roles.md` | — |
| AC-05 | Recuperación de contraseña | Pendiente de verificación / exclusión propuesta | Cambio de contraseña propia y contraseña temporal del Administrador. La recuperación por correo queda como exclusión propuesta | `docs/ac-05-contrasenas.md`, `OperacionesAc08Tests`, `e2e/operaciones.spec.js` | Firma de la exclusión (AC-12) |
| AC-06 | Portal de familiares | Exclusión propuesta | Justificación con el enunciado | `docs/ac-06-portal-familiares.md` | Firma de la exclusión (AC-12) |
| AC-07 | Pagos a la fundación | Pendiente de verificación | Tabla, registro con validaciones, historial e informe 4 con lo pagado y lo pendiente | `docs/pagos-a-la-fundacion.md`, `PagosFundacionTests`, `pruebas/evidencia-ac/ac-07-*` | — (la migración se aplicó en Azure el 10/11) |
| AC-08 | Operaciones del cuadro 3.5 | Pendiente de verificación | Las seis operaciones implementadas: editar, alta y baja, corregir la ficha, editar y cancelar solicitudes, reprogramar y cancelar citas, usuarios | `docs/ac-08-operaciones.md`, `OperacionesAc08Tests`, `e2e/operaciones.spec.js` | — |
| AC-09 | Actores y roles | Pendiente de verificación | Matriz de correspondencia con justificación y permisos por rol | `docs/matriz-actores-y-roles.md` | Aprobación (AC-12) |
| AC-10 | Permisos de los informes | Pendiente de verificación | Informes clínicos (2, 6 y 7) para los médicos, por paciente; financieros solo para el Administrador | `PermisosPorRolTests` | — |
| AC-11 | Correo en Azure | Pendiente de verificación | El desarrollador configuró en Azure la cuenta de correo, con una contraseña de aplicación de Google. Prueba real el 10/11: al asignar y al cancelar una cita de prueba llegaron los dos correos a las 22:43 | `pruebas/evidencia-ac/ac-11-correo-azure.md` | — |
| AC-12 | Supuestos y exclusiones sin aprobar | Abierta | Documentos listos para firmar | `docs/supuestos-para-aprobacion.md`, `ac-05`, `ac-06`, `matriz-actores-y-roles.md` | Firma del docente o de SQA |
| AC-13 | Interfaz | Pendiente de verificación | Los diez puntos de la sección 5, con capturas antes y después en tres resoluciones | `docs/ac-13-interfaz.md`, `e2e/navegacion.spec.js`, `pruebas/evidencia-ac/ac-13/` | Validación de SQA |
| AC-14 | Documentación | Pendiente de verificación (parcial) | 12 de los 13 documentos actualizados en el repositorio (tabla de abajo). Los documentos de las fases (Análisis, Diseño, Desarrollo y Capturas) se rehicieron en Word y PDF como versión 2 | Ver tabla | Documento 1 (Word de la Fase de Análisis) y documento 8 (instalación en un equipo limpio) |
| AC-15 | Commits y versión | En corrección | Una rama por acción con commits descriptivos. Versión y commit visibles en la aplicación y en `/api/Version`. Publicada en Azure: `/api/Version` responde `1.0.0-rc2` / `675011f…`, y el frontend muestra el mismo commit | `docs/historial-de-versiones.md`, `docs/registro-de-cambios-sqa.md` | Pull requests revisados y etiqueta `v1.0.0-rc2` (los hace el desarrollador desde GitHub Desktop) |
| AC-16 | Fechas | Pendiente de verificación | La fecha por defecto del frontend era UTC; ahora es la local. En Azure se revisaron todas las fechas guardadas (con una copia previa de los valores): ninguna cae entre el 21 y el 30/10. Todos los registros son de septiembre o de principios de octubre, copiados de la base local, que siempre guardó la hora de Guatemala. El script de corrección, en vista previa, encontró 0 filas, así que no hubo nada que cambiar | `base-de-datos/corregir-fechas-utc-ac-16.sql` | — |
| AC-17 | Consola en sesión vencida | Pendiente de verificación | Sin errores sin manejar | `e2e/sesion-vencida.spec.js` | — |
| AC-18 | Regresión completa | Pendiente de verificación | Plan único y regresión con el formato de la sección 8: completa en local y, en Azure, los permisos (256 de 256), los tokens falsos y la versión publicada | `docs/plan-de-pruebas.md`, `pruebas/resultado-regresion-ac-18.md` | Valores de los informes 3 a 7 con los datos de referencia de SQA |

## Documentos de la sección 7

| N° | Documento | Dónde | Estado |
|---|---|---|---|
| 1 | Análisis de requerimientos | Word y PDF «Fase de Análisis», versión 2 (10/11), en la carpeta de entrega | Actualizado: CU-22 a CU-30, la tabla PagosFundacion, 6 requerimientos y 7 reglas nuevas, exclusiones y la aclaración «Nueva Vida» / «Cabeza de Algodón» |
| 2 | Catálogo de casos de uso | `docs/catalogo-casos-de-uso.md` | Actualizado |
| 3 | Matriz de trazabilidad | `docs/matriz-de-trazabilidad.md` (sección H nueva) | Actualizado |
| 4 | Modelo entidad-relación | `AsiloCabezaAlgodon/modelo-er.html` (14 tablas) | Actualizado |
| 5 | Diagrama de componentes | `AsiloCabezaAlgodon/diagrama-componentes.html` | Actualizado |
| 6 | Manual técnico | `docs/manual-tecnico.md` | Nuevo |
| 7 | Manual de usuario por rol | `docs/manual-de-usuario.md` | Nuevo |
| 8 | Instalación y configuración | `README.md`, `despliegue/LEEME.md`, `pruebas/resultado-instalacion.md` | Probada desde una carpeta nueva el 10/11. Faltaban tres requisitos, que ya se agregaron al README. **Pendiente:** la prueba en otro equipo, que hace el desarrollador |
| 9 | Errores y limitaciones | `docs/errores-y-limitaciones.md` | Nuevo |
| 10 | Plan de pruebas | `docs/plan-de-pruebas.md` | Nuevo |
| 11 | Resultados de pruebas | `pruebas/resultado-regresion-ac-18.md` | Nuevo, con el formato de la sección 8 |
| 12 | Registro de cambios | `docs/registro-de-cambios-sqa.md` (segunda parte) | Actualizado |
| 13 | Historial de versiones | `docs/historial-de-versiones.md` | Nuevo |

## Cambios hechos en Azure el 10/11/2026 (con aprobación del desarrollador)

1. Se guardó una copia de los saldos y las fechas antes de tocar nada (fuera del repositorio).
2. Se revisaron las fechas UTC: 0 filas por corregir (AC-16).
3. Migración de pagos a la fundación (AC-07).
4. Conciliación de saldos (AC-01).
5. Revisión de la llave JWT (AC-02).
6. Publicación de la 1.0.0-rc2, commit `675011f`, con respaldo previo del sitio (AC-15).
7. Prueba de tokens falsificados (AC-02).
8. Usuario `app_asilo` y cierre de la regla vieja del firewall (AC-03).
9. Permisos: 256 de 256 (AC-18).
10. Correo real (AC-11). El desarrollador configuró Gmail. Se probó con el interno «PRUEBA CORREO AC-11» (id 2), al que se le asignó y canceló una cita (solicitud id 4) y llegaron los dos correos. Al terminar quedó de baja y sin el correo real.

**Pendiente del desarrollador:**
- Conseguir las firmas (AC-12).
- Pull requests y etiqueta (AC-15).
- Probar la instalación en un equipo limpio (AC-14).
