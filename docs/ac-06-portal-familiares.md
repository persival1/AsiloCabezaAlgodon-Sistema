# AC-06: portal de familiares (exclusión propuesta)

## Decisión propuesta

No implementar un rol «Familiar» con inicio de sesión en esta versión.

## Justificación

1. **El enunciado no lo pide.** Sobre el familiar solo dice que se le debe «enviar un correo electrónico» cuando se asigna la cita. El portal venía del análisis «Nueva Vida», que no describe este sistema (ver `docs/matriz-actores-y-roles.md`, sección 1).
2. **Lo que el familiar necesita ya le llega:**
   - El correo de la cita asignada (CU-11).
   - Desde AC-08, también el aviso de cita reprogramada o cancelada.
3. **Riesgo.** Un portal expondría información médica y financiera a usuarios externos. Pediría algo que hoy no existe:
   - Registro y verificación de identidad.
   - Recuperación de contraseña por correo, que depende de AC-05 y AC-11.
   - Aislamiento estricto por residente.

   Hacerlo sin requisito aprobado contradice el principio de mínimo privilegio que pide la misma revisión (sección 6).

## Si se aprueba hacerlo después

Haría falta lo siguiente:

- Un rol `Familiar` unido a `Familiar.IdFamiliar`.
- Solo lectura de su residente: las próximas citas, el saldo de cargos médicos y sus cobros.
- Pruebas de acceso horizontal, igual que las del especialista en AC-04: un familiar recibe 403 al pedir otro residente.

**Estado:** exclusión propuesta, pendiente de firma del docente o de SQA (AC-12).
