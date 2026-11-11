# Errores y limitaciones conocidas (AC-14, documento 9)

Catálogo vigente para la versión **1.0.0-rc2**. Reemplaza el apartado 3 de `docs/version-candidata.md`.

Severidad: **Alta** = afecta datos o seguridad. **Media** = una función no está completa. **Baja** = molestia o tema de presentación.

| ID | Descripción | Severidad | Impacto | Alternativa o tratamiento | Acción |
|---|---|---|---|---|---|
| LIM-01 | En Azure no está configurada la cuenta de correo (`Smtp__*`) | Media | El familiar no recibe el aviso de cita asignada, reprogramada o cancelada. La pantalla lo dice («el correo no se pudo enviar») | El Administrador avisa por teléfono. Configurar Gmail en Azure | AC-11 (usuario) |
| LIM-05 | No hay recuperación de contraseña por correo | Media | Quien olvida su contraseña depende del Administrador | Contraseña temporal desde «Usuarios» (AC-05) | AC-05 (exclusión propuesta) |
| LIM-06 | No hay portal para familiares | Baja | El familiar no consulta en línea | Recibe correos y la información en el asilo | AC-06 (exclusión propuesta) |
| LIM-07 | Al desactivar un usuario, la sesión que ya tenía abierta sigue válida hasta que vence el token | Media | Un usuario desactivado puede seguir usando una sesión ya abierta hasta 8 horas | El token vence a las 8 horas. Si urge, se puede cambiar la llave JWT y cerrar todas las sesiones | AC-08 |
| LIM-08 | Límite de intentos: falta confirmar que Azure ve la IP real de cada usuario y no la del balanceador | Media | Si viera la del balanceador, 10 intentos fallidos de una persona bloquearían a todas por un minuto | En local funciona por IP. Verificar con dos conexiones en Azure | — |
| LIM-09 | El escenario E4 (cierre de mes) no está automatizado en el navegador | Baja | Se prueba a mano | Lo cubren los informes 3, 4 y 5 y la conciliación | AC-18 |
| LIM-10 | Los valores de los informes 3 a 7 se comprueban con datos de prueba, no con los datos de referencia de SQA | Baja | — | SQA los ejecuta con sus datos | AC-18 |
| LIM-11 | El plan de Azure está temporalmente en B1 para la revisión | Baja | Costo mientras dure la revisión | Vuelve a F1 (gratis) al terminar | — |

## Corregidos en esta versión (ya no son limitaciones)

| Antes | Corrección | Acción |
|---|---|---|
| «Unhandled rejection 401» en la consola al vencer la sesión | La petición queda en espera mientras se vuelve al login | AC-17 |
| Saldo del familiar sin regla clara de signo | Saldo = cargos médicos − pagos de visitas, con «Por pagar», «Al día» y «A favor» | AC-01 |
| Pagos a la fundación no registrables | Registro, historial e informe 4 | AC-07 |
| El especialista veía todos los internos y su historial | Solo ve a sus pacientes; los datos financieros solo los ve el Administrador | AC-04 y AC-10 |
| La fecha por defecto de la ficha y los reportes era la de UTC (después de las 18:00 marcaba el día siguiente) | Fecha local | AC-16 (frontend) |
| En Azure la aplicación usaba el administrador del servidor SQL | Desde el 10/11 usa `app_asilo` (solo lee y escribe datos) | AC-03 |
| No estaba comprobado que la llave JWT de Azure fuera distinta de la publicada | Es distinta (64 caracteres, no está en el historial de Git); 7 tokens falsificados reciben 401 en Azure | AC-02 |
| Posibles fechas UTC en Azure del 21 al 30/10 | Revisado: no hay registros en esas fechas, así que no hubo nada que corregir | AC-16 |
