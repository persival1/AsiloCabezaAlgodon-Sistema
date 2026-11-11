# Historial de versiones (AC-14, documento 13; AC-15)

Cada versión con su etiqueta, fecha, commit y despliegue en Azure. Desde la 1.0.0-rc2, la versión y el commit se ven en el pie del menú y del login, y también se pueden consultar en `GET /api/Version`. Así se comprueba que lo publicado es lo revisado.

| Versión | Etiqueta | Fecha | Commit del código | Publicada en Azure | Contenido |
|---|---|---|---|---|---|
| Semanas 1 y 2 | — | 30/10/2026 | `3a45486` | Sí, 30/10 | Validaciones en el servidor, permisos por rol (observación 3), llave JWT fuera del repositorio |
| Semana 3 | — | 30/10/2026 | `b115dce` | Sí, 30/10 (con `TZ=America/Guatemala`) | Especialista asignado, estados de la solicitud, mensaje honesto del correo |
| Revisión 30/10 | — | 30/10/2026 | `8a7bbb2` | Sí, 30/10 | Reintento al despertar la base, validaciones en solicitud y ficha |
| 1.0.0-rc1 | `v1.0.0-rc1` (sobre `8b5dc27`) | 05 y 06/11/2026 | `cf6c07e` | Sí, 06/11 (frontend `index-5cSokZa6.js`) | Semanas 4 y 5: transacciones, límite de intentos, e2e, celular |
| 1.0.0-rc2 | `v1.0.0-rc2` (se crea al integrar, sobre `675011f`) | 10/11/2026 | `675011f` (rama `ac-14-documentacion`, que contiene todas las ramas por AC) | Sí, 10/11/2026 a las 21:30 (hora de Guatemala). `/api/Version` = `1.0.0-rc2` / `675011f`; frontend `index-D_gHDQwW.js`. Respaldo previo: `respaldo-azure-20261110-2128.zip` (fuera del repositorio) | Acciones correctivas de SQA-ACM-01 |

## Contenido de la 1.0.0-rc2 por rama

Las ramas están encadenadas y se integran a `main` en este orden, cada una con su pull request revisado:

| Orden | Rama | Commits | Acciones |
|---|---|---|---|
| 1 | `ac-01-saldo-familiar` | `8eff41c` | AC-01 |
| 2 | `ac-07-pagos-fundacion` | `724c861` | AC-07 |
| 3 | `ac-17-sesion-vencida` | `a915aa0` | AC-17 |
| 4 | `ac-15-version-visible` | `81e9476` | AC-15 |
| 5 | `ac-10-permisos-informes` | `ec1c413` | AC-10, AC-04, AC-09 |
| 6 | `ac-13-interfaz` | `509e457`, `85661ee` | AC-13 (y AC-04: historial de visitas) |
| 7 | `ac-08-operaciones` | `1d77ec0`, `2bed6d6` | AC-08, AC-05, AC-06, AC-16 (fecha local) |
| 8 | `ac-14-documentacion` | (esta rama) | AC-14, AC-18, AC-02 (pruebas locales del token) |

## Cómo se publica una versión

1. Integrar las ramas en orden y crear la etiqueta en el último commit de `main`.
2. Ejecutar `despliegue/actualizar-azure.ps1` con el árbol limpio. El script hace un respaldo, pasa el commit a la compilación y avisa si hay cambios sin commit.
3. Abrir el sitio y comparar la versión y el commit del pie con la etiqueta.
4. Anotar aquí la fecha, el commit y el nombre del paquete publicado.
