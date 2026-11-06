# Semana 5 del plan de pruebas de SQA (05/11/2026)

La semana 5 del plan pide la fase 5 de automatización (recorridos completos en el navegador) y preparar la versión candidata. También se hizo la fase 3, que había quedado pendiente.

## 1. Recorridos completos en el navegador (fase 5, Playwright)

Carpeta `AsiloCabezaAlgodon.Web/e2e`. Se corren con `npm run e2e`, que levanta solos la API y el frontend. Usan el Edge instalado en Windows, así que no hace falta descargar navegadores. **Crean datos** (un interno, una solicitud, una visita y dos cobros por ejecución): solo se corren en local, nunca en Azure.

| Escenario | Qué hace el navegador | Casos del plan | Resultado |
|---|---|---|---|
| **E1** Atención médica completa | El médico general registra al interno y pide la cita → el administrador asigna especialista y fecha → el especialista registra la visita con un examen y una receta → laboratorio registra el resultado → farmacia despacha → el administrador genera los informes 1 y 2 (total de la cita Q192.50) | RES-01, EXP-04, EXP-05, EXP-08, LAB-02, FAR-02, REP-01, REP-02 | Pasa |
| **E2** Pago parcial y total | La visita aparece «Pendiente de pago» (Q0 de Q192.50) → cobro de Q100 (sigue pendiente) → cobro de Q92.50 (queda «Pagada») → el saldo del familiar es Q0 → la conciliación cuadra | FIN-02, FIN-03, REP-08 | Pasa |
| **E3** Sesión vencida | Con un token falso, al abrir una pantalla vuelve al login con el aviso «Tu sesión expiró…» y borra el token | AUT-10 | Pasa |
| **Celular** (agregado) | Con 375 px de ancho, cinco pantallas caben a lo ancho y las tablas se deslizan de lado. Guarda capturas como evidencia | — | Pasa |

**E4 (cierre de mes)** no se automatizó: el plan pide dos o tres recorridos (E1 y E2) y lo deja para la regresión de SQA.

**Observación:** en E3, la consola del servidor de desarrollo muestra «Unhandled rejection … 401». Es la pantalla que intenta cargar datos con el token vencido justo antes de volver al login. El usuario ve el comportamiento correcto (aviso y regreso al login); el mensaje solo aparece en la consola.

## 2. Matriz de permisos automatizada (fase 3)

`AsiloCabezaAlgodon.Tests/Integracion/PermisosPorRolTests.cs`. Los mismos 142 casos del script `pruebas/probar-permisos.ps1`, como prueba parametrizada de xUnit:

- **`WebApplicationFactory<Program>`** levanta la API completa en memoria: los mismos controllers, `[Authorize]` y middleware.
- Usa una base de pruebas aparte, `asilo_pruebas_permisos`, con un usuario por rol y el interno 1.
- Los tokens se generan con el `JwtTokenService` de la API, así que no pasan por el login ni por su límite de intentos.

**Comprobación de que detecta fallas:** se abrió a propósito `GastosController` a cualquier rol (`[Authorize]` sin roles) y **fallaron 10 casos**: los 5 roles que no deben ver ni registrar gastos. Se restauró y vuelve a pasar.

En GitHub Actions estas pruebas se excluyen (`Tipo!=Integracion`) porque necesitan SQL Server.

## 3. Pantallas para celular (responsive)

Antes, el menú lateral medía siempre 240 px. En un teléfono de 375 px la página medía **530 px de ancho** y se salía de la pantalla; esto se midió con la prueba de celular antes de corregir. Ahora, en pantallas de hasta 768 px (`@media` en `index.css`):

- el menú pasa a ser una barra arriba;
- los campos de los formularios se apilan;
- las tablas se deslizan de lado.

Solo cambió CSS: ni la lógica ni el backend.

## 4. Limpieza del código

| Antes | Ahora |
|---|---|
| `WeatherForecast.cs` (ejemplo de la plantilla de .NET) | Borrado |
| `EquiposController.cs` (clase vacía) | Borrado (y quitado de «errores conocidos» del manual) |
| `Proximamente.jsx` (sin ruta que la use), `App.css`, `hero.png`, `react.svg`, `vite.svg` (de la plantilla de Vite) | Borrados |
| Carpeta `Application/Auntenticacion` (mal escrita) | `Application/Autenticacion` |
| `Domain/Familiar.cs` fuera de `Entidad/` | `Domain/Entidad/Familiar.cs` |
| Interfaces en `Comun/` con namespace `Comun.Interfaces` | `Comun/Interfaces/` (carpeta igual al namespace) |
| `Rutaprotegida.jsx` (el componente se llama `RutaProtegida`) | `RutaProtegida.jsx` |

Antes de borrar se comprobó que ningún archivo los usaba. Después: compilación con 0 advertencias y todas las pruebas en verde.

## 5. Regresión completa (versión candidata)

| Prueba | Resultado |
|---|---|
| `dotnet test` (todas) | **215 de 215**: 70 unitarias, 2 de transacción (ROB-03), 142 de permisos y 1 que verifica que la matriz tenga 142 casos |
| `dotnet test --filter "Tipo!=Integracion"` (lo que corre GitHub) | 70 de 70 |
| Paquetes NuGet vulnerables | 0 en los 5 proyectos |
| Frontend: lint, pruebas, build | 0 problemas · 8 de 8 · compila |
| `npm audit --omit=dev` (lo que se publica) | 0 vulnerabilidades. Todo el árbol: 4 moderadas, solo de ESLint (desarrollo) |
| Navegador (Playwright) | 4 de 4 |
| `probar-semana-3.ps1` | 11 de 11, la conciliación cuadra |
| `probar-permisos.ps1` | 142 de 142 |
| `probar-validaciones.ps1` | 20 de 20, datos sin cambios |
| `probar-semana-4.ps1` | 19 de 19 |

Entre scripts se espera un minuto, por el límite de 10 intentos de login por minuto.

La entrega de la versión candidata (lista de verificación del apartado 12 del plan) está en `docs/version-candidata.md`.
