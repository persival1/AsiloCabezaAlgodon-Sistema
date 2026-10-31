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
