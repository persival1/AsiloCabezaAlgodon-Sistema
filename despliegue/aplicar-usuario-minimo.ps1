# -----------------------------------------------------------------------------
# Usuario de base de datos con permisos mínimos (plan de pruebas SQA, semana 4)
#
# Hoy la aplicación en Azure se conecta con el ADMINISTRADOR del servidor SQL. Este script:
#   1) abre el firewall de SQL solo para la IP de este equipo (temporal)
#   2) genera una clave al azar para app_asilo y la guarda SOLO en la carpeta de secretos
#   3) crea el usuario app_asilo con db_datareader y db_datawriter (leer y escribir datos)
#   4) comprueba como app_asilo que leer funciona y que crear tablas NO (si algo no cuadra, se detiene)
#   5) pide confirmación y cambia la cadena de conexión del App Service (la anterior se guarda
#      en la carpeta de secretos para poder regresar). La aplicación se reinicia sola.
#   6) borra la regla temporal del firewall y la vieja "temporal-carga-datos" del 21/10
# Nunca muestra claves. Se puede volver a correr: reutiliza la clave guardada.
#
# Después: abrir el sitio, iniciar sesión y correr pruebas/probar-permisos.ps1 contra Azure.
# Para regresar: az webapp config connection-string set ... con la cadena guardada en
# cadena-conexion-anterior-admin.txt.
# Requisitos: Azure CLI con "az login" y sqlcmd.  Uso:  pwsh ./aplicar-usuario-minimo.ps1
# -----------------------------------------------------------------------------

param(
    [string]$DirSecretos = "$env:USERPROFILE\AsiloAzure-secretos"
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false
$az = (Get-Command az -ErrorAction SilentlyContinue).Source
if (-not $az) { $az = Join-Path $env:ProgramFiles "Microsoft SDKs\Azure\CLI2\wbin\az.cmd" }
$archivoCfg = Join-Path $DirSecretos "config-azure.json"
$cfg = Get-Content $archivoCfg -Raw | ConvertFrom-Json
$servidor = "$($cfg.servidorSql).database.windows.net"
$regla = "temporal-usuario-app"

function Oculto([string]$texto) {
    foreach ($s in @($cfg.claveSql, $cfg.claveApp, $script:clave)) { if ($s) { $texto = $texto.Replace($s, "(oculta)") } }
    return $texto.Trim()
}

function CerrarFirewall {
    $existentes = & $az sql server firewall-rule list -g $cfg.grupo -s $cfg.servidorSql --query "[].name" -o tsv
    foreach ($r in @($regla, "temporal-carga-datos")) {
        if ($existentes -contains $r) {
            & $az sql server firewall-rule delete -g $cfg.grupo -s $cfg.servidorSql -n $r -o none
            "Regla del firewall borrada: $r"
        }
    }
}

# --- 1) Firewall solo para la IP de este equipo (Azure la informa al rechazar la conexión) ---
$msg = sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 30 -Q "SELECT 1" 2>&1 | Out-String
$ip = [regex]::Match($msg, "IP address '([0-9\.]+)'").Groups[1].Value
if ($ip) {
    & $az sql server firewall-rule create -g $cfg.grupo -s $cfg.servidorSql -n $regla --start-ip-address $ip --end-ip-address $ip -o none
    "1) Regla temporal del firewall creada para la IP de este equipo"
    $conectado = $false
    for ($i = 0; $i -lt 12 -and -not $conectado; $i++) {
        Start-Sleep -Seconds 10
        $prueba = sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 30 -h -1 -Q "SET NOCOUNT ON; SELECT 'ok'" 2>&1 | Out-String
        $conectado = $prueba -match "ok"
    }
    if (-not $conectado) { "1) FALLA: no se pudo conectar: " + (Oculto $prueba); CerrarFirewall; exit 1 }
} else {
    "1) No hizo falta abrir el firewall"
}

# --- 2) Clave de app_asilo, guardada solo en la carpeta de secretos ---
if ($cfg.claveApp) {
    $clave = $cfg.claveApp
    "2) Se reutiliza la clave de app_asilo ya guardada"
} else {
    Copy-Item $archivoCfg (Join-Path $DirSecretos "config-azure.antes-de-app_asilo.json")
    $chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789-_".ToCharArray()
    $bytes = New-Object byte[] 28
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    $clave = "Aa9-" + (-join ($bytes | ForEach-Object { $chars[$_ % $chars.Length] }))
    $cfg | Add-Member -NotePropertyName usuarioApp -NotePropertyValue "app_asilo" -Force
    $cfg | Add-Member -NotePropertyName claveApp -NotePropertyValue $clave -Force
    $cfg | ConvertTo-Json | Set-Content $archivoCfg -Encoding UTF8
    "2) Clave nueva de app_asilo guardada en la carpeta de secretos"
}

# --- 3) Crear o actualizar el usuario con solo lectura y escritura de datos ---
$archivoSql = Join-Path ([IO.Path]::GetTempPath()) "usuario-minimo-$([guid]::NewGuid()).sql"
@'
SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'app_asilo')
    CREATE USER app_asilo WITH PASSWORD = '$(clave)';
ELSE
    ALTER USER app_asilo WITH PASSWORD = '$(clave)';
ALTER ROLE db_datareader ADD MEMBER app_asilo;
ALTER ROLE db_datawriter ADD MEMBER app_asilo;
SELECT r.name FROM sys.database_role_members m
    JOIN sys.database_principals r ON r.principal_id = m.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = m.member_principal_id
WHERE u.name = 'app_asilo' ORDER BY r.name;
'@ | Set-Content $archivoSql -Encoding UTF8
$roles = sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 60 -b -h -1 -v "clave=$clave" -i $archivoSql 2>&1 | Out-String
Remove-Item $archivoSql -Confirm:$false
"3) Roles de app_asilo: " + ((Oculto $roles) -replace '\s+', ' ')
if ($roles -notmatch "db_datareader" -or $roles -notmatch "db_datawriter") { "3) FALLA: el usuario no quedó con los roles esperados"; CerrarFirewall; exit 1 }

# --- 4) Comprobar como app_asilo: leer funciona; crear tablas NO ---
$lee = sqlcmd -S $servidor -d $cfg.baseDatos -U app_asilo -P $clave -C -l 30 -h -1 -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM Internos" 2>&1 | Out-String
$crea = sqlcmd -S $servidor -d $cfg.baseDatos -U app_asilo -P $clave -C -l 30 -b -Q "CREATE TABLE prueba_permisos_app (x INT)" 2>&1 | Out-String
$puedeLeer = $lee.Trim() -match '^\d+$'
$noPuedeCrear = $crea -match "permission denied|permiso"
"4) Como app_asilo, leer internos: " + $(if ($puedeLeer) { "funciona ($($lee.Trim()) internos)" } else { "NO funciona: " + (Oculto $lee) })
"4) Como app_asilo, crear una tabla: " + $(if ($noPuedeCrear) { "rechazado (correcto)" } else { "NO fue rechazado: " + (Oculto $crea) })
if (-not $noPuedeCrear) {
    sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 30 -Q "IF OBJECT_ID('prueba_permisos_app') IS NOT NULL DROP TABLE prueba_permisos_app" | Out-Null
}
if (-not $puedeLeer -or -not $noPuedeCrear) { "4) FALLA: no se cambia la cadena de conexión"; CerrarFirewall; exit 1 }

# --- 5) Cambiar la cadena de conexión (con confirmación; guarda la anterior) ---
$respuesta = Read-Host "¿Cambiar la conexión de https://$($cfg.webApp).azurewebsites.net a app_asilo? La aplicación se reinicia. Escribí SI para continuar"
if ($respuesta -ne "SI") {
    "5) No se cambió la conexión. El usuario app_asilo quedó creado y probado."
} else {
    $entrada = (& $az webapp config connection-string list -g $cfg.grupo -n $cfg.webApp -o json | ConvertFrom-Json) | Where-Object { $_.name -eq "DefaultConnection" }
    $anterior = $entrada.value
    $respaldo = Join-Path $DirSecretos "cadena-conexion-anterior-admin.txt"
    if (-not (Test-Path $respaldo)) { Set-Content $respaldo $anterior -Encoding UTF8 }
    $b = New-Object System.Data.Common.DbConnectionStringBuilder
    $b.set_ConnectionString($anterior)
    $b["user id"] = "app_asilo"
    $b["password"] = $clave
    & $az webapp config connection-string set -g $cfg.grupo -n $cfg.webApp -t SQLAzure --settings "DefaultConnection=$($b.ConnectionString)" -o none
    "5) La aplicación ahora se conecta como app_asilo (se reinicia sola). La cadena anterior quedó en la carpeta de secretos"
}

# --- 6) Cerrar el firewall ---
"6) Cerrando el firewall..."
CerrarFirewall
""
"Listo. Esperá un minuto a que reinicie y verificá: abrir el sitio, iniciar sesión y correr"
"pruebas/probar-permisos.ps1 contra Azure (142 pruebas)."
