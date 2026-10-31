# Semana 1 del plan de pruebas de SQA

**Fecha:** 26 de octubre de 2026
**Plan de referencia:** «Plan de pruebas para la implementación y validación del sistema», SQA, 25/10/2026 (commit probado por SQA: `0e0a423`).
**Alcance de esta entrega:** lo que el cronograma del plan asigna al desarrollador en la semana 1: corregir la compilación en Linux (CMP-01), el análisis estático (CMP-02), la auditoría de dependencias (CMP-03) e incorporar las pruebas automatizadas del Anexo A (fase 1 de automatización).

## Resumen

| Caso | Antes (resultado de SQA, 24/10) | Después (26/10) | Estado propuesto |
|---|---|---|---|
| PU-FE-08 · importaciones con mayúsculas exactas | Fallido (2 importaciones) | Aprobado | Listo para verificar |
| CMP-01 · compilación del frontend en Linux | Fallido | Causa corregida; compila en Windows. **Falta repetirlo en Linux** | Listo para verificar |
| CMP-02 · ESLint | Fallido: 16 errores y 3 advertencias | 0 errores y 0 advertencias | Listo para verificar |
| CMP-03 · auditoría npm | Fallido: 5 avisos altos | 0 vulnerabilidades | Listo para verificar |
| PU-FE-01 a PU-FE-07 | Aprobados (en la copia de SQA) | Aprobados, ahora dentro del repositorio | Aprobado |

Resultado de `npm test`: **3 archivos y 8 pruebas aprobadas de 8**.

## 1. Compilación en Linux (CMP-01 y PU-FE-08)

**Causa:** dos importaciones no coincidían en mayúsculas con el archivo real. Windows no distingue mayúsculas en los nombres de archivo, por eso el sistema funcionaba en el equipo del desarrollador y en Azure (que se publicó desde Windows), pero Linux sí las distingue y la compilación fallaba.

| Archivo | Antes | Después | Archivo real |
|---|---|---|---|
| `src/App.jsx` | `./componentes/RutaProtegida` | `./componentes/Rutaprotegida` | `componentes/Rutaprotegida.jsx` |
| `src/Interfaces/Login.jsx` | `../servicios/autenticacionServicio` | `../Servicios/autenticacionServicio` | `Servicios/autenticacionServicio.js` |

Se corrigieron las importaciones (no se renombraron los archivos) para hacer el cambio mínimo. La prueba PU-FE-08 revisa **todas** las importaciones del proyecto, así que un error igual en el futuro se detecta al ejecutar `npm test`.

**Limitación:** la compilación se repitió en Windows (`npm run build` correcto). No se tiene un equipo Linux; queda pendiente que SQA la repita o que lo haga el pipeline de GitHub Actions (fase 4 del plan).

## 2. ESLint (CMP-02)

De 19 problemas a 0. Ninguno cambiaba lo que el usuario ve; se verificó en el navegador (sección 5).

| Problema | Archivos | Corrección |
|---|---|---|
| Variable `err` o `e` del `catch` sin usar (8) | Citas, Internos, Login, Reportes, Solicitudes (2), Layout | `catch { ... }` sin variable, que JavaScript permite desde 2019 |
| Función de carga usada dentro de `useEffect` antes de declararla, o que cambia el estado directamente dentro del efecto (7) | Cobros, Donaciones, Examenes, FichaInterno, Gastos, Inicio, Prescripciones | El efecto pide los datos y los guarda con `.then(...)`, como ya hacían otras pantallas. La función `cargar` se mantiene para recargar después de guardar |
| `useEffect` con dependencias incompletas (3) | FichaInterno, Inicio, Solicitudes | Se declaran las dependencias reales (`id`, `rol`, `esMedicoGeneral`, `esAdministrador`) |
| Estado cambiado dentro de un efecto al cambiar interno o concepto | Cobros | La limpieza se hace en la función `cambiarSeleccion`, que se llama desde los dos `select` |
| Archivo que exporta un componente y un hook | AutenticacionContexto | Se desactiva esa regla solo en esa línea, con un comentario: la regla afecta únicamente la recarga en caliente durante el desarrollo, no al sistema |

Además, en `Inicio.jsx` la consulta de las tarjetas se movió a una función aparte, `obtenerTarjetas(rol)`, que solo devuelve los datos. Es más fácil de leer y de probar.

Las pruebas usan `__dirname` y `fs`, que son de Node, así que en `eslint.config.js` se agregó un bloque solo para `src/__tests__/` con las variables de Node.

## 3. Dependencias (CMP-03)

Se ejecutó `npm audit fix` (sin `--force`, sin cambios de versión mayor). Las 5 alertas altas eran de `brace-expansion`, `follow-redirects` (la que llega a producción por medio de axios), `source-map-js`, `vite` y `zod`.

- `vite` pasó a 8.3.5 y `follow-redirects` a 1.16.2, entre otras.
- En `package.json` la versión mínima de `vite` quedó en `^8.3.5`, para que nadie instale una versión vulnerable.
- Resultado: `npm audit` → **found 0 vulnerabilities**.
- NuGet (backend): no se tocó en esta entrega. En la corrección anterior ya no reportaba paquetes vulnerables; queda pendiente que SQA lo repita.

## 4. Pruebas automatizadas del frontend (fase 1)

Se incorporaron sin cambios las pruebas que escribió SQA en el Anexo A:

- `vitest.config.js`
- `src/__tests__/api.test.js` (PU-FE-01 a PU-FE-05)
- `src/__tests__/rutaProtegida.test.jsx` (PU-FE-06 y PU-FE-07)
- `src/__tests__/importaciones.test.js` (PU-FE-08)

Dependencias de desarrollo agregadas: `vitest` 5.0.5 (la misma versión que usó SQA), `jsdom`, `@testing-library/react` y `@testing-library/dom`. En `package.json` se agregó el script `"test": "vitest run"`.

```
 Test Files  3 passed (3)
      Tests  8 passed (8)
```

## 5. Verificación en el navegador

Con la API y el frontend locales y el usuario Administrador de prueba se revisaron las pantallas modificadas:

| Pantalla | Resultado |
|---|---|
| Inicio | Muestra las 4 tarjetas del Administrador |
| Internos y Ficha del interno | La lista y la ficha cargan con datos, psicopatologías, medicamentos e historial |
| Solicitudes | Lista las solicitudes |
| Cobros | Al elegir interno y «Cargo médico» aparecen sus 3 visitas; al volver a «Cuota mensual» la lista desaparece (no se guardó ningún cobro) |
| Donaciones, Gastos, Exámenes, Prescripciones | Cargan sus registros |
| Login con contraseña incorrecta | Muestra «Usuario o contraseña incorrectos.» |

La consola del navegador no mostró errores.

## Qué sigue (según el cronograma del plan)

- **Semana 1, parte de SQA:** repetir CMP-01 en Linux y confirmar PU-FE-08, CMP-02 y CMP-03.
- **Pendiente del desarrollador:** entregar las cuentas de prueba por un canal privado y preparar el entorno de pruebas separado de Azure.
- **Semana 2:** corregir los cargos duplicados (LAB-06, FAR-05) y la validación de montos en el servidor (FIN-05, LAB-03, FAR-03, RES-02, RES-03), con pruebas xUnit (fase 2).
