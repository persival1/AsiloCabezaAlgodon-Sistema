# Prueba de instalación desde una carpeta nueva (AC-14, documento 8)

- **Fecha:** 10/11/2026
- **Versión:** 1.0.0-rc2. Se usó un clon nuevo de la rama `ac-14-documentacion`; el pie mostró el commit `d2dc050`, que solo agrega documentos sobre el código publicado `675011f`.
- **Equipo:** el del desarrollador (Windows 11, .NET SDK 10, Node.js 24, SQL Server Express).
- **Para no tocar la base de desarrollo**, se usó una base aparte (`asilo_prueba_instalacion`) con la variable `ConnectionStrings__DefaultConnection`. Es la única diferencia con el README.

**Límite de esta prueba:** no es un «equipo limpio», porque esta computadora ya tenía instaladas algunas herramientas. Esas herramientas son justo los hallazgos 1 y 2 de abajo. La prueba en otra computadora la hace el desarrollador; su resultado se agrega al final de este archivo.

## Pasos del README y resultado

| N.º | Paso | Comando | Resultado | Tiempo |
|---|---|---|---|---|
| 1 | Clonar | `git clone …` | Correcto | 1 s |
| 2 | Restaurar el backend | `dotnet restore` | Correcto | 4 s |
| 3 | Crear la base | `dotnet ef database update --project AsiloCabezaAlgodon.Infrastructure --startup-project AsiloCabezaAlgodon.API` | Correcto: 8 migraciones aplicadas | 15 s |
| 4 | Arrancar la API | `dotnet run --launch-profile https` | Correcto: Swagger respondió 200 | 6 s |
| 5 | Usuarios de prueba | `POST /api/Seed/admin` y `POST /api/Seed/usuarios-prueba` | «Usuario admin creado» y «Usuarios de prueba creados/actualizados» | — |
| 6 | Frontend | `npm install` y `npm run dev` | Correcto: `http://localhost:5173` respondió 200 | 8 s |
| 7 | Entrar al sistema | admin y especialista desde el navegador (Edge) | Los dos entran. Cada uno ve su menú, y el pie muestra versión y commit | — |

## Hallazgos (ya corregidos en el README)

| N.º | Hallazgo | Corrección |
|---|---|---|
| 1 | El README no decía que hay que instalar la herramienta `dotnet-ef`. Aquí ya estaba instalada (versión 10.0.11); en un equipo nuevo el paso 3 fallaría porque el comando `dotnet ef` no existe | Se agregó `dotnet tool install --global dotnet-ef` |
| 2 | El README no decía que hay que confiar en el certificado HTTPS de desarrollo. Aquí ya era de confianza; en un equipo nuevo el navegador no confiaría en `https://localhost:7022` y el inicio de sesión fallaría | Se agregó `dotnet dev-certs https --trust` |
| 3 | Decía «Node.js 20 o superior», pero Vite 8 pide 20.19 o superior (o 22.12 o superior) | Se puso la versión exacta |

## Prueba en otra computadora

*Pendiente: la hace el desarrollador en un equipo sin nada del proyecto, siguiendo el README tal cual. Anotar cada paso: si funcionó, qué error salió y cuánto tardó.*
