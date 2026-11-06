# Resultados de las pruebas de validación en el servidor

Fecha: 05/11/2026 23:22 · API: https://localhost:7022/api

**20 de 20 correctas.** Datos sin cambios después de las pruebas: **sí**.

| Caso del plan | Prueba | Petición | Esperado | Obtenido | Mensaje del servidor | Resultado |
|---|---|---|---|---|---|---|
| FAR-05 | Despachar de nuevo una prescripción ya despachada | PUT /Prescripciones/1007/despachar | 409 | 409 | Esta prescripción ya fue despachada. | Correcto |
| FAR-05 | Despachar una prescripción que no existe | PUT /Prescripciones/99999/despachar | 404 | 404 |  | Correcto |
| LAB-06 | Registrar de nuevo el resultado de un examen | PUT /Examenes/1006/resultado | 409 | 409 | Este examen ya tiene un resultado registrado. | Correcto |
| LAB-05 | Registrar el resultado de un examen que no existe | PUT /Examenes/99999/resultado | 404 | 404 |  | Correcto |
| FIN-05 | Cobro con monto -1.00 | POST /Cobros | 400 | 400 | El monto no puede ser menor que cero. | Correcto |
| FIN-05 | Cobro con monto 0.00 | POST /Cobros | 400 | 400 | El monto debe ser mayor que cero. | Correcto |
| FIN-05 | Cobro con monto 100,000,000.00 | POST /Cobros | 400 | 400 | El monto no puede ser mayor que Q99,999,999.99. | Correcto |
| FIN-05 | Cobro con monto 10.555 (tres decimales) | POST /Cobros | 400 | 400 | El monto solo puede tener dos decimales. | Correcto |
| FIN-05 | Cobro con monto «abc» | POST /Cobros | 400 | 400 | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. | Correcto |
| FIN-05 | Cobro sin concepto | POST /Cobros | 400 | 400 | El campo «concepto» es obligatorio. | Correcto |
| FIN-06 | Cobro ligado a una visita que no es de su interno | POST /Cobros | 400 | 400 | La visita médica no corresponde al interno de ese familiar. | Correcto |
| FIN-05 | Donación con monto -100.00 | POST /Donaciones | 400 | 400 | El monto no puede ser menor que cero. | Correcto |
| FIN-05 | Gasto con monto -350.00 | POST /Gastos | 400 | 400 | El monto no puede ser menor que cero. | Correcto |
| RES-02 | Interno sin nombres | POST /Internos | 400 | 400 | El campo «nombres» es obligatorio. | Correcto |
| RES-02 | Interno sin correo del familiar | POST /Internos | 400 | 400 | El campo «correo del familiar» es obligatorio. | Correcto |
| RES-03 | Correo del familiar «familiar.ejemplo» | POST /Internos | 400 | 400 | El campo «correo del familiar» no es un correo válido. | Correcto |
| RES-03 | Cuota mensual -1.00 | POST /Internos | 400 | 400 | La cuota mensual no puede ser menor que cero. | Correcto |
| RES-03 | Fecha de nacimiento 01/01/2030 | POST /Internos | 400 | 400 | La fecha de nacimiento no puede ser una fecha futura. | Correcto |
| RES-03 | Fecha de ingreso anterior al nacimiento | POST /Internos | 400 | 400 | La fecha de ingreso no puede ser anterior a la fecha de nacimiento. | Correcto |
| ROB-05 | JSON mal formado | POST /Cobros | 400 | 400 | Los datos enviados no tienen el formato correcto. Revisá los campos e intentá de nuevo. | Correcto |

## Datos antes y después

| Dato | Antes | Después |
|---|---|---|
| Internos | 2 | 2 |
| Cobros | 4 | 4 |
| Donaciones | 1 | 1 |
| Gastos | 1 | 1 |
| Saldos | 2=150, 1=293 | 2=150, 1=293 |
| Cuadra | True | True |
