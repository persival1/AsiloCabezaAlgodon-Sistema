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

## Respaldo y recuperación

El script `respaldo-y-restauracion.ps1` (requiere `sqlcmd`) tiene tres acciones:

```powershell
# Respalda la base en un archivo .bak (en la carpeta de respaldos de SQL Server)
./respaldo-y-restauracion.ps1 -Accion Respaldar

# Recupera un respaldo en una base NUEVA (no sobrescribe la original)
./respaldo-y-restauracion.ps1 -Accion Restaurar -Archivo "C:\ruta\copia.bak" -BaseDestino asilo_recuperada

# Prueba completa: respalda, restaura con otro nombre, compara las filas de cada tabla y borra la copia
./respaldo-y-restauracion.ps1 -Accion Probar
```

Para restaurar **sobre una base que ya existe** hay que agregar `-Reemplazar` (borra el contenido actual de esa base). La evidencia de la última prueba está en [`pruebas/resultado-respaldo-restauracion.md`](../pruebas/resultado-respaldo-restauracion.md).

## Conciliación de saldos

El saldo de cada familiar se guarda en `Familiares.SaldoPendiente` y se actualiza con cada movimiento. Para comprobar que coincide con los movimientos reales:

- **Revisar (solo lectura):** con sesión de Administrador, `GET /api/Reportes/conciliacion-saldos`. Muestra, por familiar, el saldo recalculado (costos médicos menos cobros), el guardado y la diferencia.
- **Corregir:** respaldar primero la base y después ejecutar `conciliar-saldos.sql`. Solo cambia los saldos que no coinciden, dentro de una transacción, y al final muestra si cada uno cuadra.

```powershell
sqlcmd -S localhost\SQLEXPRESS -d asilo_cabeza_algodon -E -C -f 65001 -i conciliar-saldos.sql
```

## Cadena de conexión

Está en `AsiloCabezaAlgodon/AsiloCabezaAlgodon.API/appsettings.json`. Por defecto usa `Server=localhost\SQLEXPRESS;Database=asilo_cabeza_algodon;Trusted_Connection=True`. Si la instancia tiene otro nombre, cambiar `Server`.
