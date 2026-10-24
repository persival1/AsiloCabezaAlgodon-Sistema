# Registro de cambios por la revisión preliminar de SQA

Este documento responde, punto por punto, a las ocho observaciones de la revisión preliminar de SQA. Para cada una indica qué se encontró, qué se cambió, dónde está el cambio y cuál es la evidencia. Lo que todavía no está resuelto se dice claramente.

## Resumen

| # | Observación de SQA | Estado | Evidencia |
|---|---|---|---|
| 1 | Configuración de seguridad (llave JWT) | **Corregido** | Prueba de arranque en Producción (sección 1) |
| 2 | SeedController | **Corregido** | Prueba en Producción: responde 404 (sección 2) |
| 3 | Permisos por rol en la API | **Corregido** (se encontró un hueco real) | `pruebas/resultados-permisos.md`: 142 de 142 correctas. Antes: 8 fallidas |
| 4 | Integridad financiera (diferencia de Q25.50) | **Corregido** | Reporte de conciliación: antes −Q25.50, ahora Q0.00 (`pruebas/resultado-conciliacion-y-sesion.md`) |
| 5 | Requerimientos ambiguos | **Documentado; falta la aprobación** | `docs/supuestos-para-aprobacion.md` (tabla para aprobar o rechazar cada supuesto) |
| 6 | Correo electrónico | **Pendiente** | Hoy solo se comprueba que se registra el intento |
| 7 | Pruebas complementarias | **Parcial** | Respaldo y recuperación: hecho (`pruebas/resultado-respaldo-restauracion.md`). Integración, regresión y aceptación: según el plan de SQA |
| 8 | Despliegue reproducible | **Corregido** | Publicado en Azure con planes gratis y script reproducible (`despliegue/`). 11 comprobaciones y 142 pruebas de permisos correctas contra el sistema publicado |

Además se creó la **matriz de trazabilidad** que SQA pidió mantener: `docs/matriz-de-trazabilidad.md`.

---

## 1. Llave JWT y configuración (observación 1)

**Qué se encontró:** la llave con la que se firman los tokens estaba escrita en `appsettings.json`, que se sube al repositorio. Aunque era una llave de desarrollo, quien la conozca puede fabricar tokens válidos para cualquier rol.

**Qué se cambió:**
- `appsettings.json`: `Jwt:Key` quedó **vacía**.
- `appsettings.Development.json`: contiene una llave de prueba, que solo se usa en el entorno de desarrollo.
- `Program.cs`: al arrancar, si falta la llave o tiene menos de 32 caracteres, la aplicación **se niega a iniciar** con un mensaje claro, en lugar de arrancar de forma insegura.
- `appsettings.Production.example.json`: guía de cómo configurar un servidor real. Los secretos se entregan por variables de entorno (por ejemplo `Jwt__Key`).
- `.gitignore`: el archivo `appsettings.Production.json` real nunca se sube.

**Evidencia:** se ejecutó la aplicación en modo Producción (a) sin llave: no arrancó y mostró "Falta la llave del JWT (Jwt:Key)…"; (b) con la llave en la variable de entorno `Jwt__Key`: arrancó y el inicio de sesión funcionó.

## 2. SeedController (observación 2)

**Qué se encontró:** `SeedController` crea los roles y los usuarios de prueba y no exige sesión, porque se usa para crear el primer usuario. En un servidor real habría quedado abierto a cualquiera.

**Qué se cambió:** ahora cada acción revisa `IWebHostEnvironment.IsDevelopment()`. Fuera del entorno de desarrollo, el controlador responde **404**, como si no existiera.

**Evidencia:** en modo Producción, `POST /api/Seed/admin` y `POST /api/Seed/usuarios-prueba` respondieron 404, y Swagger también (404).

**Nota:** el controlador sigue existiendo en el código porque es necesario para crear el primer usuario en un equipo de desarrollo. Una mejora futura sería reemplazarlo por un comando de consola de un solo uso.

## 3. Permisos por rol en la API (observación 3)

**Qué se encontró (hueco real):** los menús de la interfaz ocultaban las opciones, pero el servidor no bloqueaba todo:
- `GET /api/Internos` y `GET /api/Internos/{id}` podían ser llamados por **cualquier usuario con sesión** (Enfermero, Laboratorio, Farmacia), y devuelven la ficha completa, que incluye datos del familiar.
- `GET /api/Solicitudes` devolvía **todas** las solicitudes a Laboratorio y Farmacia.

**Qué se cambió:**
- `InternosController`: los dos `GET` ahora exigen `[Authorize(Roles = "Administrador,MedicoGeneral,Especialista")]`.
- `SolicitudesController`: el `GET` exige `[Authorize(Roles = "Administrador,MedicoGeneral,Especialista,Enfermero")]`.
- La interfaz no se ve afectada: esos roles no usaban esos endpoints (se revisó `Inicio.jsx`).

**Evidencia:** el script `pruebas/probar-permisos.ps1` inicia sesión con los 6 roles y sin sesión, y prueba 142 combinaciones.
- **Antes** del cambio: 134 correctas y **8 fallidas** (`pruebas/resultados-permisos-ANTES-de-la-correccion.md`).
- **Después**: **142 correctas, 0 fallidas** (`pruebas/resultados-permisos.md`).

**Limitación:** las escrituras (POST y PUT) solo se probaron para los roles que no deben tener permiso (deben dar 403). No se probaron con los roles permitidos para no crear datos de prueba en cada ejecución.

## 4. Integridad financiera, diferencia de Q25.50 (observación 4)

**Qué se encontró:** el saldo del familiar muestra −Q385.00, y recalculado con los movimientos debería ser −Q359.50. La diferencia es el Ibuprofeno (Q25.50), que se despachó antes de implementar el cargo automático a la cuenta.

**Causa de fondo:** el saldo (`Familiar.SaldoPendiente`) es un número guardado que se modifica con cada movimiento (suma con cada costo, resta con cada cobro). Si algún movimiento ocurre sin actualizarlo, el saldo se descuadra, y no hay forma de detectarlo.

**Qué se cambió:**
1. **Verificación permanente:** el reporte `GET /api/Reportes/conciliacion-saldos` (solo Administrador, solo lectura) recalcula el saldo de cada familiar desde sus movimientos (costos médicos menos cobros) y lo compara con el saldo guardado. Si algún día se vuelve a descuadrar, se detecta.
2. **Corrección del dato:** el script `base-de-datos/conciliar-saldos.sql` corrige, dentro de una transacción, solo los saldos que no coinciden. Antes de ejecutarlo se hizo un respaldo verificado de la base.

**Evidencia:** antes de corregir, el reporte mostró saldo guardado −Q375.00 contra −Q349.50 recalculado (diferencia −Q25.50). Después: −Q349.50 en ambos, diferencia Q0.00. Las cifras cambiaron respecto al documento de desarrollo porque luego se despachó un medicamento más (Q10.00), cuyo cargo sí se sumó bien; la diferencia siguió siendo la del Ibuprofeno. Detalle en `pruebas/resultado-conciliacion-y-sesion.md`.

**Estado:** **corregido**.

## 5. Requerimientos ambiguos (observación 5)

Los siete supuestos están documentados en el README de la raíz y en el documento de la Fase de Desarrollo (correo al asignar, enfermero asignado por la fundación, descuento ya aplicado, citas canceladas = pagadas, dosis e indicaciones en texto, fundación = rol Administrador, pagos a la fundación = monto adeudado).

**Estado:** documentado, **falta la aprobación**. Se preparó `docs/supuestos-para-aprobacion.md`, una tabla donde SQA o el cliente marcan aprobado, rechazado o cambiar para cada supuesto. Los supuestos 1, 3 y 7 son los que afectan más a la lógica de negocio. El 7 es el único en el que parte del requisito no está construida (no se registran los pagos hechos a la fundación).

## 6. Correo electrónico (observación 6)

**Situación actual:** el sistema distingue dos casos, pero solo se probó uno. Sin configuración de SMTP registra una advertencia en el log y continúa. **No se ha comprobado el envío real** de un correo.

**Estado:** **pendiente**. Para probarlo hace falta un servidor SMTP (uno real o uno de pruebas en el equipo local) y guardar la evidencia del correo recibido.

## 7. Pruebas complementarias (observación 7)

Pruebas de integración, seguridad, regresión, respaldo y recuperación.

**Hecho:**
- **Respaldo y recuperación:** el script `base-de-datos/respaldo-y-restauracion.ps1` (acción `Probar`) respalda la base, la restaura con otro nombre, compara las filas de cada tabla y borra la copia. Resultado: las 14 tablas coinciden. Evidencia: `pruebas/resultado-respaldo-restauracion.md`. Limitaciones: se probó en el mismo servidor y compara cantidad de filas, no el contenido campo por campo.
- **Seguridad y regresión de permisos:** el script de la observación 3 (`pruebas/probar-permisos.ps1`) se puede volver a ejecutar después de cualquier cambio.

**Pendiente:** pruebas de integración y de aceptación funcional, según el plan por fases que defina SQA.

## 8. Despliegue (observación 8)

**Hecho:** instrucciones de instalación (README), script de base de datos (`base-de-datos/crear-base.sql`), separación entre configuración de desarrollo y producción, ejemplo de configuración de producción y sección "Despliegue" en el README.

**También hecho:**
- El origen permitido por CORS ya no está fijo: se lee de `Cors:Origenes` en la configuración (por defecto, el del entorno local).
- La dirección de la API del frontend se lee de la variable `VITE_API_URL` (ver `AsiloCabezaAlgodon.Web/.env.example`). El `.env` real queda fuera del repositorio.

- El `Dockerfile` del backend, generado por Visual Studio con rutas incorrectas, **se eliminó** junto con `.dockerignore`, la configuración de Docker del `.csproj`, el paquete de contenedores de Visual Studio y el perfil de arranque «Container». El sistema no usa contenedores; si se necesitan en el futuro, se crean de nuevo correctamente.

**Desplegado en Azure** (21/10/2026): https://asilo-cabeza-algodon-2796.azurewebsites.net
- App Service F1 (gratis) con la API y el frontend juntos, y Azure SQL con la oferta gratis y AutoPause.
- Seguridad: solo HTTPS y TLS 1.2, FTP y credenciales básicas de publicación apagados, la base solo acepta las IP de salida de la aplicación, llave JWT y contraseña de la base generadas al azar y guardadas solo en Azure, y contraseñas nuevas para los 6 usuarios (las de prueba no funcionan en Azure).
- Script reproducible: `despliegue/desplegar-azure.ps1`. Guía: `despliegue/LEEME.md`.
- Pruebas: 11 comprobaciones básicas y 142 pruebas de permisos, todas correctas (`pruebas/resultado-despliegue-azure.md`).

**Pendiente:** configurar el correo de Gmail en Azure y probar el envío real (observación 6).

## Hallazgo adicional: cierre de sesión con token inválido

Al cambiar la llave del JWT, las sesiones abiertas en el navegador quedaron con tokens que el servidor ya no acepta (401). La pantalla no avisaba: se quedaba con errores en segundo plano. Lo mismo pasaría cuando un token vence a las 8 horas. **Corregido:** `api.js` detecta el 401, cierra la sesión y vuelve al login con el aviso «Tu sesión expiró o ya no es válida»; el login se excluye para que una contraseña incorrecta siga mostrando su mensaje. Se probó con 4 casos en el navegador (`pruebas/resultado-conciliacion-y-sesion.md`).

## Hallazgo adicional: paquete con vulnerabilidad

Al compilar, .NET advertía que el paquete `System.Security.Cryptography.Pkcs 9.0.11` (dependencia indirecta de Entity Framework) tenía una vulnerabilidad de gravedad alta conocida (GHSA-846f-f764-76mx: consumo excesivo de CPU al verificar firmas). **Corregido:** se fijó de forma directa la versión **10.0.13**, que es la que trae la corrección. Después del cambio, la compilación no muestra advertencias y `dotnet list package --vulnerable --include-transitive` no reporta paquetes vulnerables.
