# Versión candidata v1.0.0-rc1 (05/11/2026)

Entrega al analista SQA según el apartado 12 del plan de pruebas («Protocolo de entrega de versiones»). Mientras no se firme el acta del plan, este documento es la propuesta del desarrollador.

- **Etiqueta de Git:** `v1.0.0-rc1`. Se crea en local y sube a GitHub con el próximo push.
- **Código de la versión:** commit `cf6c07e`. Los commits posteriores solo cambian documentos.
- **Entorno publicado:** https://asilo-cabeza-algodon-2796.azurewebsites.net. **Todavía tiene la versión anterior**: la revisión del 30/10, commit `8a7bbb2`. Publicar esta versión candidata está pendiente de aprobación (ver «Pendiente»).

## 1. Lista de verificación antes de entregar (tabla 20 del plan)

| N.º | Condición | Estado |
|---|---|---|
| 1 | Etiqueta en Git y commit desplegado identificado | ✅ Etiqueta `v1.0.0-rc1`. ⏳ El commit desplegado se registra al publicar |
| 2 | Backend y frontend compilan en Linux y en Windows | ✅ Windows. ⏳ Linux: lo comprueba GitHub Actions en el primer push (`.github/workflows/calidad.yml`) |
| 3 | Las pruebas automatizadas pasan | ✅ xUnit 215 de 215 · Vitest 8 de 8 · Playwright 4 de 4 |
| 4 | Se ejecutaron los casos del catálogo afectados y se registraron | ✅ Semanas 1 a 5 (`pruebas/resultado-semana-*.md` y `pruebas/resultados-*.md`) |
| 5 | Regresión mínima del apartado 7.9 | ✅ Permisos 142 de 142 · conciliación (REP-08) · flujo EXP-08 → LAB-02 → FAR-02 → FIN-02 en el navegador (E1 y E2). ⚠️ Informes: solo los informes 1 y 2 y la conciliación se verifican de forma automática; los ocho «con los datos de referencia» los ejecuta SQA. ⏳ CMP-01 en Linux: con el primer push |
| 6 | Sin defectos críticos abiertos; los altos con tratamiento acordado | ✅ No hay defectos críticos ni altos abiertos conocidos (ver limitaciones) |
| 7 | Auditorías de NuGet y npm sin avisos altos nuevos | ✅ NuGet: 0 paquetes vulnerables. npm de lo publicado: 0. npm completo: 4 moderados, solo de ESLint (desarrollo) |
| 8 | Registro de cambios con lo corregido, lo pendiente y los riesgos | ✅ `docs/registro-de-cambios-sqa.md` |
| 9 | Cuentas de prueba entregadas al analista por un canal privado | ⏳ Las entrega el desarrollador por mensaje privado (no van en el repositorio) |

## 2. Paquete de entrega (apartado 12.2)

| Contenido | Dónde |
|---|---|
| Etiqueta y commit | `v1.0.0-rc1` / `cf6c07e` |
| Registro de cambios con IDs de casos | `docs/registro-de-cambios-sqa.md` |
| Resultados de pruebas | `pruebas/resultado-semana-1-plan-sqa.md` a `resultado-semana-5-plan-sqa.md`, `pruebas/resultados-*.md` y, después del push, la pestaña Actions de GitHub |
| Defectos y limitaciones conocidas | Apartado 3 de este documento |
| Instalación y URL del entorno, sin credenciales | `README.md` (puesta en marcha) y `despliegue/LEEME.md` |

## 3. Limitaciones y riesgos conocidos

| Tema | Situación |
|---|---|
| Correo real (COR-01) | El sistema informa con honestidad si el correo no salió, pero en Azure falta configurar la cuenta de Gmail (`Smtp__*`) |
| Límite de intentos de login | Funciona en local (10 por minuto por IP). En Azure hay que comprobar que el servidor ve la IP real de cada usuario y no la del balanceador de Azure |
| Usuario de base con permisos mínimos | Script listo (`base-de-datos/usuario-permisos-minimos.sql`); se aplica al publicar. Hasta entonces la aplicación usa el administrador del servidor SQL |
| Escenario E4 (cierre de mes) | No automatizado en el navegador; queda para la regresión de SQA |
| Consola del navegador en sesión vencida | Aparece «Unhandled rejection 401» en la consola mientras la pantalla vuelve al login; el usuario ve el comportamiento correcto |
| Fechas guardadas en Azure entre el 21 y el 30/10 | Quedaron en UTC (6 horas adelante); desde el 30/10 se guardan en hora de Guatemala |
| Plan de Azure | Temporalmente B1 con Always On para la revisión; vuelve a F1 gratis al terminarla |

## 4. Pendiente para completar la entrega

1. **Push** de los commits y la etiqueta (GitHub Desktop), y revisar que GitHub Actions salga en verde (condiciones 1, 2 y CMP-01).
2. **Publicar esta versión en Azure** con `despliegue/actualizar-azure.ps1`, crear el usuario `app_asilo` y cambiar la cadena de conexión. Después: 11 comprobaciones básicas, 142 permisos e informes contra Azure. El paso necesita aprobación expresa porque toca producción.
3. **Enviar las cuentas** de Azure al analista por un canal privado (condición 9).
