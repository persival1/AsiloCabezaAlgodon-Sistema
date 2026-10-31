# Pruebas del sistema publicado en Azure

- **Fecha:** 21/10/2026
- **Sitio:** https://asilo-cabeza-algodon-2796.azurewebsites.net
- **Base de datos:** Azure SQL, copia de la base local (13 tablas; la cantidad de filas de cada tabla coincidió con la local después de copiar)

## 1. Comprobaciones básicas (11 de 11 correctas)

| Comprobación | Esperado | Obtenido | Resultado |
|---|---|---|---|
| Página principal (frontend) | 200 | 200 | OK |
| Ruta de React al recargar (`/dashboard/internos`) | 200 | 200 | OK |
| API sin sesión (`GET /api/Internos`) | 401 | 401 | OK |
| Ruta de API inexistente | 404 | 404 | OK |
| `POST /api/Seed/admin` en producción | 404 | 404 | OK |
| `POST /api/Seed/usuarios-prueba` en producción | 404 | 404 | OK |
| Swagger apagado en producción | 404 | 404 | OK |
| HTTP sin cifrar redirige a HTTPS | 301 | 301 | OK |
| La contraseña de prueba `admin123` **no** sirve en Azure | 401 | 401 | OK |
| Inicio de sesión del administrador con su contraseña nueva | token | token | OK |
| El saldo del familiar cuadra (conciliación) | cuadra | −Q349.50, cuadra | OK |

## 2. Permisos por rol contra la API publicada

Se ejecutó `pruebas/probar-permisos.ps1` contra la API de Azure, con las contraseñas de Azure leídas desde un archivo privado (fuera del repositorio):

**142 de 142 pruebas correctas.** Detalle en [`resultados-permisos-azure.md`](./resultados-permisos-azure.md). Se comprobó que ese archivo no contiene ninguna contraseña.

## 3. Pendiente

- **Correo real:** falta configurar la cuenta de Gmail del sistema y probar que el correo llegue al asignar una solicitud.

---

# Actualización del 30/10/2026: semanas 1 y 2 del plan de SQA

Se publicó en el mismo App Service el código del commit `3a45486` (semanas 1 y 2 del plan de pruebas de SQA). No se creó ningún recurso nuevo y la base de datos no cambió de estructura.

## Antes de publicar

| Revisión | Resultado |
|---|---|
| Pruebas del backend (`dotnet test`) | 43 de 43 |
| Pruebas del frontend (`npm test`) | 8 de 8 |
| ESLint | 0 problemas |
| `npm audit --omit=dev` (lo que se publica) | 0 vulnerabilidades |
| Respaldo de la versión anterior | Se descargó `site/wwwroot` completo (80 archivos) antes de reemplazarlo |
| Contenido del paquete | Sin `appsettings.Development.json` y sin las pruebas |

`npm audit` completo reporta 6 avisos **moderados** publicados después del 26/10. Todos vienen de ESLint (herramienta de desarrollo, no se publica). La corrección que ofrece npm (`--force`) cambia ESLint de versión mayor, por eso no se aplicó.

## Después de publicar

| Comprobación | Resultado |
|---|---|
| Azure sirve la versión nueva | El archivo del frontend cambió de `index-Kv_hgVBV.js` a `index-CWdXGFVq.js` |
| Comprobaciones básicas | 11 de 11. La primera llamada a la base después del reinicio dio 500 mientras la base (AutoPause) despertaba; repetida tres veces dio 401, como se esperaba |
| Permisos por rol | 142 de 142 ([`resultados-permisos-azure.md`](./resultados-permisos-azure.md), sin contraseñas) |
| **Cobro doble (FAR-05)** | Despachar otra vez la prescripción 3, ya despachada: **409** «Esta prescripción ya fue despachada.» Los saldos quedaron iguales y la conciliación cuadra |

El script de validaciones (`probar-validaciones.ps1`) **no** se ejecutó en Azure a propósito: si una validación fallara, el dato inválido quedaría guardado en producción. Se ejecutó en local (20 de 20).

**Cómo regresar a la versión anterior si hiciera falta:** publicar el respaldo con `az webapp deploy --type zip` (el archivo está fuera del repositorio, en el equipo del desarrollador).

---

# Actualización del 30/10/2026 (noche): semana 3 del plan de SQA

Se publicó el código del commit `b115dce` (semana 3). Antes se descargó un respaldo de la versión anterior (semanas 1 y 2).

**Configuración nueva:** `TZ=America/Guatemala` en las variables de entorno del App Service. El servidor Linux usa UTC (seis horas adelante) y la regla nueva «la cita no puede estar en el pasado» compara con la hora del servidor; sin este ajuste, una cita para dentro de menos de seis horas se habría rechazado. Desde ahora las fechas que guarda el sistema (despachos, visitas, cobros) quedan en hora de Guatemala; las guardadas entre el 21 y el 30/10 en Azure quedaron en UTC, seis horas adelante.

## Incidente al publicar

`az webapp deploy` informó un error: «el sitio no arrancó en 10 minutos». Se investigó en los registros de arranque del App Service:

- El primer arranque falló con `System.BadImageFormatException: Bad IL range` (código de salida 134). Ese error aparece cuando .NET lee un archivo `.dll` incompleto.
- **Causa:** el cambio de la variable `TZ` reinicia la aplicación, y ese reinicio ocurrió mientras el despliegue todavía estaba copiando los archivos nuevos.
- Azure volvió a intentar y el segundo arranque, con todos los archivos ya copiados, funcionó («Site started»).
- **Comprobación:** se descargaron los 79 archivos publicados y se compararon con el paquete: **79 de 79 idénticos** (mismo SHA-256).

**Lección para la próxima vez:** cambiar la configuración, esperar a que la aplicación termine de reiniciar y recién entonces publicar.

## Verificación

| Comprobación | Resultado |
|---|---|
| Azure sirve la versión nueva | El archivo del frontend cambió a `index-D6B6BntR.js` |
| Archivos publicados iguales al paquete | 79 de 79 |
| Comprobaciones básicas | 11 de 11 (la primera llamada a la base después del reinicio dio 500 mientras la base despertaba; repetida tres veces dio 401) |
| Permisos por rol | 142 de 142 |
| Rechazos de la semana 3 en Azure (segunda visita, reasignar una Atendida) | **Pendiente**: requieren enviar peticiones de escritura a producción (que deben ser rechazadas) y quedan para hacerlas con autorización. En local pasaron 11 de 11 |

**Observación:** el 500 de la primera consulta después de cada reinicio se repite porque la base gratuita se pausa sola (AutoPause) y tarda en despertar. El propio registro sugiere activar `EnableRetryOnFailure` en `UseSqlServer`: así Entity Framework reintenta en lugar de responder 500. Queda propuesto para la semana 4 (robustez).

---

# Actualización del 30/10/2026 (23:25): revisión de errores

Se publicó el código del commit `8a7bbb2` (revisión descrita en [`resultado-revision-30-10.md`](./resultado-revision-30-10.md)): `EnableRetryOnFailure` para la base que despierta, validaciones en crear solicitud, agregar psicopatología y agregar medicina de cajón, y mensajes de error reales en la ficha del interno, Solicitudes y el login. Antes se descargó un respaldo de la versión anterior (semana 3). Esta vez no se cambió ninguna configuración, así que no hubo reinicio cruzado con la copia: el despliegue terminó bien al primer intento («Site started successfully»).

| Comprobación | Resultado |
|---|---|
| Azure sirve la versión nueva | El archivo del frontend cambió a `index-Cm4vTRf1.js` |
| Registros de arranque | Sin errores |
| Comprobaciones básicas | 11 de 11 al primer intento, sin 500 |
| Permisos por rol | 142 de 142 (archivo de resultados sin contraseñas) |
| Validaciones nuevas en Azure | **No se ejecutaron**: son peticiones de escritura a producción. En local pasaron (antes 500, ahora 400) |

**Reintento de la base:** esta vez la primera llamada no dio 500, pero no se puede asegurar que la base estuviera pausada en ese momento. La prueba real será la próxima vez que la base despierte después de estar una hora sin uso.
