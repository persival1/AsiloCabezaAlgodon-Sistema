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
