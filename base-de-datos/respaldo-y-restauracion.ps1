# -----------------------------------------------------------------------------
# Respaldo y recuperacion de la base de datos (respuesta a la observacion SQA #7)
#
# Acciones:
#   Respaldar  -> crea un archivo .bak con una copia completa de la base.
#   Restaurar  -> recupera un .bak en una base (por defecto con OTRO nombre).
#   Probar     -> respalda la base real, la restaura con otro nombre, compara cuantas
#                 filas tiene cada tabla en la copia y en la original, y borra la copia
#                 temporal. Sirve para DEMOSTRAR que el respaldo se puede recuperar,
#                 sin tocar la base original.
#
# Ejemplos:
#   ./respaldo-y-restauracion.ps1 -Accion Probar
#   ./respaldo-y-restauracion.ps1 -Accion Respaldar
#   ./respaldo-y-restauracion.ps1 -Accion Restaurar -Archivo "C:\ruta\copia.bak" -BaseDestino asilo_recuperada
#
# Requisitos: sqlcmd (herramienta de SQL Server) y permiso para respaldar la base.
# Nota: el archivo .bak se guarda en una carpeta que debe poder escribir el SERVICIO de
# SQL Server (por defecto, la carpeta de respaldos de la instancia).
# -----------------------------------------------------------------------------

param(
    [ValidateSet("Respaldar", "Restaurar", "Probar")]
    [string]$Accion = "Probar",
    [string]$Instancia = "localhost\SQLEXPRESS",
    [string]$Base = "asilo_cabeza_algodon",
    [string]$Archivo = "",
    [string]$BaseDestino = "",
    [switch]$Reemplazar    # permite restaurar SOBRE una base que ya existe (borra su contenido)
)

$ErrorActionPreference = "Stop"

# Ejecuta una consulta con sqlcmd (autenticacion de Windows) y devuelve las lineas de salida.
function Invocar-Sql([string]$consulta, [string]$bd = "master") {
    $salida = & sqlcmd -S $Instancia -d $bd -E -b -h -1 -W -s "|" -C -Q $consulta 2>&1
    if ($LASTEXITCODE -ne 0) { throw ("Error de SQL Server: " + ($salida -join " ")) }
    # La coma delante evita que PowerShell convierta una lista de 1 elemento en un texto suelto.
    return , @($salida | Where-Object { $_ -and $_.ToString().Trim() -ne "" })
}

function Existe-Base([string]$nombre) {
    $r = Invocar-Sql "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'$nombre'"
    return ([int]$r[0].Trim() -gt 0)
}

function Ruta-Respaldos { return (Invocar-Sql "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))")[0].Trim() }
function Ruta-Datos     { return (Invocar-Sql "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(400))")[0].Trim() }

function Respaldar-Base {
    $marca = Get-Date -Format "yyyyMMdd_HHmmss"
    $destino = if ($Archivo) { $Archivo } else { Join-Path (Ruta-Respaldos) "${Base}_$marca.bak" }
    Write-Host "Respaldando '$Base' en: $destino"
    # CHECKSUM verifica la integridad de las paginas al respaldar; INIT sobrescribe el archivo.
    Invocar-Sql "BACKUP DATABASE [$Base] TO DISK = N'$destino' WITH INIT, CHECKSUM, NAME = N'Respaldo de $Base'" | Out-Null
    # Verifica que el archivo generado se pueda leer.
    Invocar-Sql "RESTORE VERIFYONLY FROM DISK = N'$destino' WITH CHECKSUM" | Out-Null
    Write-Host "Respaldo creado y verificado."
    return $destino
}

function Restaurar-Base([string]$origen, [string]$nombreDestino) {
    if (-not $nombreDestino) { throw "Indica -BaseDestino (por seguridad no se restaura sobre la base original por defecto)." }
    if ((Existe-Base $nombreDestino) -and -not $Reemplazar) {
        throw "La base '$nombreDestino' ya existe. Usa otro nombre o agrega -Reemplazar si quieres sobrescribirla."
    }
    # Nombres logicos de los archivos dentro del .bak (datos y log)
    $lista = Invocar-Sql "RESTORE FILELISTONLY FROM DISK = N'$origen'"
    $mover = @()
    $datos = Ruta-Datos
    foreach ($linea in $lista) {
        # sqlcmd agrega al final una linea como "(2 rows affected)": se ignora (no tiene "|")
        if (-not $linea.Contains("|")) { continue }
        $campos = $linea.Split("|")
        $logico = $campos[0].Trim(); $tipo = $campos[2].Trim()
        $extension = if ($tipo -eq "L") { "ldf" } else { "mdf" }
        $mover += "MOVE N'$logico' TO N'$datos${nombreDestino}_$logico.$extension'"
    }
    $opciones = ($mover -join ", ")
    if ($Reemplazar) { $opciones += ", REPLACE" }
    Write-Host "Restaurando en la base '$nombreDestino'..."
    Invocar-Sql "RESTORE DATABASE [$nombreDestino] FROM DISK = N'$origen' WITH $opciones, RECOVERY" | Out-Null
    Write-Host "Restauracion terminada."
}

# Cuenta las filas de cada tabla de la base (para comparar original contra copia).
function Conteo-Tablas([string]$bd) {
    $consulta = "SET NOCOUNT ON; SELECT t.name, SUM(p.rows) FROM sys.tables t JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1) GROUP BY t.name ORDER BY t.name"
    $mapa = [ordered]@{}
    foreach ($l in (Invocar-Sql $consulta $bd)) {
        if (-not $l.Contains("|")) { continue }
        $c = $l.Split("|"); $mapa[$c[0].Trim()] = [long]$c[1].Trim()
    }
    return $mapa
}

switch ($Accion) {
    "Respaldar" { Respaldar-Base | Out-Null }
    "Restaurar" {
        if (-not $Archivo) { throw "Indica -Archivo con la ruta del .bak." }
        Restaurar-Base $Archivo $BaseDestino
    }
    "Probar" {
        $temporal = "${Base}_prueba_restauracion"
        if (Existe-Base $temporal) { throw "Ya existe '$temporal'. Borrala antes de ejecutar la prueba." }
        $bak = Respaldar-Base
        try {
            Restaurar-Base $bak $temporal
            $original = Conteo-Tablas $Base
            $copia    = Conteo-Tablas $temporal
            $diferencias = 0
            Write-Host ""
            Write-Host ("{0,-24} {1,10} {2,10}  {3}" -f "Tabla", "Original", "Copia", "Resultado")
            foreach ($t in $original.Keys) {
                $ok = ($copia.Contains($t) -and $copia[$t] -eq $original[$t])
                if (-not $ok) { $diferencias++ }
                Write-Host ("{0,-24} {1,10} {2,10}  {3}" -f $t, $original[$t], $copia[$t], $(if ($ok) { "OK" } else { "DIFERENTE" }))
            }
            Write-Host ""
            if ($diferencias -eq 0) { Write-Host "RECUPERACION VERIFICADA: las $($original.Count) tablas tienen las mismas filas en la copia restaurada." -ForegroundColor Green }
            else { Write-Host "ATENCION: $diferencias tabla(s) con diferencias." -ForegroundColor Red }
        }
        finally {
            # Limpieza: la base temporal siempre se elimina; el .bak queda como evidencia.
            if (Existe-Base $temporal) {
                Invocar-Sql "ALTER DATABASE [$temporal] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$temporal]" | Out-Null
                Write-Host "Base temporal '$temporal' eliminada. La base original no se toco."
            }
            Write-Host "Archivo de respaldo conservado: $bak"
        }
        if ($diferencias -gt 0) { exit 1 }
    }
}
