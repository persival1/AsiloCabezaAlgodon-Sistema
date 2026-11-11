# AC-11: correo real desde Azure (10/11/2026)

**Configuración.** El desarrollador puso en el App Service las variables `Smtp__Host` (`smtp.gmail.com`), `Smtp__Puerto` (`587`), `Smtp__Usuario`, `Smtp__Remitente` y `Smtp__Password`:

- La cuenta es la del desarrollador en la universidad, que usa Google.
- La clave es una contraseña de aplicación de 16 letras, con la verificación en dos pasos activa.
- Los valores no están en el repositorio y no se muestran en este documento.

Antes se revisó que todo estuviera bien: las cinco variables existen, el usuario y el remitente son la misma cuenta, y la contraseña tiene el formato de una contraseña de aplicación.

**Prueba (autorizada por el desarrollador), contra la versión 1.0.0-rc2 publicada (commit `675011f`).** Se hizo por la API, con las cuentas de Azure guardadas fuera del repositorio:

| Paso | Acción | Resultado |
|---|---|---|
| 1 | Crear el interno de prueba «PRUEBA CORREO AC-11», con el correo del desarrollador como correo del familiar | Interno id 2 |
| 2 | El médico general crea la solicitud «Prueba del correo al familiar (AC-11)» | Solicitud id 4, Pendiente |
| 3 | El Administrador asigna especialista, enfermero(a) y una cita a dos días | «Solicitud asignada y correo enviado al familiar.» (`correoEnviado = true`) |
| 4 | El Administrador cancela la cita | «Solicitud cancelada.» (`correoEnviado = true`) |
| 5 | Se cambia el correo del familiar a `prueba.correo@prueba.test` y se da de baja al interno | Interno de baja; su correo ya no es el real |

**Recepción.** El desarrollador confirmó que a las 22:43 llegaron a su bandeja de entrada los dos correos: «Cita médica asignada para PRUEBA CORREO AC-11» y «Cita médica cancelada para PRUEBA CORREO AC-11».

**Lo que queda en Azure.** El interno id 2, de baja y marcado como prueba, y su solicitud id 4, cancelada. No aparecen en las listas de trabajo: un interno de baja no recibe solicitudes y las canceladas no están pendientes.

**Reprogramar** usa el mismo envío que asignar (el mismo comando `AsignarMedicoCommand` sobre una solicitud Asignada). Por eso no se mandó un tercer correo: el texto de ese caso ya está cubierto por la prueba automática `Reprogramar_una_cita_asignada_cambia_la_fecha_y_avisa_de_nuevo`.
