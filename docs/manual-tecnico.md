# Manual técnico (AC-14, documento 6)

Sistema para Administración del Asilo de Ancianos Cabeza de Algodón, versión 1.0.0-rc2.

## 1. Arquitectura

```
React 19 + Vite (SPA)  ──HTTPS + JWT──▶  API ASP.NET Core (.NET 10)  ──EF Core──▶  SQL Server
AsiloCabezaAlgodon.Web                    AsiloCabezaAlgodon.API                     asilo_cabeza_algodon
```

En Azure, la API sirve también el frontend compilado (`wwwroot`) desde el mismo App Service. El diagrama de componentes está en `AsiloCabezaAlgodon/diagrama-componentes.html` y el modelo de datos (14 tablas) en `AsiloCabezaAlgodon/modelo-er.html`.

## 2. Capas del backend (Clean Architecture)

| Proyecto | Responsabilidad | Depende de |
|---|---|---|
| `Domain` | Entidades: Usuario, Rol, Interno, Familiar, Psicopatologia, MedicamentoCajon, Solicitud, VisitaMedica, ExamenSolicitado, Prescripcion, Donacion, Gasto, Cobro y PagoFundacion | Nada |
| `Application` | Casos de uso como comandos y consultas de MediatR; interfaces de repositorios, correo, JWT y hash; reglas de validación (`Comun/Validar.cs`); excepciones de negocio | Domain |
| `Infrastructure` | `BDConexion` (DbContext), repositorios, `UnidadDeTrabajo` (transacciones), `JwtTokenService`, `BCryptPasswordHasher`, `EmailService` (SMTP), migraciones | Application |
| `API` | Controladores, `[Authorize(Roles=…)]`, `Program.cs` (inyección de dependencias, JWT, CORS, límite de intentos, manejo de errores) | Application, Infrastructure |
| `Tests` | xUnit: handlers con NSubstitute; integración con SQL Server y la API en memoria (`WebApplicationFactory`) | Todos |

**CQRS con MediatR.** Cada operación es un `record` (comando o consulta) con su handler. El controlador solo arma el comando con los datos del token y llama `_mediator.Send(...)`. Ejemplo: `CancelarSolicitudCommand(IdSolicitud, IdUsuario, Rol)`, donde el usuario y el rol salen del token y no del cuerpo de la petición.

**Errores.** Los handlers lanzan tres excepciones, y un middleware de `Program.cs` las convierte en la respuesta correcta:

| Excepción | Respuesta | Cuándo |
|---|---|---|
| `DatosInvalidosException` | 400 | El dato está mal |
| `ConflictoException` | 409 | Choca con el estado actual |
| `AccesoDenegadoException` | 403 | El registro no es del usuario |

El mensaje se muestra tal cual en la pantalla.

**Transacciones.** Registrar una visita guarda tres cosas en una sola transacción (`IUnidadDeTrabajo`): la visita, el cargo al familiar y el estado de la solicitud. Prueba: `RegistrarVisitaTransaccionTests`.

## 3. Frontend

| Carpeta | Contenido |
|---|---|
| `src/Interfaces` | Una pantalla por módulo |
| `src/componentes` | `Layout` (menú por rol y por área), `RutaProtegida`, `Badge` (estados), `Saldo`, `ControlesTabla` (búsqueda, filtros y paginación), `EditarInterno`, `VersionApp` |
| `src/Servicios` | Una función por endpoint; `api.js` agrega el token y maneja el 401 (vuelve al login sin errores en la consola) |
| `src/utilidades` | Funciones puras con pruebas: `tabla.js`, `indicadores.js`, `validacionCampos.js`, `saldo.js` |

El menú solo muestra lo que el rol puede usar, pero **la seguridad está en la API**. Ocultar un botón no protege nada.

## 4. Seguridad

| Tema | Implementación |
|---|---|
| Autenticación | JWT HS256 de 8 horas, con validación de firma, emisor, audiencia y vencimiento. Sin `Jwt:Key` de al menos 32 caracteres, la API no arranca. Pruebas: `Token_*` (falsificado, alterado, vencido, otro emisor, sin firma) |
| Autorización vertical | `[Authorize(Roles=…)]` en cada endpoint. Matriz automática de 282 casos (`PermisosPorRolTests`) |
| Autorización horizontal | `PuedeVerInternoQuery`: el especialista solo accede a los internos de sus solicitudes, en la ficha, el historial, los informes clínicos y las correcciones de la ficha. Solo registra visitas de sus citas |
| Datos financieros | La cuota, el saldo y el estado de pago solo llegan al Administrador (vienen en `null` para los demás roles) |
| Contraseñas | BCrypt. Mínimo 8 caracteres, con letras y números. Cambio con la contraseña actual. Contraseña temporal asignada por el Administrador |
| Fuerza bruta | 10 intentos por minuto por IP en el login y en el cambio de contraseña (429 al pasarse) |
| Producción | `SeedController` responde 404 y Swagger queda apagado. Los secretos solo van en variables de entorno |
| Base de datos | Usuario con permisos mínimos `app_asilo` (`base-de-datos/usuario-permisos-minimos.sql`). En Azure está pendiente aplicarlo (AC-03) |

## 5. Configuración

| Clave (variable de entorno) | Para qué |
|---|---|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a SQL Server |
| `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` | Firma y validación de tokens |
| `Cors__Origenes__0` | Origen permitido del frontend, si se sirve aparte |
| `Smtp__Host`, `Smtp__Puerto`, `Smtp__Usuario`, `Smtp__Password`, `Smtp__Remitente` | Correo al familiar (AC-11) |
| `ASPNETCORE_ENVIRONMENT` | `Development` o `Production` |
| `TZ=America/Guatemala` | Hora local en el servidor Linux de Azure |
| `VITE_COMMIT` (al compilar el frontend) | Commit que se muestra en el pie (AC-15) |

La guía con valores de ejemplo, sin secretos, está en `AsiloCabezaAlgodon.API/appsettings.Production.example.json`.

## 6. Base de datos

- **Crear desde cero:** `base-de-datos/crear-base.sql` (idempotente) o `dotnet ef database update`.
- **Migración de AC-07 sobre una base existente:** `base-de-datos/migracion-ac-07-pagos-fundacion.sql`.
- **Ajuste de saldos de AC-01:** `base-de-datos/conciliar-saldos.sql`.
- **Fechas UTC antiguas de Azure (AC-16):** `base-de-datos/corregir-fechas-utc-ac-16.sql`. Primero muestra una vista previa.
- **Respaldo y restauración:** `base-de-datos/respaldo-y-restauracion.ps1`.

## 7. Despliegue

- **Primera vez:** `despliegue/desplegar-azure.ps1` (planes gratis).
- **Actualizaciones:** `despliegue/actualizar-azure.ps1`. El script:
  - descarga un respaldo del sitio;
  - avisa si hay cambios sin commit;
  - compila el frontend con el commit;
  - publica la API con `SourceRevisionId`;
  - sube el paquete.
- **Después de publicar:** comparar la versión del pie con la etiqueta y ejecutar `pruebas/probar-permisos.ps1 -Api https://<sitio>/api`.

El detalle está en `despliegue/LEEME.md`.

## 8. Pruebas

Ver `docs/plan-de-pruebas.md`.

- **Backend:** `dotnet test`. Las de integración necesitan SQL Server Express local; en GitHub Actions se excluyen con `Tipo!=Integracion`.
- **Frontend:** `npm test`, `npm run lint`, y `npx playwright test e2e/<archivo>`. Hay que correr los archivos por separado por el límite de 10 inicios de sesión por minuto.
