# Matriz de trazabilidad de requerimientos

Respuesta a la solicitud de SQA de mantener una matriz de trazabilidad. Une cada requerimiento del enunciado con la pantalla que lo cubre, el endpoint de la API, la prueba que lo verifica y su estado.

**Cómo leerla:** de izquierda a derecha, «qué se pidió → dónde está en la interfaz → qué endpoint lo ejecuta → con qué prueba se comprobó».

**Pruebas:** `PF-xx` son casos funcionales, `R1`–`R7` son los informes (ver el documento de la Fase de Desarrollo). Las pruebas de permisos están en `pruebas/probar-permisos.ps1` (142 combinaciones de rol y endpoint, todas correctas).

**Estados:** **Cumplido** = construido y probado. **Con observación** = funciona, pero hay un punto abierto. **Parcial** = una parte del requisito no está construida o no se ha probado.

## A. Ficha del interno

| # | Requerimiento | Pantalla | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| 1 | Ficha por cada interno, con psicopatología y medicinas de cajón | Internos y Ficha médica | `POST /Internos`, `POST /Internos/{id}/psicopatologias`, `POST /Internos/{id}/medicamentos-cajon` | PF-02, PF-03 | Cumplido |
| 2 | Historial médico de cada interno | Ficha médica (historial de visitas) | `GET /Internos/{id}`, `GET /VisitasMedicas?idInterno=` | PF-04, R2 | Cumplido |
| 3 | Cada visita lleva fecha, motivo, médico y especialidad, exámenes, resultados, diagnóstico, medicamento y observaciones | Visitas médicas | `POST /VisitasMedicas`, `GET /VisitasMedicas` | PF-07, R2 | Con observación (supuesto 5: dosis e indicaciones en texto libre) |

## B. Flujo médico

| # | Requerimiento | Pantalla | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| 4 | El médico general decide la especialidad a la que remite | Solicitudes (Médico General) | `POST /Solicitudes` | PF-05 | Cumplido |
| 5 | Se crea la solicitud y se asigna el enfermero(a) | Solicitudes | `POST /Solicitudes`, `PUT /Solicitudes/{id}/asignar` | PF-05, PF-06 | Con observación (supuesto 2: lo asigna la fundación) |
| 6 | Las solicitudes aparecen en un módulo para la fundación, que asigna horario y médico | Solicitudes (Administrador) | `GET /Solicitudes`, `PUT /Solicitudes/{id}/asignar` | PF-06 | Con observación (supuesto 6: la fundación es el rol Administrador) |
| 7 | El médico ve la solicitud y el motivo, y esta se convierte en visita médica | Mis citas | `POST /VisitasMedicas` | PF-07 | Cumplido |
| 8 | El médico tiene acceso a la ficha médica del paciente | Ficha médica | `GET /Internos/{id}` | PF-04, permisos | Cumplido (permisos corregidos tras la revisión de SQA) |
| 9 | Los exámenes se asignan en la visita médica | Mis citas | `POST /VisitasMedicas` | PF-07 | Cumplido |
| 10 | El laboratorio coloca los resultados y el médico los consulta | Exámenes solicitados, Visitas médicas | `PUT /Examenes/{id}/resultado` | PF-08 | Cumplido |
| 11 | El médico indica el medicamento y cómo tomarlo | Mis citas | `POST /VisitasMedicas` | PF-07 | Con observación (supuesto 5) |
| 12 | Farmacia entrega el medicamento y lo asigna a la visita con su costo | Prescripciones | `PUT /Prescripciones/{id}/despachar` | PF-09 | Cumplido |

## C. Entradas, salidas y caja

| # | Requerimiento | Pantalla | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| 13 | Registrar el costo total que se le debe a la fundación | Reportes (informe 4) | `GET /Reportes/pagos-fundacion` | R4 | **Parcial** (supuesto 7: muestra lo adeudado; no registra los pagos hechos) |
| 14 | Control de las citas que aún no están completamente canceladas | Visitas médicas (estado de pago) y Cobros | `GET /VisitasMedicas`, `POST /Cobros` | PF-11 | Con observación (supuesto 4: cancelada = pagada) |
| 15 | Control de donaciones y de la cuota mensual de los familiares | Donaciones y Cobros | `POST /Donaciones`, `POST /Cobros` | PF-12 | Cumplido |
| 16 | Control de gastos (luz, agua, etc.) | Gastos | `POST /Gastos` | PF-12 | Cumplido |
| 17 | Los costos se cargan a la cuenta del familiar con el descuento de la fundación | Automático al registrar visita, resultado o despacho | (en los handlers de visita, examen y prescripción) | PF-10, conciliación de saldos | Con observación (supuesto 3: el costo se captura ya con el descuento). La diferencia de Q25.50 (O-01) ya se corrigió |

## D. Reportes

| # | Requerimiento | Pantalla | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| 18 | Costos de cada cita por paciente | Reportes → informe 1 | `GET /Reportes/costos-por-cita` | R1 | Cumplido |
| 19 | Análisis médico por paciente (ficha, motivo, medicamento) | Reportes → informe 2 | `GET /Reportes/ficha-medica/{idInterno}` | R2 | Cumplido |
| 20 | Cobros por paciente por rango de fecha, con gastos médicos como detalle | Reportes → informe 3 | `GET /Reportes/cobros-por-paciente` | R3 | Cumplido |
| 21 | Pagos a la fundación | Reportes → informe 4 | `GET /Reportes/pagos-fundacion` | R4 | **Parcial** (ver requerimiento 13) |
| 22 | Entradas: donaciones y cobros | Reportes → informe 5 | `GET /Reportes/entradas` | R5 | Cumplido |
| 23 | Exámenes realizados por paciente | Reportes → informe 6 | `GET /Reportes/examenes-por-paciente/{idInterno}` | R6 | Cumplido |
| 24 | Medicamentos aplicados por paciente | Reportes → informe 7 | `GET /Reportes/medicamentos-por-paciente/{idInterno}` | R7 | Cumplido |

## E. Funcionalidad adicional y técnicos

| # | Requerimiento | Pantalla | Endpoint | Prueba | Estado |
|---|---|---|---|---|---|
| 25 | Correo al familiar con el estado del paciente y el médico al que fue referido | Automático al asignar | `PUT /Solicitudes/{id}/asignar` | PF-06 | **Parcial** (supuesto 1; se comprobó que se registra el intento, **no** el envío real: observación O-02) |
| 26 | Base de datos SQL Server, MySQL u Oracle | — | — | Script `base-de-datos/crear-base.sql` y prueba de respaldo y recuperación | Cumplido (SQL Server) |

## Seguridad (agregado por la revisión de SQA)

| Qué | Dónde | Prueba | Estado |
|---|---|---|---|
| Permisos por rol aplicados en la API, no solo en los menús | Todos los controladores | `pruebas/probar-permisos.ps1`: 142 pruebas, antes 8 fallidas, ahora 0 | Cumplido |
| Llave JWT fuera del repositorio; la aplicación no arranca sin ella | `Program.cs`, `appsettings*.json` | Arranque en Producción con y sin la variable `Jwt__Key` | Cumplido |
| SeedController solo en desarrollo | `SeedController.cs` | En Producción responde 404 | Cumplido |
| Respaldo y recuperación de la base de datos | `base-de-datos/respaldo-y-restauracion.ps1` | `pruebas/resultado-respaldo-restauracion.md`: 14 de 14 tablas coinciden | Cumplido (ver sus limitaciones) |
| Conciliación de saldos de los familiares | `GET /api/Reportes/conciliacion-saldos`, `base-de-datos/conciliar-saldos.sql` | `pruebas/resultado-conciliacion-y-sesion.md`: diferencia de −Q25.50 a Q0.00 | Cumplido |
| Cierre de sesión cuando el token ya no sirve | `AsiloCabezaAlgodon.Web/src/Servicios/api.js` | 4 casos en el navegador | Cumplido |

## Resumen

| Estado | Cantidad |
|---|---:|
| Cumplido | 17 |
| Con observación | 6 (requerimientos 3, 5, 6, 11, 14 y 17) |
| Parcial | 3 (requerimientos 13, 21 y 25) |
| **Total** | **26** |

## Defectos y observaciones relacionadas

| ID | Qué | Estado |
|---|---|---|
| D-01 | Un examen sin costo se mostraba como «(Q)» | Corregido |
| O-01 | Diferencia de Q25.50 en el saldo de un familiar | Corregido (conciliación de saldos) |
| O-02 | El correo no se ha probado con envío real | Abierta |
