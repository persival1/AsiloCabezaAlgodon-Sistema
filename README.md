# Sistema para Administración del Asilo de Ancianos Cabeza de Algodón

Proyecto final — Análisis y Diseño de Sistemas II
Universidad Mariano Gálvez de Guatemala, Centro Regional de Mazatenango
Catedrático: Ing. Angel Atilio Maltez C.

Este repositorio contiene el sistema completo en dos carpetas:

| Carpeta | Contenido | Tecnología |
|---|---|---|
| [`AsiloCabezaAlgodon/`](./AsiloCabezaAlgodon) | Backend (API) | .NET 10, Clean Architecture, CQRS con MediatR, Entity Framework Core, SQL Server |
| [`AsiloCabezaAlgodon.Web/`](./AsiloCabezaAlgodon.Web) | Frontend (interfaz de usuario) | React + Vite |

El **manual de usuario e instalación completo** (arquitectura, modelo E-R, roles, credenciales de prueba, errores conocidos) está en [`AsiloCabezaAlgodon/README.md`](./AsiloCabezaAlgodon/README.md). Los diagramas están en la misma carpeta: `modelo-er.html` y `diagrama-componentes.html`.

## Puesta en marcha rápida

Requisitos: .NET SDK 10, Node.js 20 o superior y SQL Server (Express).

```bash
git clone https://github.com/persival1/AsiloCabezaAlgodon-Sistema.git
cd AsiloCabezaAlgodon-Sistema
```

**1. Backend** (crea la base de datos y arranca la API en `https://localhost:7022`)

```bash
cd AsiloCabezaAlgodon
dotnet restore
dotnet ef database update --project AsiloCabezaAlgodon.Infrastructure --startup-project AsiloCabezaAlgodon.API
cd AsiloCabezaAlgodon.API
dotnet run --launch-profile https
```

**2. Usuarios de prueba**: con la API corriendo, abrir `https://localhost:7022/swagger` y ejecutar `POST /api/Seed/admin` y después `POST /api/Seed/usuarios-prueba`.

**3. Frontend** (en otra terminal, desde la raíz del repositorio; queda en `http://localhost:5173`)

```bash
cd AsiloCabezaAlgodon.Web
npm install
npm run dev
```

Para revisar el frontend: `npm test` (pruebas automáticas con Vitest), `npm run lint` (ESLint) y `npm audit` (dependencias con vulnerabilidades). `npm run e2e` recorre el sistema en el navegador con Playwright (usa Edge; levanta solo la API y el frontend; **crea datos**, así que es solo para el entorno local).

Para el backend, desde la carpeta `AsiloCabezaAlgodon`: `dotnet test AsiloCabezaAlgodon.slnx` (pruebas con xUnit). Las de integración (`Tipo=Integracion`) usan SQL Express local con una base aparte, `asilo_pruebas_integracion`; para otra instancia, definir la variable `ASILO_BD_PRUEBAS`. Sin SQL Server: `dotnet test AsiloCabezaAlgodon.slnx --filter "Tipo!=Integracion"`.

En cada push, GitHub Actions (`.github/workflows/calidad.yml`) compila y prueba el backend y el frontend en Linux.

Si SQL Server no usa la instancia `localhost\SQLEXPRESS`, cambiar la cadena de conexión en `AsiloCabezaAlgodon/AsiloCabezaAlgodon.API/appsettings.json`.

## Pruebas y revisión de calidad

- [`docs/registro-de-cambios-sqa.md`](./docs/registro-de-cambios-sqa.md): respuesta punto por punto a la revisión preliminar de SQA (qué se cambió, evidencia y qué sigue pendiente).
- [`docs/matriz-de-trazabilidad.md`](./docs/matriz-de-trazabilidad.md): cada requerimiento unido a su pantalla, endpoint, prueba y estado.
- [`docs/supuestos-para-aprobacion.md`](./docs/supuestos-para-aprobacion.md): los 7 supuestos del análisis, en una tabla para aprobar o rechazar.
- [`docs/matriz-actores-y-roles.md`](./docs/matriz-actores-y-roles.md): actores del análisis, roles del sistema y qué puede hacer cada rol (AC-09 y AC-10).
- [`docs/regla-del-saldo.md`](./docs/regla-del-saldo.md): cómo se calcula el saldo del familiar (AC-01).
- [`docs/pagos-a-la-fundacion.md`](./docs/pagos-a-la-fundacion.md): registro de los pagos del asilo a la fundación (AC-07).
- [`docs/ac-13-interfaz.md`](./docs/ac-13-interfaz.md): mejoras de la interfaz por rol, con capturas antes y después y pruebas de navegación (AC-13).
- [`docs/guia-de-estilo.md`](./docs/guia-de-estilo.md): tipografía, colores, leyenda de estados y componentes de la interfaz.
- [`pruebas/resultado-conciliacion-y-sesion.md`](./pruebas/resultado-conciliacion-y-sesion.md): conciliación del saldo de los familiares y cierre de sesión cuando el token ya no sirve.
- [`pruebas/resultado-respaldo-restauracion.md`](./pruebas/resultado-respaldo-restauracion.md): prueba de respaldo y recuperación de la base de datos.
- [`pruebas/resultado-semana-1-plan-sqa.md`](./pruebas/resultado-semana-1-plan-sqa.md): semana 1 del plan de pruebas de SQA (compilación en Linux, ESLint, dependencias y pruebas automáticas del frontend).
- [`pruebas/resultado-semana-2-plan-sqa.md`](./pruebas/resultado-semana-2-plan-sqa.md): semana 2 (cargos duplicados y validación de datos en el servidor, con pruebas xUnit).
- [`pruebas/resultado-semana-3-plan-sqa.md`](./pruebas/resultado-semana-3-plan-sqa.md): semana 3 (solo el especialista asignado registra la visita, estados de la solicitud, validación de la asignación y mensaje honesto del correo).
- [`pruebas/resultado-revision-30-10.md`](./pruebas/resultado-revision-30-10.md): revisión de los errores registrados en Azure y de las operaciones que aún no validaban datos.
- [`pruebas/resultado-semana-4-plan-sqa.md`](./pruebas/resultado-semana-4-plan-sqa.md): semana 4 (transacción en las operaciones compuestas, límite de intentos de login, datos inesperados, pipeline de GitHub Actions y usuario de base con permisos mínimos).
- [`pruebas/resultado-semana-5-plan-sqa.md`](./pruebas/resultado-semana-5-plan-sqa.md): semana 5 (recorridos completos en el navegador con Playwright, matriz de permisos automatizada, pantallas para celular, limpieza del código y regresión completa).
- [`docs/version-candidata.md`](./docs/version-candidata.md): entrega de la versión candidata a SQA (lista de verificación, resultados y limitaciones conocidas).
- [`pruebas/probar-semana-4.ps1`](./pruebas/probar-semana-4.ps1): datos mal formados y límite de intentos de login. No crea datos; tarda unos dos minutos. Resultados en [`pruebas/resultados-semana-4.md`](./pruebas/resultados-semana-4.md).
- [`pruebas/probar-validaciones.ps1`](./pruebas/probar-validaciones.ps1): script que envía datos inválidos a la API y comprueba que se rechacen sin cambiar nada. Resultados en [`pruebas/resultados-validaciones.md`](./pruebas/resultados-validaciones.md).
- [`pruebas/probar-semana-3.ps1`](./pruebas/probar-semana-3.ps1): recorre el ciclo de una solicitud y prueba lo que no se debe poder hacer en cada paso. **Crea datos**: solo en local. Resultados en [`pruebas/resultados-semana-3.md`](./pruebas/resultados-semana-3.md).
- [`pruebas/probar-permisos.ps1`](./pruebas/probar-permisos.ps1): script que prueba los permisos de la API con los 6 roles (218 pruebas en la base de pruebas; el script agrega el acceso horizontal según los datos). Los resultados están en [`pruebas/resultados-permisos.md`](./pruebas/resultados-permisos.md).

## Base de datos

Se incluye el script [`base-de-datos/crear-base.sql`](./base-de-datos/crear-base.sql) para crear las tablas sin necesitar las herramientas de .NET. Las instrucciones están en [`base-de-datos/LEEME.md`](./base-de-datos/LEEME.md).

## Despliegue

**El sistema está publicado en Azure: https://asilo-cabeza-algodon-2796.azurewebsites.net**

Usa planes gratuitos (App Service F1 y la oferta gratis de Azure SQL); durante la revisión de SQA el App Service está temporalmente en B1. La API y el frontend están en la misma aplicación. Las contraseñas de los usuarios en Azure **no son las de prueba** de este README: se entregan en privado. Detalle del despliegue, medidas de seguridad y limitaciones en [`despliegue/LEEME.md`](./despliegue/LEEME.md); las pruebas del sistema publicado, en [`pruebas/resultado-despliegue-azure.md`](./pruebas/resultado-despliegue-azure.md).

Para publicarlo en otro servidor:

1. Publicar el backend con `dotnet publish` y el frontend con `npm run build`.
2. Definir la configuración propia del servidor con **variables de entorno**, siguiendo [`appsettings.Production.example.json`](./AsiloCabezaAlgodon/AsiloCabezaAlgodon.API/appsettings.Production.example.json): `ASPNETCORE_ENVIRONMENT=Production`, `Jwt__Key` (mínimo 32 caracteres), `ConnectionStrings__DefaultConnection` y los datos de `Smtp__*`. La llave del JWT **ya no está** en `appsettings.json`: en desarrollo viene de `appsettings.Development.json` y en producción la aplicación **no arranca** si no se define `Jwt__Key`.
3. Configurar las dos direcciones que cambian en un servidor: en el backend, el origen del frontend permitido por CORS (`Cors:Origenes`, por defecto `http://localhost:5173`); en el frontend, la dirección de la API con la variable `VITE_API_URL` (ver [`AsiloCabezaAlgodon.Web/.env.example`](./AsiloCabezaAlgodon.Web/.env.example)). Ya no están escritas dentro del código.
4. `SeedController` (crea los usuarios de prueba) **ya responde 404 fuera del entorno de desarrollo**, así que en producción no queda expuesto. Los usuarios reales deben crearse por otro medio.

## Roles del sistema

Administrador (fundación y administración del asilo), Médico General, Especialista, Enfermero, Laboratorio y Farmacia. Cada rol ve solo las opciones que necesita, y el servidor valida los permisos con `[Authorize(Roles = ...)]`.

## Nota

La llave del JWT y los demás valores de `appsettings.json` son de **desarrollo**. Antes de un despliegue real deben cambiarse y el controlador `SeedController` debe eliminarse o protegerse.
