# -----------------------------------------------------------------------------
# Despliegue reproducible del sistema en Azure (observación SQA #8)
#
# Reúne, en orden, los pasos que se ejecutaron y probaron el 20 y 21/10/2026 para
# publicar el sistema. Crea todo con planes GRATIS:
#   - Azure SQL Database con la oferta gratis y AutoPause (si se agota el uso gratis,
#     la base se pausa en lugar de cobrar).
#   - App Service plan F1 (Linux, gratis) que entrega la API y también el frontend.
#
# Requisitos: Azure CLI (az), sqlcmd, .NET SDK 10, Node.js, y haber iniciado sesión con
# "az login". Ejecutar desde la carpeta "despliegue" del repositorio.
#
# Seguridad: las contraseñas y la llave del JWT se generan al azar y se guardan SOLO en
# la carpeta $DirSecretos (fuera del repositorio) y en la configuración de Azure.
# -----------------------------------------------------------------------------

param(
    [string]$Region = "westus",            # la suscripción de estudiante solo permite algunas regiones
    [string]$Grupo = "rg-asilo-cabeza-algodon",
    [string]$DirSecretos = "$env:USERPROFILE\AsiloAzure-secretos",
    [string]$BaseLocal = "Server=localhost\SQLEXPRESS;Database=asilo_cabeza_algodon;Trusted_Connection=True;TrustServerCertificate=True;"
)

$ErrorActionPreference = "Stop"
$repo = Split-Path $PSScriptRoot

function Nueva-Clave([int]$n) {
    $chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!#%*-_".ToCharArray()
    $bytes = New-Object byte[] $n
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return "Aa9!" + (-join ($bytes | ForEach-Object { $chars[$_ % $chars.Length] }))
}

# --- 0) Nombres y secretos (fuera del repositorio) ---------------------------
New-Item -ItemType Directory -Path $DirSecretos -Force | Out-Null
$sufijo = Get-Random -Minimum 1000 -Maximum 9999
$cfg = [ordered]@{
    grupo = $Grupo; region = $Region
    servidorSql = "sql-asilo-$sufijo"; baseDatos = "asilo_cabeza_algodon"
    adminSql = "asiloadmin"; claveSql = (Nueva-Clave 24)
    plan = "plan-asilo"; webApp = "asilo-cabeza-algodon-$sufijo"
    claveJwt = (Nueva-Clave 60)
}
$cfg | ConvertTo-Json | Set-Content (Join-Path $DirSecretos "config-azure.json") -Encoding UTF8
$url = "https://$($cfg.webApp).azurewebsites.net"
$servidor = "$($cfg.servidorSql).database.windows.net"

# --- 1) Grupo de recursos y base de datos ------------------------------------
az group create --name $cfg.grupo --location $cfg.region --output none
az sql server create --name $cfg.servidorSql --resource-group $cfg.grupo --location $cfg.region `
    --admin-user $cfg.adminSql --admin-password $cfg.claveSql --minimal-tls-version 1.2 --output none
az sql db create --resource-group $cfg.grupo --server $cfg.servidorSql --name $cfg.baseDatos `
    --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless `
    --use-free-limit --free-limit-exhaustion-behavior AutoPause --backup-storage-redundancy Local --output none

# --- 2) Firewall temporal para cargar la base desde este equipo ---------------
# Azure rechaza la primera conexión e informa la IP pública; se abre solo esa IP.
$msg = sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 30 -Q "SELECT 1" 2>&1 | Out-String
$miIp = [regex]::Match($msg, "IP address '([0-9\.]+)'").Groups[1].Value
az sql server firewall-rule create --resource-group $cfg.grupo --server $cfg.servidorSql `
    --name temporal-carga-datos --start-ip-address $miIp --end-ip-address $miIp --output none
Start-Sleep -Seconds 30

# --- 3) Tablas y datos --------------------------------------------------------
sqlcmd -S $servidor -d $cfg.baseDatos -U $cfg.adminSql -P $cfg.claveSql -C -l 60 -b -f 65001 `
    -i (Join-Path $repo "base-de-datos\crear-base.sql")
# Datos: se copian de la base local en una sola transacción (ver copiar-datos.ps1)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "copiar-datos.ps1") `
    -Config (Join-Path $DirSecretos "config-azure.json") -Origen $BaseLocal

# --- 4) App Service gratis, solo HTTPS, sin FTP ni credenciales básicas -------
az appservice plan create --name $cfg.plan --resource-group $cfg.grupo --location $cfg.region --is-linux --sku F1 --output none
az webapp create --name $cfg.webApp --resource-group $cfg.grupo --plan $cfg.plan --runtime "DOTNETCORE:10.0" --https-only true --output none
az webapp config set --name $cfg.webApp --resource-group $cfg.grupo --ftps-state Disabled --min-tls-version 1.2 --output none
foreach ($n in "ftp", "scm") {
    az resource update --resource-group $cfg.grupo --namespace Microsoft.Web --resource-type basicPublishingCredentialsPolicies `
        --parent "sites/$($cfg.webApp)" --name $n --set properties.allow=false --output none
}

# --- 5) Configuración de producción (los secretos quedan solo en Azure) -------
$conexion = "Server=tcp:$servidor,1433;Database=$($cfg.baseDatos);User ID=$($cfg.adminSql);Password=$($cfg.claveSql);Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"
az webapp config appsettings set --name $cfg.webApp --resource-group $cfg.grupo --output none --settings `
    "ASPNETCORE_ENVIRONMENT=Production" "Jwt__Key=$($cfg.claveJwt)" "Cors__Origenes__0=$url"
az webapp config connection-string set --name $cfg.webApp --resource-group $cfg.grupo --connection-string-type SQLAzure `
    --settings "DefaultConnection=$conexion" --output none

# --- 6) Firewall de la base: solo las IP de salida de la aplicación -----------
$ips = (az webapp show --name $cfg.webApp --resource-group $cfg.grupo --query possibleOutboundIpAddresses --output tsv).Split(",") | Sort-Object -Unique
$i = 0
foreach ($ip in $ips) {
    $i++
    az sql server firewall-rule create --resource-group $cfg.grupo --server $cfg.servidorSql `
        --name ("app-salida-{0:D2}" -f $i) --start-ip-address $ip --end-ip-address $ip --output none
}

# --- 7) Compilar y publicar (frontend dentro de wwwroot de la API) ------------
$pub = Join-Path $env:TEMP ("asilo-publicar-" + (Get-Date -Format "yyyyMMddHHmmss"))
Push-Location (Join-Path $repo "AsiloCabezaAlgodon.Web"); $env:VITE_API_URL = "/api"; npm ci; npm run build; Pop-Location
dotnet publish (Join-Path $repo "AsiloCabezaAlgodon\AsiloCabezaAlgodon.API") -c Release -o (Join-Path $pub "app")
New-Item -ItemType Directory -Path (Join-Path $pub "app\wwwroot") | Out-Null
Copy-Item (Join-Path $repo "AsiloCabezaAlgodon.Web\dist\*") (Join-Path $pub "app\wwwroot") -Recurse
Compress-Archive -Path (Join-Path $pub "app\*") -DestinationPath (Join-Path $pub "asilo.zip")
az webapp deploy --name $cfg.webApp --resource-group $cfg.grupo --src-path (Join-Path $pub "asilo.zip") --type zip --restart true --output none

# --- 8) Cerrar el acceso temporal a la base -----------------------------------
az sql server firewall-rule delete --resource-group $cfg.grupo --server $cfg.servidorSql --name temporal-carga-datos

"Sistema publicado en: $url"
"Pendiente manual: cambiar las contraseñas de los usuarios (ver LEEME.md) y configurar el correo (Smtp__*)."
