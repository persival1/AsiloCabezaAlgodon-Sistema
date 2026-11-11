# -----------------------------------------------------------------------------
# Subir correcciones al sistema que ya está publicado en Azure
#
# No crea nada nuevo: los recursos, la base y la configuración ya existen (ver
# desplegar-azure.ps1). Solo reemplaza el código de la aplicación, en este orden:
#   1) respaldo de lo que hay hoy en Azure (para poder regresar)
#   2) compila el frontend con la API en la misma dirección (/api)
#   3) compila la API en modo Release
#   4) pone el frontend dentro de wwwroot y quita la configuración de desarrollo
#   5) empaqueta en un zip
#   6) pide confirmación y lo sube con az webapp deploy
#
# Antes de usarlo: pasar las pruebas (dotnet test, npm test, npm run lint), hacer
# commit y push, y haber iniciado sesión con "az login".
# Lección del 30/10: no cambiar la configuración de Azure al mismo tiempo; primero la
# configuración, esperar a que reinicie, y después este script.
# Uso:  pwsh ./actualizar-azure.ps1
# -----------------------------------------------------------------------------

param(
    [string]$Grupo = "rg-asilo-cabeza-algodon",
    [string]$WebApp = "asilo-cabeza-algodon-2796",
    # Respaldos y paquetes quedan FUERA del repositorio
    [string]$Carpeta = "$env:USERPROFILE\AsiloAzure-respaldos"
)

$ErrorActionPreference = "Stop"
$repo = Split-Path $PSScriptRoot
$fecha = Get-Date -Format "yyyyMMdd-HHmm"
$az = (Get-Command az -ErrorAction SilentlyContinue).Source
if (-not $az) { $az = "$env:ProgramFiles\Microsoft SDKs\Azure\CLI2\wbin\az.cmd" }
New-Item -ItemType Directory -Force $Carpeta | Out-Null

# --- 1) Respaldo de lo publicado ----------------------------------------------
"=== 1) Respaldo de Azure ==="
$token = & $az account get-access-token --query accessToken -o tsv
$respaldo = Join-Path $Carpeta "respaldo-azure-$fecha.zip"
Invoke-WebRequest -Uri "https://$WebApp.scm.azurewebsites.net/api/zip/site/wwwroot/" `
    -Headers @{ Authorization = "Bearer $token" } -OutFile $respaldo
"Respaldo: $respaldo"

# --- AC-15: commit que se publica -------------------------------------------------
# Se usa el git de GitHub Desktop si git no está en el PATH. Si hay cambios sin commit, se
# avisa: lo publicado tiene que ser exactamente un commit identificado.
$git = (Get-Command git -ErrorAction SilentlyContinue).Source
if (-not $git) {
    $desktop = Get-ChildItem "$env:LOCALAPPDATA\GitHubDesktop" -Directory -Filter "app-*" | Sort-Object Name -Descending | Select-Object -First 1
    $git = Join-Path $desktop.FullName "resources\app\git\cmd\git.exe"
}
$commit = (& $git -C $repo rev-parse HEAD).Trim()
if (& $git -C $repo status --porcelain) { Write-Warning "Hay cambios sin commit: lo publicado no coincidirá con el commit $($commit.Substring(0,7))." }
"Commit que se publica: $commit"

# --- 2) Frontend -----------------------------------------------------------------
"=== 2) Frontend (npm run build) ==="
Push-Location (Join-Path $repo "AsiloCabezaAlgodon.Web")
$env:VITE_API_URL = "/api"
$env:VITE_COMMIT = $commit.Substring(0, 7)
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "El frontend no compiló." }
Pop-Location

# --- 3) Backend ------------------------------------------------------------------
"=== 3) Backend (dotnet publish) ==="
$pub = Join-Path $Carpeta "publicar-$fecha"
dotnet publish (Join-Path $repo "AsiloCabezaAlgodon\AsiloCabezaAlgodon.API") -c Release -o $pub --nologo -p:SourceRevisionId=$commit
if ($LASTEXITCODE -ne 0) { throw "La API no compiló." }

# --- 4) Juntar las dos partes ------------------------------------------------------
"=== 4) Frontend dentro de wwwroot ==="
$www = Join-Path $pub "wwwroot"
New-Item -ItemType Directory -Force $www | Out-Null
Copy-Item (Join-Path $repo "AsiloCabezaAlgodon.Web\dist\*") $www -Recurse -Force
# La llave de prueba de desarrollo nunca debe viajar al servidor
Remove-Item (Join-Path $pub "appsettings.Development.json") -ErrorAction SilentlyContinue
if (-not (Test-Path (Join-Path $www "index.html"))) { throw "Falta index.html en wwwroot." }

# --- 5) Empaquetar ---------------------------------------------------------------
"=== 5) Paquete ==="
$zip = Join-Path $Carpeta "paquete-$fecha.zip"
Compress-Archive -Path (Join-Path $pub "*") -DestinationPath $zip
"Paquete: $zip ($([math]::Round((Get-Item $zip).Length / 1MB, 1)) MB)"

# --- 6) Publicar (solo si se confirma) ---------------------------------------------
$respuesta = Read-Host "¿Publicar este paquete en https://$WebApp.azurewebsites.net? Escribí SI para continuar"
if ($respuesta -ne "SI") { "No se publicó nada. El paquete quedó listo en $zip."; return }

& $az webapp deploy -g $Grupo -n $WebApp --src-path $zip --type zip --clean true --restart true
""
"Listo. Ahora verificá: abrir el sitio, iniciar sesión, probar lo corregido y correr"
"pruebas/probar-permisos.ps1 contra Azure (142 pruebas)."
"Para regresar a la versión anterior: publicar $respaldo con el mismo comando az webapp deploy."
