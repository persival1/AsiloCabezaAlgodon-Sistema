# -----------------------------------------------------------------------------
# Pruebas de la semana 4 del plan de SQA: datos inesperados (ROB-05) y límite de intentos
# de inicio de sesión
#
# Qué hace:
#   0. ROB-05: envía a POST /api/Cobros y /api/Internos JSON mal escrito, texto en un campo
#      numérico o de fecha y cuerpo vacío. Debe responder 400 con { mensaje } en español, sin
#      nombres de clases ni detalles internos. Son datos inválidos: no se guarda nada.
#   1. Espera un minuto para empezar con el contador en cero.
#   2. Envía 11 intentos de login con un usuario que no existe:
#      los intentos 1 a 10 deben dar 401 y el 11 debe dar 429 con un mensaje claro.
#   3. Durante el bloqueo, un login con la contraseña CORRECTA también da 429
#      (el límite es por dirección IP, no por usuario).
#   4. Espera un minuto: el login correcto vuelve a funcionar (200).
#
# No crea datos. Tarda unos dos minutos por las esperas. Al terminar, el contador queda en
# cero, así que se puede correr otro script enseguida.
# La transacción de la semana 4 (ROB-03) se prueba aparte, con dotnet test (pruebas de integración).
#
# Requisitos: la API corriendo y el usuario admin. Para otro servidor, pasar -Api y -ClaveAdmin.
# Uso:  pwsh ./probar-semana-4.ps1
# -----------------------------------------------------------------------------

param(
    [string]$Api = "https://localhost:7022/api",
    [string]$ClaveAdmin = "admin123",
    [string]$Salida = "$PSScriptRoot/resultados-semana-4.md"
)

function Login($usuario, $clave) {
    $cuerpo = @{ nombreUsuario = $usuario; password = $clave } | ConvertTo-Json
    $r = Invoke-WebRequest -Method Post -Uri "$Api/Auth/login" -ContentType "application/json" -Body $cuerpo -SkipCertificateCheck -SkipHttpErrorCheck
    $datos = $null
    try { $datos = $r.Content | ConvertFrom-Json } catch { }
    return @{ Codigo = [int]$r.StatusCode; Mensaje = $datos.mensaje }
}

$filas = @()
$ok = 0; $mal = 0
function Revisar($caso, $esperado, $obtenido, $detalle = "") {
    $bien = $esperado -eq $obtenido
    if ($bien) { $script:ok++ } else { $script:mal++ }
    $script:filas += "| $caso | $esperado | $obtenido | $(if ($bien) { 'OK' } else { 'FALLA' }) | $detalle |"
    "$(if ($bien) { 'OK   ' } else { 'FALLA' }) $caso (esperado $esperado, obtenido $obtenido) $detalle"
}

# --- 0) ROB-05: datos inesperados ------------------------------------------------
$cuerpo = @{ nombreUsuario = "admin"; password = $ClaveAdmin } | ConvertTo-Json
$token = (Invoke-RestMethod -Method Post -Uri "$Api/Auth/login" -ContentType "application/json" -Body $cuerpo -SkipCertificateCheck).token
$casos = @(
    @("Cobros: JSON mal escrito", "/Cobros", '{"idFamiliar": 1, "concepto": '),
    @("Cobros: texto en el monto", "/Cobros", '{"idFamiliar": 1, "concepto": "Cuota", "monto": "abc"}'),
    @("Cobros: cuerpo vacío", "/Cobros", ''),
    @("Internos: JSON mal escrito", "/Internos", '{"nombres": "Ana", '),
    @("Internos: texto en la fecha", "/Internos", '{"nombres": "Ana", "fechaNacimiento": "ayer"}'),
    @("Internos: cuerpo vacío", "/Internos", '')
)
foreach ($c in $casos) {
    $r = Invoke-WebRequest -Method Post -Uri "$Api$($c[1])" -Headers @{ Authorization = "Bearer $token" } -ContentType "application/json" -Body $c[2] -SkipCertificateCheck -SkipHttpErrorCheck
    $texto = if ($r.Content -is [byte[]]) { [Text.Encoding]::UTF8.GetString($r.Content) } else { $r.Content }
    $mensaje = try { ($texto | ConvertFrom-Json).mensaje } catch { $null }
    # Detalle interno = cualquier nombre del código, excepción, ruta JSON o dato de conexión
    $interno = $texto -match "AsiloCabezaAlgodon|Exception|StackTrace|Server=|BytePosition|\$\."
    $estado = if ($mensaje -and -not $interno) { [int]$r.StatusCode } else { "$([int]$r.StatusCode) sin mensaje claro" }
    Revisar "ROB-05 $($c[0])" 400 $estado $mensaje
}

"Esperando 60 s para empezar con el contador en cero..."
Start-Sleep -Seconds 61

for ($i = 1; $i -le 10; $i++) {
    $r = Login "usuario-que-no-existe" "clave-falsa"
    Revisar "Intento $i con usuario inexistente" 401 $r.Codigo
}
$r = Login "usuario-que-no-existe" "clave-falsa"
Revisar "Intento 11 (pasó el límite)" 429 $r.Codigo $r.Mensaje

$r = Login "admin" $ClaveAdmin
Revisar "Login correcto durante el bloqueo" 429 $r.Codigo "el límite es por IP"

"Esperando 60 s a que se libere el límite..."
Start-Sleep -Seconds 61
$r = Login "admin" $ClaveAdmin
Revisar "Login correcto después de un minuto" 200 $r.Codigo

""
"Total: $($ok + $mal) | Correctas: $ok | Fallidas: $mal"

$md = @(
    "# Resultados: semana 4, datos inesperados (ROB-05) y límite de intentos de inicio de sesión",
    "",
    "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm') · API: $Api",
    "",
    "| Caso | Esperado | Obtenido | Resultado | Detalle |",
    "|---|---|---|---|---|"
) + $filas + @("", "**Total: $($ok + $mal) · Correctas: $ok · Fallidas: $mal**")
$md | Set-Content -Path $Salida -Encoding utf8
"Resultados en $Salida"
