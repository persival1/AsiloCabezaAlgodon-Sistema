# Resultados de las pruebas de la semana 3 (plan de SQA)

Fecha: 03/11/2026 20:44 · API: https://localhost:7022/api · Solicitud de prueba: 9

**11 de 11 correctas.** Conciliación de saldos al final: **cuadra**.

| Caso del plan | Prueba | Esperado | Obtenido | Mensaje del servidor | Comprobación extra | Resultado |
|---|---|---|---|---|---|---|
| EXP-04 | El médico general crea la solicitud | 200 | 200 |  | Estado: Pendiente | Correcto |
| EXP-07 | Visita sobre la solicitud Pendiente | 409 | 409 | La solicitud todavía no tiene especialista asignado. | Visitas sin cambio | Correcto |
| EXP-06 | Asignar con fecha pasada (01/01/2020) | 400 | 400 | La fecha de la cita no puede estar en el pasado. | Sigue: Pendiente | Correcto |
| EXP-06 | Asignar un usuario de Farmacia como especialista | 400 | 400 | El usuario elegido como especialista no tiene el rol Especialista. | Sigue: Pendiente | Correcto |
| EXP-06 | Asignar un usuario de Laboratorio como enfermero | 400 | 400 | El usuario elegido como enfermero(a) no tiene el rol Enfermero. | Sigue: Pendiente | Correcto |
| EXP-05 / COR | Asignación correcta (mensaje del correo según lo que pasó) | 200 | 200 | Solicitud asignada, pero el correo al familiar no se pudo enviar. | correoEnviado = False | Correcto |
| AUT-08 | El Especialista 2 registra la visita de una solicitud del Especialista 1 | 403 | 403 | Solo el especialista asignado puede registrar la visita de esta solicitud. | Visitas y estado sin cambio | Correcto |
| AUT-08 | Igual, pero mandando en el cuerpo el ID del especialista asignado | 403 | 403 | Solo el especialista asignado puede registrar la visita de esta solicitud. | El ID sale del token, no del cuerpo | Correcto |
| EXP-08 | El especialista asignado registra la visita | 200 | 200 |  | Saldo: 100.5 -> 250.5 | Correcto |
| EXP-07 | Segunda visita sobre la solicitud Atendida | 409 | 409 | Esta solicitud ya fue atendida. | Saldo sin cambio: 250.5 | Correcto |
| EXP-07 | Reasignar la solicitud ya Atendida | 409 | 409 | No se puede asignar una solicitud en estado «Atendida». | Sigue: Atendida | Correcto |
