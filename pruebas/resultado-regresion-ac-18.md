# Regresión completa de la versión 1.0.0-rc2 (AC-18)

- **Fecha:** 10/11/2026
- **Código:** rama `ac-14-documentacion`, que contiene todas las ramas `ac-*` encadenadas. El último commit de código es `2bed6d6` y en esta rama se suman las pruebas de tokens.
- **Entorno:** local (Windows 11, SQL Server Express, Edge) y, después de publicar, Azure: versión 1.0.0-rc2, commit `675011f`, con la base conectada como `app_asilo`.
- **Datos:** todos ficticios. Las pruebas que crean datos usan nombres con un sello de tiempo («Prueba E2E 123456», «lab.e2e123456»).
- **Formato:** el de la sección 8 de SQA-ACM-01. El plan está en `docs/plan-de-pruebas.md`.

## Resumen

| Suite | Resultado |
|---|---|
| `dotnet test` (xUnit, con SQL Server: unitarias, integración y matriz de 282 permisos) | **408 de 408** |
| `npm test` (Vitest) | **25 de 25** |
| `npm run lint` | Sin errores |
| `npm run build` | Correcto |
| Playwright (5 archivos, Edge) | **12 de 12** |
| `pruebas/probar-permisos.ps1` contra la API local (base de desarrollo) | **277 de 277** (`pruebas/resultados-permisos.md`) |
| `pruebas/probar-permisos.ps1` contra **Azure** | **256 de 256** (`pruebas/resultados-permisos-azure.md`). Son menos casos porque Azure tiene un solo interno, así que no hay uno «ajeno» al especialista; ese caso lo cubre la base de pruebas |
| Tokens falsificados contra **Azure** | **7 de 7** rechazados con 401 (`pruebas/evidencia-ac/ac-02-tokens-falsos-azure.md`) |
| Auditorías: NuGet vulnerable / `npm audit --omit=dev` | 0 / 0 |

**Hallazgos durante la regresión, ya corregidos:**

1. **La prueba E2 (pago de la cita) estaba desactualizada.** Buscaba las visitas como tarjetas, y con AC-13 ahora son una tabla con detalle. Se actualizó la prueba; el sistema no tenía error.
2. **Fecha por defecto en UTC (AC-16).** La encontró `e2e/operaciones.spec.js`: después de las 18:00, el diagnóstico se rechazaba como «fecha futura». Se corrigió en el frontend.

La primera ejecución de E2 también chocó con el límite de 10 inicios de sesión por minuto (respuesta «Demasiados intentos»). Es el comportamiento esperado: se repitió un minuto después.

## Casos

| Identificador | Precondiciones | Datos de prueba | Procedimiento | Resultado esperado | Resultado observado | Estado | Evidencia |
|---|---|---|---|---|---|---|---|
| PR-SES-01 | API corriendo; usuarios del seed | admin / contraseña del seed | 1. Abrir el login. 2. Ingresar | Entra al inicio del Administrador | Entra | Aprobado | `navegacion.spec.js` |
| PR-SES-02 | Usuario desactivado | `lab.e2e…` | 1. Desactivarlo. 2. Intentar entrar | «Usuario o contraseña incorrectos» | Igual | Aprobado | `operaciones.spec.js`, `ac-08/usuarios-desactivado.png` |
| PR-SES-03 | — | 11 inicios de sesión en menos de un minuto | Repetir el login | El 11.º recibe «Demasiados intentos» (429) | Igual (se vio en E2) | Aprobado | Esta regresión |
| PR-SES-04 | Sesión con token vencido | Token vencido | 1. Abrir una pantalla | Vuelve al login con aviso y sin errores en la consola | Igual | Aprobado | `sesion-vencida.spec.js` |
| PR-SES-05 | — | Token firmado con otra llave, alterado (Farmacia → Administrador), vencido, de otro emisor y sin firma | `GET /api/Usuarios` con cada uno | 401 en los cinco; el token bien firmado da 200 | Igual | Aprobado | `PermisosPorRolTests.Token_*` |
| PR-SES-06 | rc2 publicada en Azure | Tokens de Administrador firmados con las 2 llaves del historial de Git, con una inventada, sin firma, vencido, basura y sin token | GET /api/Usuarios con cada uno | 401 | 401 en los 7 | Aprobado | `ac-02-tokens-falsos-azure.md` |
| PR-ROL-01 | Base de pruebas con un usuario por rol | 282 combinaciones de rol y endpoint | `dotnet test` | 200 / 401 / 403 según la matriz | 282 de 282 | Aprobado | `PermisosPorRolTests` |
| PR-ROL-02 | API local, base de desarrollo | 277 combinaciones | `pwsh pruebas/probar-permisos.ps1` | 0 fallas | 277 de 277 | Aprobado | `pruebas/resultados-permisos.md` |
| PR-ROL-03 | rc2 en Azure con `app_asilo` | Contraseñas de Azure (archivo fuera del repositorio) | `-Api https://…/api -ArchivoClaves …` | 0 fallas | 256 de 256 | Aprobado | `resultados-permisos-azure.md` |
| PR-RES-01 | Sesión de Administrador | Interno «Prueba Operaciones …» | Registrar → editar el teléfono y la cuota | Se guarda; la cuota cambia | Igual | Aprobado | `operaciones.spec.js`, `ac-08/ficha-editada.png` |
| PR-RES-02 | Existe el interno 2 | Mismos nombre y fecha que el 2 | `PUT /api/Internos/1` | 409 «Ya existe otro interno» | Igual | Aprobado | `Editar_un_interno_para_que_quede_igual_a_otro_da_409` |
| PR-RES-03 | Médico general edita un interno | Cuota 800 | `EditarInternoCommand` sin permiso de cuota | La cuota no cambia | Igual | Aprobado | `OperacionesAc08Tests` |
| PR-RES-04 | Interno sin solicitudes abiertas / con una cita asignada | — | Dar de baja | Se da de baja / 409 «Cancelalas» | Igual | Aprobado | `OperacionesAc08Tests`, `ac-08/interno-de-baja.png` |
| PR-RES-05 | Interno de baja | — | El médico abre «Nueva solicitud» | El interno no aparece; la API rechaza con 409 | Igual | Aprobado | `operaciones.spec.js`, `A_un_interno_de_baja_*` |
| PR-RES-06 | Ficha con una psicopatología | «Ansieda» | Corregir a «Ansiedad generalizada» → Quitar | Cambia y luego desaparece | Igual | Aprobado | `operaciones.spec.js` |
| PR-EXP-01 | Especialista con un paciente propio y uno ajeno | Internos 1 y 2 | `GET /Internos/{id}`, `/VisitasMedicas?idInterno=`, informes 2, 6 y 7 | 200 el propio; 403 el ajeno | Igual | Aprobado | `PermisosPorRolTests` |
| PR-EXP-02 | — | Laboratorio y Farmacia | `GET /Internos/1` | 403 | Igual | Aprobado | `PermisosPorRolTests` |
| PR-EXP-03 | Familiar con cuota de Q500 | — | Ficha como cada rol médico | Solo el Administrador recibe la cuota y el saldo; los demás `null` | Igual | Aprobado | `La_ficha_solo_muestra_finanzas_al_administrador` |
| PR-SOL-01 | Interno activo | Motivo «Control E2E …» | Médico crea → Administrador asigna | Pendiente → Asignada | Igual | Aprobado | `atencion-y-pago.spec.js` (E1) |
| PR-SOL-02 | Solicitud pendiente del médico | «Revisoin» → «Revisión E2E …» | Corregir | Se guarda; sigue Pendiente | Igual | Aprobado | `operaciones.spec.js` |
| PR-SOL-03 | Solicitud ajena o ya asignada | — | Editar | 403 / 409 | Igual | Aprobado | `OperacionesAc08Tests` |
| PR-CAN-01 | Solicitud pendiente del médico | — | Cancelar | Cancelada | Igual | Aprobado | `operaciones.spec.js`, `ac-08/solicitud-cancelada.png` |
| PR-CAN-02 | Cita asignada | — | El médico intenta cancelar; el Administrador cancela | 409 para el médico; Cancelada y correo al familiar para el Administrador | Igual | Aprobado | `OperacionesAc08Tests` |
| PR-CAN-03 | Cita asignada | Fecha nueva +9 días | Reprogramar | Cambia la fecha; correo con la fecha nueva | Igual | Aprobado | `Reprogramar_una_cita_asignada_*` |
| PR-VIS-01 | Cita asignada al especialista | Diagnóstico «Diagnóstico E2E …» | Registrar visita | La visita, el cargo y la solicitud Atendida, todo en una transacción | Igual | Aprobado | E1, `RegistrarVisitaTransaccionTests` |
| PR-VIS-02 | Falla simulada en el último guardado | — | Registrar visita | No queda nada a medias | Igual | Aprobado | `RegistrarVisitaTransaccionTests` |
| PR-LAB-01 | Examen pendiente | Resultado y costo | Registrar | Resultado registrado; costo cargado | Igual | Aprobado | E1, `RegistrarResultadoCommandHandlerTests` |
| PR-FAR-01 | Prescripción pendiente | Costo | Despachar | Despachado; costo cargado; no se despacha dos veces | Igual | Aprobado | E1, `DespacharPrescripcionCommandHandlerTests` |
| PR-COB-01 | Visita con Q192.50 en cargos | Q100 y luego Q92.50 | Dos cobros ligados a la visita | Pendiente de pago → Pagada; saldo «Al día»; la conciliación cuadra | Igual | Aprobado | `atencion-y-pago.spec.js` (E2) |
| PR-COB-02 | Visita con cargos pendientes | Un monto mayor que lo pendiente | Cobro ligado a la visita | Rechazado: «supera lo que falta pagar» | Igual | Aprobado | `CajaCommandHandlerTests`, `SaldoFamiliarTests` |
| PR-COB-03 | — | Cuota mensual de Q500 | Cobro sin visita | El saldo de cargos médicos no cambia | Igual | Aprobado | `SaldoFamiliarTests` |
| PR-FUN-01 | Adeudado a la fundación | Q200 válido; monto mayor que lo adeudado | Registrar pago | Baja lo pendiente; lo mayor se rechaza | Igual | Aprobado | `PagosFundacionTests`, `ac-07-*.png` |
| PR-DON-01 | — | Montos 0, negativos o con tres decimales | Donación o gasto | Rechazados con mensaje | Igual | Aprobado | `CajaCommandHandlerTests`, `ValidarTests` |
| PR-INF-01 | — | Los 7 informes + conciliación por rol | `GET /Reportes/*` | Clínicos para los médicos; financieros solo para el Administrador | Igual | Aprobado | `PermisosPorRolTests` |
| PR-INF-02 | Datos de E1 | — | Informes 1 y 2 en pantalla | Muestran la visita, el examen y el medicamento | Igual | Aprobado | E1 |
| PR-INF-03 | Datos de referencia de SQA | — | Valores de los informes 3 a 7 | Coinciden con los cálculos de SQA | — | **Pendiente:** los datos de referencia los tiene SQA | — |
| PR-REC-01 | Usuario con contraseña temporal | Actual equivocada; luego la correcta | Cambiar contraseña | Rechaza la equivocada; acepta la correcta; entra con la nueva | Igual | Aprobado | `operaciones.spec.js`, `ac-08/cambiar-contrasena.png` |
| PR-REC-02 | — | «soloLetras», «corta1» | Crear usuario | Rechazadas por las reglas | Igual | Aprobado | `operaciones.spec.js`, `OperacionesAc08Tests` |
| PR-REC-03 | — | — | Recuperación por enlace en el correo | — | — | **Pendiente:** exclusión propuesta (AC-05) | `docs/ac-05-contrasenas.md` |
| PR-COR-01 | Correo que falla | — | Asignar | La asignación se conserva y la pantalla dice que el correo no salió | Igual | Aprobado | `AsignarMedicoCommandHandlerTests` |
| PR-COR-02 | Gmail configurado en Azure | Correo de prueba | Asignar, reprogramar y cancelar | Llegan los tres correos | — | **Bloqueado:** falta configurar el correo en Azure (AC-11) | — |
| PR-VAL-01 | Formulario de interno | Vacío y con un correo mal escrito | Guardar | 11 mensajes junto a los campos; nada se guarda; desaparecen al corregir | Igual | Aprobado | `validacionCampos.test.js`, `ac-13/despues-formulario-invalido.png` |
| PR-UI-01 | Seis roles | — | Menú, cada opción, botón atrás, indicador | El menú coincide con la matriz; todo abre; sin errores de JavaScript | 6 de 6 | Aprobado | `navegacion.spec.js` |
| PR-UI-02 | — | 1366 × 768, 768 × 1024, 390 × 844 | Abrir el inicio de cada rol | Sin desbordes; menú arriba en el celular | Igual | Aprobado | `celular.spec.js`, `ac-13/despues-*` |
| PR-REG-01 | Versión final en Azure | — | 1. `/api/Version`. 2. Commit del frontend. 3. PR-SES-06. 4. PR-ROL-03 | rc2 / `675011f` en la API y en el frontend; sin fallas | Igual | Aprobado | Esta regresión; `docs/historial-de-versiones.md` |
