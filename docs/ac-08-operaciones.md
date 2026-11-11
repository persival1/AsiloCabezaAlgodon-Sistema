# AC-08: operaciones sobre internos, solicitudes, citas y usuarios

Respuesta al cuadro 3.5 de SQA-ACM-01. Las seis operaciones quedaron implementadas, validadas en el servidor y probadas. No hizo falta cambiar la base de datos: se usan los campos `Estado` que ya existían.

- **Rama:** `ac-08-operaciones`
- **Evidencia:** `pruebas/evidencia-ac/ac-08/` (capturas tomadas por la prueba de navegador, en la base local de desarrollo)

## Operaciones

| Operación | CU | Endpoint | Quién puede | Reglas | Pruebas |
|---|---|---|---|---|---|
| Editar datos del interno y su familiar | CU-006 | `PUT /api/Internos/{id}` | Administrador, Médico general | Mismas validaciones que al registrar. La cuota mensual solo la cambia el Administrador; si la manda otro rol, se ignora. No puede quedar igual a otro interno | `Editar_interno_*` (4); `Editar_un_interno_para_que_quede_igual_a_otro_da_409` (SQL Server real); e2e |
| Alta o baja del interno | CU-007 | `PUT /api/Internos/{id}/estado` con `activo` o `baja` | Administrador | La baja no borra nada (ficha, visitas y cobros quedan para los informes). No se da de baja con solicitudes Pendientes o citas Asignadas. A un interno de baja no se le crean solicitudes | `Baja_*`, `Estado_desconocido_*`, `A_un_interno_de_baja_*`; e2e |
| Evitar internos duplicados | CU-005 | `POST /api/Internos` | — | Mismos nombres, apellidos y fecha de nacimiento (sin importar mayúsculas) dan 409 | `Registrar_un_interno_duplicado_se_rechaza` |
| Corregir o quitar psicopatologías y medicinas de cajón | CU-009, CU-010 | `PUT` y `DELETE /api/Internos/{id}/psicopatologias/{idP}` y `.../medicamentos-cajon/{idM}` | Administrador, Médico general, Especialista (solo sus pacientes) | El registro debe ser del interno de la ruta. El especialista recibe 403 en fichas ajenas | `Corregir_*` (2); matriz de permisos; e2e |
| Editar o cancelar una solicitud pendiente | CU-013, CU-014 | `PUT /api/Solicitudes/{id}` y `PUT /api/Solicitudes/{id}/cancelar` | Editar: el médico general que la creó. Cancelar: ese médico (Pendiente) o el Administrador | Solo se edita en estado Pendiente. Una solicitud Atendida no se cancela | `El_medico_edita_*`, `Editar_una_solicitud_ajena_*`, `Una_solicitud_atendida_*`; e2e |
| Reprogramar o cancelar una cita asignada | CU-018, CU-019 | Reprogramar: `PUT /api/Solicitudes/{id}/asignar` (ya aceptaba solicitudes Asignadas). Cancelar: `PUT /api/Solicitudes/{id}/cancelar` | Administrador (la fundación) | La fecha nueva no puede estar en el pasado. En los dos casos se avisa al familiar por correo, y la pantalla dice si el correo salió | `Reprogramar_una_cita_asignada_*`, `El_medico_no_cancela_una_cita_ya_asignada_*` |
| Crear, modificar y desactivar usuarios | CU-004 | `POST /api/Usuarios`, `PUT /api/Usuarios/{id}`, `PUT /api/Usuarios/{id}/estado` | Administrador | El usuario es único, en minúsculas. Contraseña de 8 o más caracteres, con letras y números. El especialista necesita especialidad. Nadie se desactiva a sí mismo y siempre queda un Administrador activo. Un usuario inactivo no inicia sesión ni recibe citas nuevas | `Crear_usuario_*` (7), `No_se_desactiva_*`, `Desactivar_y_editar_*`; e2e |

Las contraseñas (AC-05) están en `docs/ac-05-contrasenas.md`.

## Pantallas

- **Ficha del interno:** botones «Editar datos» (Administrador y médico general) y «Dar de baja» o «Reactivar» (Administrador). Cada psicopatología y medicina de cajón tiene «Corregir» y «Quitar», y quitar pide confirmación.
- **Solicitudes:** el médico general ve «Corregir» y «Cancelar» en sus solicitudes pendientes. El Administrador ve «Asignar», «Reprogramar» y «Cancelar» según el estado. Los internos de baja no aparecen al crear una solicitud.
- **Usuarios** (menú Administración): lista con búsqueda y filtro por rol, y los botones «Nuevo usuario», «Editar», «Contraseña» y «Desactivar» o «Activar».

## Permisos

La matriz automática pasó de 218 a **282 casos**, con cada operación nueva probada con los roles que no pueden usarla y sin sesión. También se prueba que el especialista no pueda borrar en la ficha de un interno ajeno. El detalle está en `docs/matriz-actores-y-roles.md`.

## Resultado de las pruebas

- `dotnet test`: 402 de 402.
- `npx playwright test e2e/operaciones.spec.js`: 2 de 2.

## Corrección encontrada durante la prueba (AC-16)

La fecha que la ficha y los reportes ponen por defecto se calculaba en UTC (`toISOString`). Después de las 18:00 en Guatemala eso ya es el día siguiente, y el servidor rechazaba el diagnóstico como «fecha futura». Ahora se usa la fecha local del navegador en `FichaInterno.jsx`, `EditarInterno.jsx` y `Reportes.jsx`.
