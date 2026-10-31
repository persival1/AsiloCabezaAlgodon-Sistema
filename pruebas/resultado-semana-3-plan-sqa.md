# Semana 3 del plan de pruebas de SQA

**Fecha:** 30 de octubre de 2026
**Plan de referencia:** «Plan de pruebas para la implementación y validación del sistema», SQA, 25/10/2026.
**Alcance:** lo que el cronograma asigna al desarrollador en la semana 3: validar el especialista asignado y los estados de la solicitud (casos AUT-08, EXP-05 a EXP-08) y el correo (COR).
**Entorno:** todo se hizo y se probó en local. **Azure no se tocó.**

## Resumen

| Caso | Problema antes | Ahora | Evidencia |
|---|---|---|---|
| AUT-08 | Cualquier especialista podía registrar la visita de una solicitud asignada a **otro** | **403** «Solo el especialista asignado puede registrar la visita de esta solicitud.» También si manda en el cuerpo el ID del especialista asignado: el ID sale del token | xUnit + script |
| EXP-07 | Se podía registrar una visita sobre una solicitud **Pendiente** | **409** «La solicitud todavía no tiene especialista asignado.» | xUnit + script |
| EXP-07 | Una **segunda visita** sobre la misma solicitud cobraba la consulta **otra vez** al familiar | **409** «Esta solicitud ya fue atendida.» y el saldo no cambia | xUnit + script |
| EXP-07 | Se podía **reasignar** una solicitud ya Atendida | **409** «No se puede asignar una solicitud en estado «Atendida».» | xUnit + script |
| EXP-06 | La asignación aceptaba una **fecha pasada** y cualquier usuario como especialista o enfermero (por ejemplo, Farmacia o Laboratorio) | **400** con el motivo; la solicitud sigue Pendiente | xUnit + script |
| COR-04 | La API respondía «correo enviado al familiar» **siempre**, aunque no hubiera SMTP configurado | Dice la verdad: «Solicitud asignada, pero el correo al familiar no se pudo enviar.» La pantalla lo muestra en un aviso ámbar | xUnit + script + navegador |
| COR-02 | Igual, con el servidor de correo caído | La asignación se conserva y se informa que el correo no salió | xUnit + script con SMTP falso |
| COR-05 | — | El registro del error **no** contiene la contraseña SMTP (se comprobó con una contraseña marcada) | Registro de la API |
| COR-01 | Envío real | **Pendiente**: necesita la contraseña de aplicación de Gmail | — |

## Cómo se corrigió

**Un tercer tipo de error** (`Application/Comun/Excepciones.cs`): `AccesoDenegadoException` → la API responde **403**. Se usa cuando el usuario tiene el rol correcto, pero el registro no le pertenece. El middleware de `Program.cs` lo convierte en `{ "mensaje": "..." }`, igual que los otros dos.

**Visita médica** (`RegistrarVisitaCommandHandler`). Antes de crear la visita o cobrar algo, revisa en este orden:
1. Que la solicitud exista (si no, 400).
2. Que no esté Atendida y que esté Asignada (si no, 409).
3. Que el especialista que llama sea el asignado (si no, 403).

El ID del especialista **no lo manda el navegador**: `VisitasMedicasController` lo toma del token (`command with { IdEspecialista = ... }`). Así nadie puede hacerse pasar por otro especialista.

**Asignación** (`AsignarMedicoCommandHandler`):
- Solo se asignan solicitudes Pendientes o Asignadas (una Asignada se puede reasignar, por ejemplo para cambiar la fecha). Atendida o Cancelada → 409.
- La fecha de la cita no puede estar en el pasado → 400.
- El especialista debe existir, estar activo y tener el rol Especialista; el enfermero (opcional) igual con el rol Enfermero → 400. Para eso, `IUsuarioRepository` tiene un método nuevo: `TieneRolAsync`.

**Correo** (`EmailService`): ahora devuelve `true` solo si el servidor de correo aceptó el mensaje. Si falta la configuración, falla la conexión o la dirección no es válida, devuelve `false` y lo deja en el registro. El handler devuelve `ResultadoAsignacion(CorreoEnviado)` y el controlador elige el mensaje según lo que pasó. La asignación se guarda **antes** de intentar el correo, así que un fallo del correo nunca deshace la asignación.

**Frontend** (`Solicitudes.jsx`): muestra el mensaje del servidor después de asignar (verde si el correo salió, ámbar si no) y, si la asignación se rechaza, el motivo (antes decía solo «No se pudo asignar la solicitud.»).

**Usuarios de prueba** (`SeedController`, solo en desarrollo): se agregó `especialista2` (Dra. Segunda Especialista, Cardiología), necesario para la prueba AUT-08. En Azure el Seed responde 404, así que no afecta a producción.

## Pruebas unitarias del backend

| Archivo | Qué prueba |
|---|---|
| `RegistrarVisitaCommandHandlerTests.cs` | AUT-08, EXP-07 (Pendiente, segunda visita), solicitud inexistente, EXP-08 (visita correcta cobra una vez y deja la solicitud Atendida), y los dos casos de datos de la semana 2 |
| `AsignarMedicoCommandHandlerTests.cs` (nuevo) | EXP-05, EXP-06 (fecha pasada, especialista y enfermero con otro rol), EXP-07 (reasignar Atendida), enfermero opcional, COR-02/COR-04 (correo falla), familiar sin correo, solicitud inexistente |

**Antes y después** (pruebas escritas primero):

| Momento | Resultado de `dotnet test` |
|---|---|
| Con el código anterior | **10 con error**, 47 superadas, 57 en total. Las 10 son exactamente los casos nuevos de la semana 3 |
| Después de corregir | **57 superadas**, 0 con error |

## Pruebas contra la API (integración)

Script nuevo `pruebas/probar-semana-3.ps1`. Recorre el ciclo completo de **una solicitud nueva** y en cada paso intenta también lo prohibido. **Crea datos** (una solicitud y una visita de Q150.00 por ejecución), así que solo se ejecuta en local o en un entorno de pruebas.

| Ejecución | Resultado | Detalle |
|---|---|---|
| Sin SMTP configurado (COR-04) | **11 de 11**, la conciliación cuadra. El saldo subió exactamente Q150.00 una vez (−349.50 → −199.50) y el segundo intento no cobró | `pruebas/resultados-semana-3.md` |
| Con un SMTP configurado que falla (COR-02) | **11 de 11**, la conciliación cuadra. Mensaje: «el correo al familiar no se pudo enviar» | `pruebas/resultados-semana-3-smtp-con-error.md` |

Para COR-05 se usó como contraseña SMTP un texto marcado; después se buscó ese texto en el registro de la API: **no aparece**. El registro solo dice «Error al enviar correo a …» con el detalle técnico de la conexión.

## Regresión

| Prueba | Resultado |
|---|---|
| Permisos por rol (`probar-permisos.ps1`, local) | 142 de 142 |
| Validaciones de la semana 2 (`probar-validaciones.ps1`, local) | 20 de 20, datos sin cambios |
| Backend: `dotnet build` | 0 advertencias, 0 errores |
| Frontend: `npm test` / `npm run lint` / `npm run build` | 8 de 8 / 0 problemas / compila |
| Navegador (Administrador, Solicitudes) | Fecha pasada: muestra «La fecha de la cita no puede estar en el pasado.» y no asigna. Fecha válida: asigna y muestra en ámbar «Solicitud asignada, pero el correo al familiar no se pudo enviar.» |

## Pendiente

- **COR-01, correo real:** hace falta la contraseña de aplicación de la cuenta de Gmail del sistema. El desarrollador la escribe en Azure (Configuración → Variables de entorno, `Smtp__*`); nunca va en el repositorio. Después se asigna una solicitud y se guarda la captura del correo recibido.
- **COR-03, destinatario inválido:** el registro de internos ya no acepta correos con formato inválido (semana 2), así que no se puede preparar el caso desde la interfaz. Si llegara uno, `EmailService` lo atrapa y responde «no se pudo enviar».
- **Antes de publicar esta semana en Azure: poner la hora de Guatemala** (`TZ=America/Guatemala`, punto de la semana 4). El servidor de Azure usa la hora UTC, seis horas adelante; el navegador manda la fecha de la cita en hora de Guatemala. Sin ese ajuste, una cita para dentro de menos de seis horas se rechazaría como «en el pasado».
- **Semana 4 (ROB-03):** la visita, su cargo y el cambio de estado de la solicitud se guardan en pasos separados; conviene una sola transacción.
- Aprobar con SQA: que una solicitud **Asignada** sí se pueda reasignar (para cambiar la fecha o el especialista) y que solo se rechacen las Atendidas y Canceladas.
