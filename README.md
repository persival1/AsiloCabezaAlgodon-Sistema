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

Si SQL Server no usa la instancia `localhost\SQLEXPRESS`, cambiar la cadena de conexión en `AsiloCabezaAlgodon/AsiloCabezaAlgodon.API/appsettings.json`.

## Roles del sistema

Administrador (fundación y administración del asilo), Médico General, Especialista, Enfermero, Laboratorio y Farmacia. Cada rol ve solo las opciones que necesita, y el servidor valida los permisos con `[Authorize(Roles = ...)]`.

## Nota

La llave del JWT y los demás valores de `appsettings.json` son de **desarrollo**. Antes de un despliegue real deben cambiarse y el controlador `SeedController` debe eliminarse o protegerse.
