# Despliegue en Azure

El sistema está publicado en: **https://asilo-cabeza-algodon-2796.azurewebsites.net**

## Cómo está armado

| Pieza | Servicio de Azure | Plan |
|---|---|---|
| Base de datos | Azure SQL Database (`asilo_cabeza_algodon`) | Oferta gratis, serverless, con **AutoPause**: si se agota el uso gratis del mes, la base se pausa en lugar de cobrar |
| API y frontend | Azure App Service (Linux, .NET 10) | **F1, gratis** |

La API y el frontend están en **una sola aplicación**: la API entrega también los archivos de React (carpeta `wwwroot`). Se hizo así porque la suscripción de estudiante solo permite cinco regiones (Brasil Sur, Oeste de EE. UU., Canadá Central, Francia Central y Bélgica Central) y Static Web Apps no está disponible en ninguna de ellas. Como ventaja, el navegador habla con una sola dirección y no hacen falta llamadas entre sitios distintos.

## Medidas de seguridad aplicadas

| Medida | Por qué |
|---|---|
| Solo HTTPS (HTTP redirige con 301) y TLS 1.2 como mínimo, en la aplicación y en el servidor SQL | Todo viaja cifrado |
| FTP apagado y credenciales básicas de publicación desactivadas | Se cierran dos formas de entrar al servidor con usuario y contraseña |
| La base solo acepta conexiones desde las IP de salida de la aplicación | Nadie más puede conectarse a SQL, aunque conozca la contraseña |
| Llave del JWT nueva, generada al azar, guardada solo en la configuración de Azure | No está en el código ni en el repositorio |
| Contraseña de la base generada al azar, solo en la configuración de Azure | Ídem |
| `ASPNETCORE_ENVIRONMENT=Production` | Apaga Swagger y el SeedController (responden 404) |
| Contraseñas **nuevas** para los 6 usuarios en Azure | Las de prueba (`admin123`, `prueba123`) están publicadas en el README; en Azure **no funcionan** |
| La configuración de desarrollo no se publica | `appsettings.Development.json` (con la llave de prueba) no viaja al servidor |

Las contraseñas de Azure **no están en el repositorio**: se entregan en privado.

## Cómo se publicó

El script [`desplegar-azure.ps1`](./desplegar-azure.ps1) reúne, en orden, los comandos que se ejecutaron el 20 y 21/10/2026. Esos comandos se ejecutaron y probaron uno por uno; el script los junta para poder repetir el despliegue desde cero en otra suscripción. Usa [`copiar-datos.ps1`](./copiar-datos.ps1) para pasar los datos de la base local a la de Azure dentro de una transacción.

Después del script falta un paso manual: cambiar las contraseñas de los usuarios en la base de Azure (no se dejan las de prueba).

## Cómo actualizar la aplicación después de un cambio

El script [`actualizar-azure.ps1`](./actualizar-azure.ps1) hace todo en orden y pide confirmación antes de publicar:

1. Descarga un respaldo de lo que hay en Azure (fuera del repositorio).
2. Compila el frontend con `VITE_API_URL=/api` (`npm run build`).
3. `dotnet publish` de la API en modo Release.
4. Copia `AsiloCabezaAlgodon.Web/dist` dentro de la carpeta `wwwroot` de lo publicado y quita `appsettings.Development.json`.
5. Comprime y publica con `az webapp deploy --type zip`.

Antes: pasar las pruebas, hacer commit y push. Después: verificar el sitio y correr `pruebas/probar-permisos.ps1` contra Azure.

## Correo electrónico

Para que el correo al familiar se envíe de verdad, la aplicación necesita estos valores en **Configuración → Variables de entorno** del App Service:

| Variable | Valor |
|---|---|
| `Smtp__Host` | `smtp.gmail.com` |
| `Smtp__Puerto` | `587` |
| `Smtp__Usuario` | la cuenta de Gmail del sistema |
| `Smtp__Remitente` | la misma cuenta |
| `Smtp__Password` | la **contraseña de aplicación** de Google (16 letras). Se escribe solo en Azure, nunca en el repositorio |

## Limitaciones del plan gratis

- La aplicación **se duerme** si nadie la usa por un rato: la primera visita después puede tardar unos 20 a 30 segundos; luego va normal.
- El plan F1 tiene un límite de uso de procesador por día. Para revisión y pruebas alcanza.
- El crédito de Azure for Students vence el 19/10/2027.

## Pruebas del sistema publicado

Ver [`pruebas/resultado-despliegue-azure.md`](../pruebas/resultado-despliegue-azure.md).
