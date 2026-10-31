# Revisión de errores en Azure y del proyecto (30/10/2026)

Revisión hecha después de publicar la semana 3, a pedido del desarrollador: buscar todos los errores que registró Azure y lo que todavía pudiera estar mal en el proyecto.

## 1. Errores registrados en Azure

Se leyeron todos los registros del App Service desde la publicación (21/10). **Hay un solo tipo de error de la aplicación**, repetido 3 veces (25/10 dos veces y 31/10 una):

> `Database 'asilo_cabeza_algodon' ... is not currently available. Please retry the connection later.` (error 40613)

**Causa:** la base gratuita de Azure SQL se pausa sola cuando nadie la usa (AutoPause). La primera petición que llega mientras despierta falla, y la API respondía **500**. Es lo que se vio como «500 en la primera llamada» en cada verificación.

**Arreglo** (`Program.cs`): `EnableRetryOnFailure` en `UseSqlServer`. Entity Framework reintenta hasta 6 veces, esperando hasta 10 segundos entre intentos, antes de rendirse. La primera petición tarda más mientras la base despierta, pero ya no falla. El proyecto no usa transacciones manuales, que es lo único incompatible con este modo; quedó anotado en el código que, si se agregan (semana 4, ROB-03), hay que envolverlas en `CreateExecutionStrategy()`.

**Limitación:** el reintento no se pudo provocar en local, porque la base local no se pausa. Se comprobará en Azure la próxima vez que la base despierte.

El otro error del 31/10 (`BadImageFormatException` al arrancar) no es de la aplicación: fue el reinicio por el cambio de hora cruzado con la copia de archivos. Está explicado en `resultado-despliegue-azure.md`.

## 2. Problemas encontrados en el proyecto

### 2.1 Tres operaciones respondían 500 y aceptaban datos vacíos

Las operaciones que no habían pasado por las semanas del plan de SQA no validaban nada:

| Operación | Antes, con un interno inexistente | Antes, con texto vacío | Ahora |
|---|---|---|---|
| Crear solicitud (`POST /api/Solicitudes`) | **500** | Solicitud sin motivo | 400 «El interno indicado no existe.» / «El campo «motivo» es obligatorio.» |
| Agregar psicopatología | **500** | Psicopatología sin nombre; diagnóstico en el futuro | 400 con el motivo |
| Agregar medicina de cajón | **500** | Medicina sin nombre | 400 con el motivo |

Se comprobó contra la API local: **antes, los tres casos daban 500**; ahora dan 400 con mensaje.

Pruebas xUnit nuevas: `FichaYSolicitudCommandHandlerTests.cs` (10 pruebas). Con el código anterior **fallaban 7** (los casos inválidos); después de corregir pasan todas.

### 2.2 Pantallas que no mostraban el error

- **`FichaInterno.jsx`:** agregar una psicopatología o una medicina de cajón no tenía `catch`. Si fallaba, la pantalla no mostraba nada. Ahora muestra el motivo. La fecha de diagnóstico tiene `max` = hoy.
- **`Solicitudes.jsx`:** al crear una solicitud mostraba siempre «No se pudo crear la solicitud.»; ahora muestra el motivo del servidor.
- **`Login.jsx`:** decía «Usuario o contraseña incorrectos.» ante **cualquier** error, incluido el 500 de la base despertando. Un usuario con la contraseña correcta creía que se había equivocado. Ahora ese mensaje sale solo con un 401; otros errores muestran «No se pudo conectar con el servidor. Esperá unos segundos e intentá de nuevo.»

## 3. Resultados después de los cambios

| Prueba | Resultado |
|---|---|
| xUnit | 67 de 67 |
| `dotnet build` | 0 advertencias, 0 errores |
| Frontend: `npm test` / `npm run lint` / `npm run build` | 8 de 8 / 0 problemas / compila |
| Permisos (local) | 142 de 142 |
| Validaciones semana 2 (local) | 20 de 20, datos sin cambios |
| Semana 3 (local) | 11 de 11, la conciliación cuadra |

## 4. Revisado sin cambios

- **Login:** rechaza usuarios inactivos y usa el mismo mensaje para usuario inexistente y contraseña incorrecta (no revela qué usuarios existen).
- **Reportes:** el controlador completo exige rol Administrador (confirmado también por las 142 pruebas de permisos).
- **Secretos:** el archivo de resultados de permisos en Azure no contiene ninguna de las contraseñas de Azure (se buscó cada una).

## 5. Pendiente (no es un error, son mejoras ya planificadas)

- Semana 4: transacción en despacho, resultado y visita (ROB-03), límite de intentos del login, usuario de base con permisos mínimos y GitHub Actions.
- Correo real con Gmail (COR-01).
- `npm audit`: 6 avisos moderados solo en ESLint (desarrollo); lo publicado tiene 0.
