# Manual de usuario por rol (AC-14, documento 7)

Versión 1.0.0-rc2. Las capturas son de la versión final con datos ficticios (`pruebas/evidencia-ac/ac-13/` y `ac-08/`). La instalación está aparte, en `README.md` y `despliegue/LEEME.md`.

## Para todos los roles

- **Entrar:** escribí tu usuario y tu contraseña. Después de 10 intentos en un minuto, el sistema pide esperar.
- **Inicio:** cada número dice de qué período es («Al día de hoy» o el mes) y se puede tocar para abrir el módulo. Debajo aparece lo pendiente, empezando por lo más antiguo. Un borde ámbar indica que hay algo por atender.
- **Menú:** está agrupado por área (Atención médica, Caja, Reportes, Administración). En el celular aparece arriba.
- **Listas:** se pueden buscar sin importar las tildes, filtrar por estado y fechas, y ver de 10 en 10.
- **Formularios:** los campos con * son obligatorios. Si algo falta o está mal, el mensaje aparece debajo del campo antes de guardar.
- **Cambiar contraseña:** usá el enlace del pie del menú. La contraseña nueva debe tener 8 o más caracteres, con letras y números. Si la olvidaste, pedile al Administrador una temporal.
- **Estados:** Pendiente (ámbar), Asignada (azul), Atendida, Pagada y Despachado (verde), Cancelada (rojo).
- **Versión:** el pie del menú muestra la versión y el commit publicados.
- **Salir:** «Cerrar sesión». Si la sesión vence (8 horas), el sistema vuelve solo al login.

## Administrador (asilo y fundación)

Captura del inicio: `ac-13/despues-escritorio-inicio-administrador.png`.

| Tarea | Dónde | Pasos |
|---|---|---|
| Asignar una cita | Solicitudes | «Asignar» → especialista, enfermero(a) (opcional) y fecha → «Asignar». La pantalla dice si el correo al familiar salió |
| Reprogramar o cancelar una cita | Solicitudes | «Reprogramar» (cambiá la fecha o la persona) o «Cancelar» (pide confirmación y avisa al familiar) |
| Registrar o corregir un interno | Internos → «Nuevo interno», o la ficha → «Editar datos» | El sistema no acepta dos internos con el mismo nombre, apellidos y fecha de nacimiento |
| Dar de baja o reactivar | Ficha → «Dar de baja» o «Reactivar» | Antes hay que cancelar sus solicitudes pendientes. La ficha y el historial se conservan |
| Cobrar | Cobros → «Nuevo cobro» | Para un cargo médico se elige la visita, y no se acepta más de lo que falta pagar. La cuota mensual no cambia el saldo de cargos médicos |
| Leer el saldo | Cobros, tabla «Saldo de cargos médicos por familiar» | Por pagar (rojo), Al día (gris) o A favor (azul) |
| Pagar a la fundación | Pagos a la fundación | Monto y número de comprobante. No se acepta más de lo adeudado |
| Donaciones y gastos | Caja | Monto mayor que cero, con dos decimales como máximo |
| Informes | Reportes | Los siete informes y la conciliación |
| Usuarios | Administración → Usuarios | «Nuevo usuario», «Editar», «Contraseña» (asigna una temporal) y «Desactivar». No podés desactivarte a vos mismo, y siempre queda un Administrador activo. Capturas: `ac-08/usuarios-*.png` |

## Médico general

| Tarea | Dónde | Pasos |
|---|---|---|
| Registrar o corregir un interno | Internos, ficha | Igual que el Administrador, pero sin ver la cuota ni el saldo |
| Pedir una cita al especialista | Solicitudes → «Nueva solicitud» | Interno, motivo y especialidad. Los internos de baja no aparecen |
| Corregir o cancelar mi solicitud | Solicitudes → «Corregir» o «Cancelar» | Solo mientras esté Pendiente. Si ya tiene cita, la cancela la fundación |
| Ficha y registros | Ficha | Agregar, corregir o quitar psicopatologías y medicinas de cajón |
| Informes clínicos | Reportes | Informes 2, 6 y 7 |

## Especialista

| Tarea | Dónde | Pasos |
|---|---|---|
| Ver mis citas | Inicio o «Mis citas» | Solo las asignadas a vos, en orden de fecha |
| Atender | Mis citas → registrar visita | Diagnóstico, observaciones, costo de la consulta, exámenes y medicamentos |
| Mis pacientes | «Mis pacientes» y su ficha | Solo los internos de tus citas |
| Visitas | Visitas médicas → «Ver detalle» | Exámenes y medicamentos de cada visita. No se muestra el estado de pago |
| Informes clínicos | Reportes | Informes 2, 6 y 7, solo de tus pacientes |

## Enfermero(a)

«Mis acompañamientos»: las citas en las que acompañás a un interno, con la fecha y el motivo.

## Laboratorio

«Exámenes solicitados»: escribí el resultado y el costo de cada examen Pendiente. El costo se carga a la cuenta del familiar. Un examen con resultado ya no se puede volver a registrar.

## Farmacia

«Prescripciones»: despachá cada medicamento Pendiente con su costo. El costo se carga a la cuenta del familiar.
