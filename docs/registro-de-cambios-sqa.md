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

## Plan de pruebas de SQA (25/10/2026): semana 1

SQA entregó un plan con 74 casos de prueba. Al ejecutar los primeros encontró tres fallas en el frontend, que se corrigieron el 26/10/2026:

- **Compilación en Linux (CMP-01, PU-FE-08):** dos importaciones no coincidían en mayúsculas con el archivo real (`RutaProtegida` y `servicios`). Corregido.
- **ESLint (CMP-02):** de 16 errores y 3 advertencias a 0.
- **Dependencias (CMP-03):** de 5 avisos altos a 0 con `npm audit fix`.
- Se incorporaron al repositorio las 8 pruebas automatizadas de SQA (Anexo A): `npm test` → 8 de 8 aprobadas.

Detalle y evidencia: `pruebas/resultado-semana-1-plan-sqa.md`. Pendiente: repetir la compilación en un equipo Linux.

## Plan de pruebas de SQA: semana 2 (26/10/2026)

- **Cargos duplicados corregidos:** despachar de nuevo una prescripción (FAR-05) o registrar de nuevo un resultado (LAB-06) ahora responde 409 y no vuelve a cargar el costo.
- **Validación en el servidor:** montos negativos, en cero, con más de dos decimales o demasiado grandes; campos obligatorios; correos y fechas imposibles. Responde 400 con un mensaje que la pantalla muestra (FIN-05, FAR-03, LAB-03, RES-02, RES-03).
- **Cobro ligado a la visita de otro interno** rechazado (FIN-06).
- **Pruebas unitarias del backend:** proyecto `AsiloCabezaAlgodon.Tests` con 43 pruebas. Con el código anterior fallaban 21; después de corregir pasan las 43.
- **Pruebas contra la API:** `pruebas/probar-validaciones.ps1`, 20 de 20 correctas y sin cambios en los datos. Permisos: 142 de 142.
- Las reglas de montos quedaron como **supuesto 8**, pendiente de aprobar.

Detalle y evidencia: `pruebas/resultado-semana-2-plan-sqa.md`.

## Plan de pruebas de SQA: semana 3 (30/10/2026, solo local)

- **Visita solo del especialista asignado (AUT-08):** otro especialista recibe 403, aunque mande en el cuerpo el ID del asignado (el ID sale del token).
- **Estados de la solicitud (EXP-07):** no se registra una visita sobre una solicitud Pendiente ni una segunda visita sobre una Atendida (la consulta ya no se cobra dos veces), y no se reasigna una Atendida: 409.
- **Validación de la asignación (EXP-06):** fecha pasada, especialista o enfermero sin ese rol: 400.
- **Mensaje honesto del correo (COR-02, COR-04):** la API ya no dice «correo enviado» si no se envió, y la pantalla de Solicitudes lo muestra. El registro no contiene la contraseña SMTP (COR-05).
- **Pruebas:** xUnit 57 (con el código anterior fallaban 10, justo los casos nuevos); `pruebas/probar-semana-3.ps1` 11 de 11, sin SMTP y con un SMTP que falla; regresión 142/142 permisos y 20/20 validaciones.
- **Pendiente:** correo real (COR-01) con la cuenta de Gmail.
- **Publicada en Azure el 30/10/2026** con `TZ=America/Guatemala`. El primer arranque falló porque el reinicio por el cambio de hora coincidió con la copia de archivos; el segundo funcionó y los 79 archivos publicados son idénticos al paquete. 11 comprobaciones básicas y 142 permisos correctos. Detalle en `pruebas/resultado-despliegue-azure.md`.

Detalle y evidencia: `pruebas/resultado-semana-3-plan-sqa.md`.

## Plan de pruebas de SQA: semana 5 (05/11/2026)

- **Recorridos en el navegador (fase 5, Playwright):** E1 atención médica completa, E2 pago parcial y total con conciliación, E3 sesión vencida y una prueba en tamaño de celular. 4 de 4.
- **Matriz de permisos automatizada (fase 3):** los 142 casos del script como prueba de xUnit con `WebApplicationFactory`. Se comprobó que detecta fallas: abriendo a propósito un controller fallaron 10 casos.
- **Celular:** el menú lateral pasa a barra superior, los campos se apilan y las tablas se deslizan (antes la página medía 530 px en un teléfono de 375 px).
- **Limpieza:** se borraron archivos de plantilla sin uso y se corrigieron nombres de carpetas y archivos (`Autenticacion`, `Entidad/Familiar.cs`, `Comun/Interfaces`, `RutaProtegida.jsx`).
- **Regresión completa:** xUnit 215 de 215, navegador 4 de 4, scripts 11/11, 142/142, 20/20 y 19/19.

- **Publicada en Azure el 06/11/2026** (commit `cf6c07e`, etiqueta `v1.0.0-rc1`) con `despliegue/actualizar-azure.ps1` y respaldo previo: 11 comprobaciones básicas, 142 permisos, 8 informes y 19 de 19 en datos mal formados y límite de intentos. Se confirmó en Azure que el reintento de la base (30/10) evita el 500 al despertar. Falta aplicar el usuario `app_asilo` (`despliegue/aplicar-usuario-minimo.ps1`).

Detalle: `pruebas/resultado-semana-5-plan-sqa.md` y `pruebas/resultado-despliegue-azure.md`. Entrega: `docs/version-candidata.md`.

## Plan de pruebas de SQA: semana 4 (03/11/2026, solo local)

- **Transacción (ROB-03):** registrar visita, despachar y registrar resultado guardan todo o nada (`IUnidadDeTrabajo` / `UnidadDeTrabajo`). Prueba de integración con SQL Server real: antes quedaba una visita guardada tras la falla; ahora no queda nada a medias.
- **Límite de intentos de login:** 10 por minuto por IP; el intento 11 recibe 429 con mensaje en español.
- **Datos inesperados (ROB-05):** JSON mal escrito o texto en campos numéricos responden 400 con `{ mensaje }` en español, sin nombres de clases internas (antes los mostraba).
- **Pipeline (Anexo C):** `.github/workflows/calidad.yml`, sin las pruebas que necesitan SQL Server. Primera ejecución en GitHub el 06/11/2026: en verde.
- **Usuario de base con permisos mínimos:** script preparado (`base-de-datos/usuario-permisos-minimos.sql`); se aplica en Azure con la publicación.
- **Pruebas:** xUnit 72 de 72; `probar-semana-4.ps1` 19 de 19; regresión 11/11, 142/142 y 20/20.

Detalle, y lo que no se hizo: `pruebas/resultado-semana-4-plan-sqa.md`.

## Revisión de errores en Azure y del proyecto (30/10/2026)

- **Único error de la aplicación en Azure:** la base gratuita se pausa y la primera petición mientras despierta daba 500. Arreglo: `EnableRetryOnFailure` (Entity Framework reintenta).
- **Tres operaciones daban 500 con un interno inexistente** y aceptaban textos vacíos: crear solicitud, agregar psicopatología y agregar medicina de cajón. Ahora 400 con mensaje. 10 pruebas xUnit nuevas (con el código anterior fallaban 7).
- **Pantallas:** la ficha del interno no mostraba los errores al guardar; crear solicitud mostraba un mensaje genérico; el login decía «contraseña incorrecta» ante cualquier error, incluso con la base despertando. Corregido.

- **Publicada en Azure el 30/10/2026 (23:25)**, con respaldo previo: 11 comprobaciones básicas y 142 permisos correctos, sin errores en el arranque. Las validaciones nuevas no se probaron en Azure (son escrituras a producción); en local, sí.

Detalle: `pruebas/resultado-revision-30-10.md`.

## Semanas 1 y 2 publicadas en Azure (30/10/2026)

Se publicó en Azure el código de las semanas 1 y 2 (commit `3a45486`), con respaldo previo de la versión anterior. En el sistema publicado: 11 comprobaciones básicas y 142 pruebas de permisos correctas, y **despachar de nuevo una prescripción ya despachada responde 409 sin cambiar los saldos**. Detalle: `pruebas/resultado-despliegue-azure.md` (sección «Actualización del 30/10/2026»).

**Nuevo aviso de dependencias:** `npm audit` reporta 6 avisos moderados (paquete `minimatch`, usado por ESLint) publicados después del 26/10. No afectan lo que se publica (`npm audit --omit=dev`: 0). La corrección automática rompe ESLint, así que se deja anotado para revisar cuando ESLint publique una versión corregida.

## Hallazgo adicional: cierre de sesión con token inválido

Al cambiar la llave del JWT, las sesiones abiertas en el navegador quedaron con tokens que el servidor ya no acepta (401). La pantalla no avisaba: se quedaba con errores en segundo plano. Lo mismo pasaría cuando un token vence a las 8 horas. **Corregido:** `api.js` detecta el 401, cierra la sesión y vuelve al login con el aviso «Tu sesión expiró o ya no es válida»; el login se excluye para que una contraseña incorrecta siga mostrando su mensaje. Se probó con 4 casos en el navegador (`pruebas/resultado-conciliacion-y-sesion.md`).

## Hallazgo adicional: paquete con vulnerabilidad

Al compilar, .NET advertía que el paquete `System.Security.Cryptography.Pkcs 9.0.11` (dependencia indirecta de Entity Framework) tenía una vulnerabilidad de gravedad alta conocida (GHSA-846f-f764-76mx: consumo excesivo de CPU al verificar firmas). **Corregido:** se fijó de forma directa la versión **10.0.13**, que es la que trae la corrección. Después del cambio, la compilación no muestra advertencias y `dotnet list package --vulnerable --include-transitive` no reporta paquetes vulnerables.
