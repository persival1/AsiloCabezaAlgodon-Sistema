# Asilo Cabeza de Algodón — Sistema de Administración

Sistema para la gestión integral del Asilo de Ancianos Cabeza de Algodón: ficha médica de internos, solicitudes y visitas médicas con la fundación aliada, laboratorio, farmacia, y el módulo de caja (donaciones, cuotas y gastos), con generación de los 7 reportes que exige el proyecto.

Proyecto final — Análisis y Diseño de Sistemas II, Universidad Mariano Gálvez, Centro Regional de Mazatenango. Catedrático: Ing. Angel Atilio Maltez C.

> Manual de usuario e instalación. Incluye el modelo E-R y el diagrama de componentes de la arquitectura.

---

## 1. Descripción general

El sistema automatiza el flujo real del asilo:

1. El **Médico General** evalúa a un interno y crea una **solicitud** indicando el motivo.
2. La **Fundación** (rol Administrador) recibe la solicitud, asigna **especialista**, **enfermero(a) acompañante** y **horario**, y el sistema envía un **correo automático** al familiar informándole.
3. El **Especialista** atiende la cita, registra la **visita médica** (diagnóstico, observaciones, costo de la consulta) y, si hace falta, solicita **exámenes de laboratorio** y receta **medicamentos**.
4. **Laboratorio** ve los exámenes pendientes, registra el resultado y su costo.
5. **Farmacia** ve las prescripciones pendientes, despacha el medicamento y registra su costo.
6. Cada costo capturado (consulta, examen, medicamento) — **ya con el descuento que da la fundación** — se carga automáticamente a la cuenta del familiar responsable.
7. El módulo de **Caja** lleva el control de **donaciones** (empresas, gobierno, particulares), **cobros** a familiares (cuota mensual y cargos médicos) y **gastos** operativos del asilo.
8. El módulo de **Reportes** genera los 7 informes exactos que pide el enunciado.

---

## 2. Arquitectura del sistema

El backend sigue **Clean Architecture** con **CQRS** (usando MediatR); el frontend es una SPA independiente que consume la API por HTTP/JWT.

```
┌─────────────────────────────────────────────────────────────────┐
│  AsiloCabezaAlgodon.Web  (React + Vite)                         │
│  Interfaces/*.jsx  →  Servicios/*.js  →  axios (JWT en header)  │
└───────────────────────────────┬───────────────────────────────┘
                                 │ HTTPS (REST/JSON)
┌────────────────────────────────▼───────────────────────────────┐
│  AsiloCabezaAlgodon.API            (ASP.NET Core Web API)       │
│  Controllers/*.cs — [Authorize(Roles = "...")] por endpoint     │
│  Program.cs — inyección de dependencias, JWT, CORS               │
└───────────────┬──────────────────────────────┬──────────────────┘
                │ IMediator.Send()              │ implementa
┌───────────────▼──────────────────┐  ┌─────────▼─────────────────┐
│  AsiloCabezaAlgodon.Application   │  │ AsiloCabezaAlgodon.       │
│  Commands / Queries (CQRS)        │  │ Infrastructure            │
│  — un Handler por caso de uso     │  │ Repositorios (EF Core)    │
│  — Interfaces de repositorio      │◄─┤ BDConexion (DbContext)    │
│  — DTOs                           │  │ Seguridad (JWT, BCrypt)   │
│                                    │  │ Servicios (Email SMTP)    │
└───────────────┬───────────────────┘  └─────────┬─────────────────┘
                │ usa entidades                    │ mapea a tablas
┌───────────────▼───────────────────────────────────▼──────────────┐
│  AsiloCabezaAlgodon.Domain                                       │
│  Entidades puras (Usuario, Interno, Solicitud, VisitaMedica...)  │
│  Sin dependencias externas — el centro de la arquitectura         │
└────────────────────────────────────────────────────────────────┘
                                 │ EF Core Migrations
                                 ▼
                        ┌──────────────────┐
                        │   SQL Server      │
                        └──────────────────┘
```

> Versión interactiva del diagrama (con leyenda y notas): [`diagrama-componentes.html`](./diagrama-componentes.html) — ábrilo directo en el navegador.

**Regla de dependencia:** las capas de afuera dependen de las de adentro, nunca al revés. `Domain` no conoce a nadie; `Application` solo conoce `Domain`; `Infrastructure` implementa las interfaces que `Application` define; `API` orquesta todo con inyección de dependencias.

**Por qué CQRS/MediatR:** cada caso de uso (crear solicitud, asignar médico, registrar visita, despachar prescripción...) es un `Command` o `Query` con su propio `Handler`. Facilita explicar el sistema caso por caso y mantiene los controllers delgados (solo reciben la petición HTTP y llaman a `_mediator.Send(...)`).

**Por qué JWT hecho a mano (sin ASP.NET Identity):** con solo 6 roles fijos, Identity es sobre-ingeniería. Se usa `BCrypt.Net-Next` para el hash de contraseñas y `System.IdentityModel.Tokens.Jwt` para generar y validar el token — más simple de explicar y de mantener.

### Estructura de carpetas

```
AsiloCabezaAlgodon/                    (solución .NET — backend)
├── AsiloCabezaAlgodon.Domain/         Entidades (Usuario, Interno, Solicitud, VisitaMedica, ...)
├── AsiloCabezaAlgodon.Application/    Commands, Queries, Handlers, interfaces de repositorio, DTOs
│   ├── Autenticacion/  Internos/  Solicitudes/  VisitasMedicas/
│   ├── Examenes/  Prescripciones/  Caja/  Reportes/  Usuarios/
├── AsiloCabezaAlgodon.Infrastructure/ Repositorios EF Core, DbContext, JWT, BCrypt, Email SMTP
│   ├── Persistencia/  Seguridad/  Servicios/  Migrations/
├── AsiloCabezaAlgodon.API/            Controllers, Program.cs, appsettings.json
└── modelo-er.html                     Diagrama E-R (ver sección 4)

AsiloCabezaAlgodon.Web/                (SPA — frontend, proyecto separado)
├── src/Interfaces/    Una pantalla por módulo (Internos.jsx, Solicitudes.jsx, ...)
├── src/Servicios/     Un archivo por recurso de la API (axios)
├── src/componentes/   Layout, RutaProtegida, Badge, Iconos
└── src/contexto/      AutenticacionContexto (JWT + rol en localStorage)
```

---

## 3. Roles del sistema

| Rol | Qué hace |
|---|---|
| **Administrador** | Ve todo: internos, asigna solicitudes, visitas médicas, donaciones, cobros, gastos, los 7 reportes |
| **MedicoGeneral** | Registra internos, crea solicitudes de evaluación |
| **Especialista** | Ve sus citas asignadas, la ficha médica del paciente, y registra la visita (diagnóstico, exámenes, medicamentos, costo) |
| **Enfermero** | Ve los acompañamientos que le asignó la fundación |
| **Laboratorio** | Ve exámenes pendientes, registra resultado y costo |
| **Farmacia** | Ve prescripciones pendientes, despacha y registra costo |

---

## 4. Modelo de datos (E-R)

El diagrama entidad-relación completo (13 tablas, llaves primarias/foráneas y cardinalidad) está en [`modelo-er.html`](./modelo-er.html) — ábrilo directo en el navegador. Resume los 4 dominios del sistema: **Seguridad** (Usuario, Rol), **Ficha médica** (Interno, Familiar, Psicopatología, MedicamentoCajón), **Flujo médico/Fundación** (Solicitud, VisitaMedica, ExamenSolicitado, Prescripción) y **Caja** (Donación, Gasto, Cobro).

---

## 5. Instalación y puesta en marcha

### 5.1 Requisitos previos

| Herramienta | Uso | Notas |
|---|---|---|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | Compilar y correr el backend | `dotnet --version` para verificar |
| [Node.js 20+](https://nodejs.org/) | Compilar y correr el frontend | incluye `npm` |
| SQL Server (Express, LocalDB o instancia completa) | Base de datos | se puede usar `Server=localhost\SQLEXPRESS` o cambiar la cadena de conexión |
| `dotnet-ef` (herramienta global) | Aplicar migraciones | `dotnet tool install --global dotnet-ef` si no la tenés |

### 5.2 Clonar y restaurar

El repositorio contiene las dos partes del sistema (backend y frontend) en carpetas separadas.

```bash
git clone https://github.com/persival1/AsiloCabezaAlgodon-Sistema.git
cd AsiloCabezaAlgodon-Sistema/AsiloCabezaAlgodon
dotnet restore
```

Los comandos de 5.3 a 5.5 se ejecutan desde esta carpeta (`AsiloCabezaAlgodon`, la del backend).

### 5.3 Configurar la base de datos

Editá `AsiloCabezaAlgodon.API/appsettings.json` si tu SQL Server no usa la configuración por defecto:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=asilo_cabeza_algodon;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Aplicá las migraciones (crea la base y todas las tablas):

```bash
dotnet ef database update --project AsiloCabezaAlgodon.Infrastructure --startup-project AsiloCabezaAlgodon.API
```

### 5.4 Sembrar usuarios de prueba

Con el backend corriendo (ver 5.5), abrí `https://localhost:7022/swagger` y ejecutá, en este orden:

1. `POST /api/Seed/admin` — crea los 6 roles y el usuario `admin`
2. `POST /api/Seed/usuarios-prueba` — crea un usuario de prueba por cada rol restante

> Borrá o restringí `SeedController` antes de un despliegue real — está pensado solo para poblar datos de prueba.

### 5.5 Correr el backend

```bash
cd AsiloCabezaAlgodon.API
dotnet run --launch-profile https
```

(Desde la carpeta del backend, `AsiloCabezaAlgodon`.)

Queda escuchando en `https://localhost:7022` (y `http://localhost:5253`). Swagger disponible en `/swagger`.

### 5.6 Correr el frontend

En otra terminal, desde la **raíz del repositorio** (`AsiloCabezaAlgodon-Sistema`):

```bash
cd AsiloCabezaAlgodon.Web
npm install
npm run dev
```

Queda en `http://localhost:5173`. El archivo `src/Servicios/api.js` apunta a `https://localhost:7022/api` — cambialo si corrés el backend en otro puerto.

### 5.7 Configurar el envío de correo (opcional)

El sistema envía un correo al familiar cuando se asigna una solicitud. Mientras `Smtp:Host`/`Smtp:Usuario`/`Smtp:Password` en `appsettings.json` estén vacíos, el sistema **no falla** — solo registra en el log que no pudo enviar. Para activarlo de verdad (ej. con Gmail y una contraseña de aplicación):

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Puerto": "587",
  "Usuario": "tu-correo@gmail.com",
  "Password": "contraseña-de-aplicación",
  "Remitente": "tu-correo@gmail.com"
}
```

---

## 6. Credenciales de prueba

Generadas por `/api/Seed/*` (ver 5.4):

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `admin123` | Administrador |
| `medicogeneral` | `prueba123` | MedicoGeneral |
| `especialista` | `prueba123` | Especialista |
| `enfermero` | `prueba123` | Enfermero |
| `laboratorio` | `prueba123` | Laboratorio |
| `farmacia` | `prueba123` | Farmacia |

---

## 7. Recorrido rápido por módulo

- **Internos** (Administrador / MedicoGeneral crean; Especialista solo consulta): alta de un interno junto con su familiar responsable (cuota mensual incluida) en un mismo formulario. Cada fila tiene un link **Ver ficha**.
- **Ficha médica** (`/dashboard/internos/:id`, Administrador / MedicoGeneral / Especialista): datos generales, familiar, **psicopatologías** y **medicamentos de cajón** (con formulario para agregar nuevos registros) y el historial completo de visitas médicas del paciente — esto es lo que el enunciado pide que el médico pueda consultar antes de atender.
- **Solicitudes**: MedicoGeneral crea la solicitud sobre un interno → queda *Pendiente*. Administrador la asigna a un Especialista (mostrando su **especialidad**) + Enfermero(a) + fecha/hora → queda *Asignada* y sale el correo al familiar.
- **Mis citas** (Especialista): registra la visita médica — diagnóstico, observaciones, costo de consulta, exámenes a solicitar y medicamentos a recetar — todo en un solo formulario. La solicitud pasa a *Atendida*.
- **Visitas médicas**: historial de visitas con su diagnóstico, exámenes, prescripciones y un **estado de pago** (Pagada / Pendiente de pago / Sin cargos) calculado comparando el total cargado contra los cobros vinculados a esa visita.
- **Exámenes solicitados** (Laboratorio): registra resultado + costo de cada examen pendiente.
- **Prescripciones** (Farmacia): despacha cada medicamento pendiente y registra su costo.
- **Donaciones / Cobros / Gastos** (Administrador): caja del asilo. Un cobro con concepto "Cargo médico" se vincula a una visita específica (así se sabe cuáles citas están completamente canceladas); un cobro con concepto "Cuota mensual" no se vincula a ninguna. Cada cobro/cargo médico actualiza el saldo pendiente del familiar automáticamente.
- **Reportes** (Administrador): los 7 informes del enunciado, con selector de reporte y filtros de paciente/rango de fechas donde aplica.

---

## 8. Limitaciones y errores conocidos

- **Correo**: si `Smtp:*` no está configurado, no se envía nada (ver 5.7). No es un error del sistema, es el comportamiento esperado sin credenciales reales.
- **Descuentos**: el sistema no calcula descuentos automáticamente. Quien captura el costo (especialista, laboratorio, farmacia) lo ingresa **ya con el descuento de la fundación aplicado** — es una simplificación deliberada para no construir un motor de tarifas que el enunciado no detalla.
- **`EquiposController`**: controlador vacío sin usar, remanente de una prueba temprana; no afecta el funcionamiento del sistema.
- **Sin recuperación de contraseña ni edición de usuarios desde la interfaz**: la gestión de usuarios (incluida la `Especialidad` de un especialista) se hace por ahora vía `/api/Seed` o directo en la base de datos.
- **HTTPS con certificado de desarrollo**: al abrir `https://localhost:7022/swagger` por primera vez el navegador puede advertir de certificado no confiable — es el certificado de desarrollo de .NET (`dotnet dev-certs https --trust` lo soluciona localmente).

---

## 9. Créditos

Desarrollado para la cátedra de Análisis y Diseño de Sistemas II — Universidad Mariano Gálvez de Guatemala, Centro Regional de Mazatenango.
