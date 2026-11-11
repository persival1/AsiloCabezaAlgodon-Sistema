# Plan de pruebas consolidado (AC-14, documento 10; AC-18)

Plan único para la versión 1.0.0-rc2. Reúne lo que antes estaba repartido en los resultados por semana (`pruebas/resultado-semana-*.md`). Los resultados de su ejecución están en `pruebas/resultado-regresion-ac-18.md`, con el formato de la sección 8 de SQA-ACM-01.

## 1. Alcance

- **Entra:** todos los casos de uso de `docs/catalogo-casos-de-uso.md` marcados como implementados, los permisos por rol (vertical y horizontal), las validaciones, los siete informes, la interfaz por rol y la regresión completa.
- **No entra:**
  - Las funciones excluidas (AC-05, recuperación por correo; AC-06, portal de familiares).

## 2. Entornos y datos

| Entorno | Base | Para qué |
|---|---|---|
| Pruebas automáticas de integración | `asilo_pruebas_*`: se borra y se crea en cada ejecución | xUnit con SQL Server real (permisos, transacciones, saldos) |
| Local de desarrollo | `asilo_cabeza_algodon` | Playwright y `probar-permisos.ps1`. Las pruebas crean datos con un sello de tiempo en el nombre |
| Local de capturas | `asilo_capturas` (datos ficticios) | Capturas para documentos y evidencia de AC-13 |
| Azure | Producción | Solo lectura y permisos. Ninguna prueba que cree datos sin registros marcados como de prueba y sin aprobación |

Todos los datos de prueba son ficticios: nombres inventados y correos `@prueba.test` o `@asilo.gt` del seed.

## 3. Herramientas

| Herramienta | Qué cubre | Comando |
|---|---|---|
| xUnit + NSubstitute | Reglas de negocio de cada handler | `dotnet test` |
| xUnit + WebApplicationFactory + SQL Server | Matriz de permisos (282 casos), tokens, acceso horizontal, duplicados, saldos y transacciones | `dotnet test` (sin filtro) |
| Vitest | Funciones del frontend: sesión, filtros, indicadores, validación | `npm test` |
| ESLint | Calidad del código del frontend | `npm run lint` |
| Playwright (Edge) | Recorridos en el navegador por rol | `npx playwright test e2e/<archivo>` |
| `pruebas/probar-permisos.ps1` | Permisos contra una API que está corriendo (local o Azure) | `pwsh pruebas/probar-permisos.ps1 -Api <url>` |
| GitHub Actions | Compilación en Linux, pruebas sin SQL Server, lint y auditorías | Al hacer push |

## 4. Áreas y casos

| Código | Área | Casos | Automatizado en |
|---|---|---|---|
| PR-SES | Sesión | Login correcto e incorrecto, usuario inactivo, límite de intentos, sesión vencida, token falsificado, alterado, vencido, de otro emisor o sin firma | `PermisosPorRolTests` (tokens), `sesion-vencida.spec.js`, `operaciones.spec.js` |
| PR-ROL | Permisos por rol | Matriz de 282 casos (200, 401 y 403) | `PermisosPorRolTests`, `probar-permisos.ps1` |
| PR-RES | Residentes | Registrar, duplicado, editar, cuota solo del Administrador, baja y alta, corregir la ficha | `OperacionesAc08Tests`, `CrearInternoCommandHandlerTests`, `operaciones.spec.js` |
| PR-EXP | Expedientes | El especialista solo ve a sus pacientes; Laboratorio y Farmacia no ven la ficha; las finanzas solo las ve el Administrador | `PermisosPorRolTests` |
| PR-SOL | Solicitudes y asignaciones | Crear, asignar, especialista con el rol correcto, fecha no pasada, editar la propia pendiente | `AsignarMedicoCommandHandlerTests`, `OperacionesAc08Tests`, `atencion-y-pago.spec.js` |
| PR-CAN | Cancelación y reprogramación | El médico cancela la suya pendiente; la fundación cancela o reprograma la cita y avisa al familiar; una solicitud atendida no se cancela | `OperacionesAc08Tests`, `AsignarMedicoCommandHandlerTests`, `operaciones.spec.js` |
| PR-VIS | Visitas | Solo en citas propias, transacción completa, historial ajeno con 403 | `RegistrarVisitaCommandHandlerTests`, `RegistrarVisitaTransaccionTests`, `PermisosPorRolTests` |
| PR-LAB | Laboratorio | Registrar resultado una sola vez, con costo válido | `RegistrarResultadoCommandHandlerTests`, `atencion-y-pago.spec.js` |
| PR-FAR | Farmacia | Despachar una sola vez; el costo se carga al familiar | `DespacharPrescripcionCommandHandlerTests`, `atencion-y-pago.spec.js` |
| PR-COB | Cobros y saldos | No se paga más de lo pendiente de una visita; la cuota no cambia el saldo; el saldo cuadra en la conciliación | `CajaCommandHandlerTests`, `SaldoFamiliarTests`, `atencion-y-pago.spec.js` (E2) |
| PR-FUN | Pagos a la fundación | Registrar un pago, no pagar de más, el informe 4 distingue lo pagado de lo pendiente | `PagosFundacionTests` |
| PR-DON | Donaciones y gastos | Montos válidos, campos obligatorios | `CajaCommandHandlerTests`, `ValidarTests` |
| PR-INF | Informes | Permisos de los siete; valores de los informes 1, 2, 4 y conciliación | `PermisosPorRolTests`, `PagosFundacionTests`, `atencion-y-pago.spec.js` |
| PR-REC | Contraseñas | Cambio con la contraseña actual; contraseña temporal; reglas de fuerza. La recuperación por correo está excluida | `OperacionesAc08Tests`, `operaciones.spec.js` |
| PR-COR | Correo | Que la pantalla diga la verdad si el correo no sale; avisos de asignación, reprogramación y cancelación | `AsignarMedicoCommandHandlerTests`, `OperacionesAc08Tests` y la prueba real en Azure (PR-COR-02) |
| PR-VAL | Validaciones | Campos obligatorios marcados; error junto al campo; validaciones repetidas en el servidor | `validacionCampos.test.js`, `ValidarTests`, `probar-validaciones.ps1` |
| PR-UI | Navegación y adaptabilidad | Menú por rol, cada opción abre, botón atrás, indicadores; escritorio, tableta y celular | `navegacion.spec.js`, `celular.spec.js`, capturas de AC-13 |
| PR-REG | Regresión | Todo lo anterior sobre la versión final | Este plan completo |

## 5. Criterios de aceptación

- **Antes de entregar:** 0 fallas en xUnit, Vitest, lint y Playwright, y 0 fallas en la matriz de permisos.
- **Si una prueba falla:** se corrige y se repite la regresión completa.
- **Si una prueba no se puede ejecutar** (por ejemplo, el correo en Azure): se marca como **Pendiente**, con la razón. Nunca como aprobada.
- **En Azure**, después de publicar: los permisos (`probar-permisos.ps1 -Api https://…`) y la versión que muestra el pie.
