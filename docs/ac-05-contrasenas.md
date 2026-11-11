# AC-05: contraseñas olvidadas y cambio de contraseña

## Qué se implementó

| Caso | Cómo funciona | Endpoint | Pruebas |
|---|---|---|---|
| Un usuario olvidó su contraseña | Avisa al Administrador. El Administrador abre «Usuarios», elige «Contraseña» y le asigna una temporal, que le entrega en persona | `PUT /api/Usuarios/{id}/clave` (solo Administrador) | `El_administrador_restablece_una_clave_temporal_valida`; e2e |
| Cambiar la contraseña propia | Desde el enlace «Cambiar contraseña» del menú, cualquier rol. Pide la contraseña actual y la nueva dos veces | `PUT /api/Auth/cambiar-clave` (el usuario sale del token, no de los datos enviados) | `Cambiar_la_clave_pide_la_actual_correcta`; e2e |
| Reglas | 8 o más caracteres, con letras y números, y distinta de la actual. Se guarda solo el hash BCrypt | — | `Crear_usuario_con_datos_invalidos_se_rechaza` |
| Fuerza bruta | El cambio de contraseña comparte el límite del login: 10 intentos por minuto | — | Atributo `[EnableRateLimiting("login")]`; por eso la prueba e2e espera un minuto entre sus dos escenarios |

## Qué se propone excluir

La **recuperación por enlace en el correo** (el usuario escribe su correo y recibe un enlace de un solo uso) se propone excluir de esta versión, por dos motivos:

1. El enunciado no la pide. Solo pide el correo al familiar cuando se asigna una cita.
2. Abre un punto de entrada público nuevo (cualquiera puede pedir un enlace). El correo ya funciona en Azure desde el 10/11 (AC-11), así que se podría implementar si se aprueba. Pero hacerlo bien pide más que enviar un correo: un token de un solo uso que vence, la misma respuesta exista o no el usuario, y las pruebas de enlace vencido, enlace reutilizado y usuario inexistente que pide el criterio de aceptación.

Con el restablecimiento por el Administrador nadie se queda sin acceso, y no se abre un punto de entrada público nuevo.

Si se aprueba implementarla, el diseño sería este:

- Un token aleatorio de un solo uso, guardado como hash, que vence en 30 minutos.
- La misma respuesta exista o no el usuario.
- El límite de intentos del login.

**Estado:** exclusión propuesta, pendiente de firma del docente o de SQA (AC-12).
