# Supuestos del análisis para aprobación

Respuesta a la observación 5 de SQA (requerimientos ambiguos). El enunciado del proyecto no definía con claridad siete puntos. En cada uno se tomó la opción más simple y se implementó así. Este documento los lista para que **SQA o el cliente los aprueben, los rechacen o pidan un cambio**, y para dejar constancia de la decisión.

**Cómo usarlo:** en la última columna marcar una opción y, si se pide un cambio, escribir cuál. Mientras no haya respuesta, el sistema funciona con el supuesto descrito.

| # | Qué dice el enunciado | Qué se implementó (el supuesto) | Qué pasaría si se cambia | Decisión |
|---|---|---|---|---|
| 1 | El correo al familiar se envía «cuando se crea una solicitud» e informa «a qué médico fue referido» | El correo se envía cuando la fundación **asigna el médico**, porque al crear la solicitud todavía no existe médico asignado | Para enviarlo al crear habría que mandar un primer correo sin médico y otro al asignar (dos correos) | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 2 | Hay que «asignar el enfermero(a) que acompañará» al paciente, pero la fundación «asigna el horario y el médico» | **La fundación** (rol Administrador) asigna al enfermero junto con el médico y el horario | Si lo asigna el médico general, la solicitud tendría un campo más al crearse y la fundación solo podría confirmarlo | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 3 | Los costos se cargan «siempre con el descuento que proporciona la fundación» | Quien captura el costo (especialista, laboratorio o farmacia) lo escribe **ya con el descuento aplicado**. El sistema no calcula porcentajes | Para calcularlo se necesita el porcentaje o la tarifa de cada servicio, y un lugar donde configurarlos | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 4 | Llevar el control de las citas «aún no completamente canceladas» | «Cancelada» se interpretó como **pagada**. Cada visita muestra su estado de pago: pagada o pendiente (por ejemplo «Q150.00 de Q265.00») | Si «cancelada» significara anulada, haría falta un estado de cita anulada y otro reporte | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 5 | Registrar «medicamento aplicado, cantidad y tiempo de aplicación» | Se registran **dosis e indicaciones como texto libre** (por ejemplo «20mg» y «Una vez al día por 30 días») | Para campos numéricos (cantidad, días) habría que definir unidades y validaciones | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 6 | Habla de «un módulo que se le proporcionará a la fundación» | La fundación **usa el rol Administrador**; no es un usuario separado | Si la fundación debe ser un rol aparte, el Administrador del asilo y la fundación tendrían permisos distintos | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |
| 7 | Registrar «el costo total que se le debe a la fundación» y un reporte de «pagos a la fundación» | El reporte 4 muestra **lo que se le debe** (consultas + laboratorio + farmacia). El sistema **no registra los pagos que el asilo hace** a la fundación | Para registrar pagos efectuados haría falta una tabla nueva, una pantalla y un saldo adeudado que baje con cada pago | ☐ Aprobado ☐ Rechazado ☐ Cambiar a: ______ |

## Prioridad sugerida para la revisión

Los supuestos **1, 3 y 7** son los que más afectan la lógica del negocio. El 7 es el único en el que parte de lo que pide el enunciado **no está construido**: se interpretó el reporte como «monto adeudado», pero no hay registro de pagos hechos.

## Constancia

| | |
|---|---|
| Revisó | ______________________________ |
| Cargo | ☐ SQA ☐ Cliente / catedrático |
| Fecha | ______________ |
