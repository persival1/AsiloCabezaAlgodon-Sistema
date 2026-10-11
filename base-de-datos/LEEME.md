# Base de datos

El motor es **SQL Server**. El archivo `crear-base.sql` crea todas las tablas del sistema (13 tablas más la de control de migraciones). Se generó a partir de las migraciones de Entity Framework y se puede ejecutar varias veces sin duplicar nada.

## Opción A: con este script (no necesita herramientas de .NET)

1. Abrir SQL Server Management Studio (o Azure Data Studio) y conectarse a la instancia, por ejemplo `localhost\SQLEXPRESS`.
2. Crear la base vacía:

```sql
CREATE DATABASE asilo_cabeza_algodon;
```

3. Seleccionar esa base y ejecutar el contenido de `crear-base.sql`.
4. Crear los usuarios de prueba con la API corriendo: `POST /api/Seed/admin` y después `POST /api/Seed/usuarios-prueba` (desde `https://localhost:7022/swagger`).

## Opción B: con las migraciones de .NET

Desde la carpeta `AsiloCabezaAlgodon` del repositorio:

```bash
dotnet ef database update --project AsiloCabezaAlgodon.Infrastructure --startup-project AsiloCabezaAlgodon.API
```

Esto crea la base `asilo_cabeza_algodon` y las tablas automáticamente. Requiere la herramienta `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

## Cadena de conexión

Está en `AsiloCabezaAlgodon/AsiloCabezaAlgodon.API/appsettings.json`. Por defecto usa `Server=localhost\SQLEXPRESS;Database=asilo_cabeza_algodon;Trusted_Connection=True`. Si la instancia tiene otro nombre, cambiar `Server`.
