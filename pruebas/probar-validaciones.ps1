# -----------------------------------------------------------------------------
# Pruebas de validación en el servidor (plan de pruebas SQA, semana 2)
#
# Qué hace: con la cuenta de Administrador envía a la API datos que DEBEN ser rechazados
# (montos negativos, campos vacíos, fechas imposibles, despachar dos veces...) y compara el
# código HTTP obtenido con el esperado:
#   400 = datos inválidos    404 = no existe    409 = choca con el estado actual
# Al final comprueba que NADA cambió: mismo saldo del familiar, misma cantidad de cobros,
# donaciones, gastos e internos, y que la conciliación de saldos sigue cuadrando.
#
# Ejecutarlo SOLO en el entorno local o de pruebas: si alguna validación fallara, el dato
# inválido sí se guardaría.
#
# Requisitos: la API corriendo, los usuarios de prueba creados y al menos un interno con una
# prescripción despachada y un examen con resultado (los datos de ejemplo del proyecto).
# Uso:  pwsh ./probar-validaciones.ps1
# -----------------------------------------------------------------------------

param(
    [string]$Api = "https://localhost:7022/api",
    [string]$Salida = "$PSScriptRoot/resultados-validaciones.md",
    [string]$Usuario = "admin",
    [string]$Clave = "admin123"   # contraseña de prueba que crea /api/Seed en el entorno local
)

$cuerpoLogin = @{ nombreUsuario = $Usuario; password = $Clave } | ConvertTo-Json
$token = (Invoke-RestMethod -Method Post -Uri "$Api/Auth/login" -ContentType "application/json" -Body $cuerpoLogin -SkipCertificateCheck).token
$cabeceras = @{ Authorization = "Bearer $token" }

function Leer($ruta) {
    # Invoke-RestMethod devuelve la lista JSON como un solo objeto; ForEach-Object la separa
    # en sus elementos para poder filtrarlos y contarlos.
    Invoke-RestMethod -Uri "$Api$ruta" -Headers $cabeceras -SkipCertificateCheck | ForEach-Object { $_ }
}

# Envía la petición y devuelve el código HTTP y el mensaje del servidor (si lo hay)
function Enviar($metodo, $ruta, $cuerpo) {
    try {
        $r = Invoke-WebRequest -Method $metodo -Uri "$Api$ruta" -Headers $cabeceras -ContentType "application/json" `
            -Body $cuerpo -SkipCertificateCheck -SkipHttpErrorCheck
        $mensaje = ""
        try { $mensaje = ($r.Content | ConvertFrom-Json).mensaje } catch { }
        return @{ Codigo = [int]$r.StatusCode; Mensaje = $mensaje }
    } catch {
        return @{ Codigo = -1; Mensaje = $_.Exception.Message }
    }
}

function Foto() {
    $internos = Leer "/Internos"
    $conciliacion = Leer "/Reportes/conciliacion-saldos"
    return [ordered]@{
        Internos   = @($internos).Count
        Cobros     = @(Leer "/Cobros").Count
        Donaciones = @(Leer "/Donaciones").Count
        Gastos     = @(Leer "/Gastos").Count
        Saldos     = (@($conciliacion) | ForEach-Object { "$($_.idFamiliar)=$($_.saldoGuardado)" }) -join ", "
        Cuadra     = -not (@($conciliacion) | Where-Object { -not $_.cuadra })
    }
}

# Datos de referencia
$interno = @(Leer "/Internos")[0]
$idFamiliar = $interno.idFamiliar
$despachada = @(Leer "/Prescripciones") | Where-Object { $_.estado -eq "Despachado" } | Select-Object -First 1
$conResultado = @(Leer "/Examenes") | Where-Object { $_.estado -eq "ResultadoRegistrado" } | Select-Object -First 1

function Cobro($monto, $concepto = "Cuota mensual", $visita = $null) {
    return "{""idFamiliar"":$idFamiliar,""concepto"":""$concepto"",""monto"":$monto,""idVisitaMedica"":$(if ($visita) { $visita } else { 'null' })}"
}
function Interno($cambios) {
    $datos = [ordered]@{
        nombres = "Interno"; apellidos = "Prueba Validacion"; fechaNacimiento = "1945-03-15"; fechaIngreso = "2026-10-01"
        motivoIngreso = "Cuidado permanente"; nombreFamiliar = "Familiar"; apellidoFamiliar = "Prueba"; parentesco = "Hijo"
        telefonoFamiliar = "5555-0001"; correoFamiliar = "familiar.a@ejemplo.test"; cuotaMensual = 500
    }
    foreach ($k in $cambios.Keys) { $datos[$k] = $cambios[$k] }
    return ($datos | ConvertTo-Json)
}

$casos = @(
    @{ Caso = "FAR-05"; Descripcion = "Despachar de nuevo una prescripción ya despachada"; Metodo = "PUT"; Ruta = "/Prescripciones/$($despachada.idPrescripcion)/despachar"; Cuerpo = '{"costo":40}'; Esperado = 409 },
    @{ Caso = "FAR-05"; Descripcion = "Despachar una prescripción que no existe"; Metodo = "PUT"; Ruta = "/Prescripciones/99999/despachar"; Cuerpo = '{"costo":40}'; Esperado = 404 },
    @{ Caso = "LAB-06"; Descripcion = "Registrar de nuevo el resultado de un examen"; Metodo = "PUT"; Ruta = "/Examenes/$($conResultado.idExamenSolicitado)/resultado"; Cuerpo = '{"resultado":"Repetido","costo":75}'; Esperado = 409 },
    @{ Caso = "LAB-05"; Descripcion = "Registrar el resultado de un examen que no existe"; Metodo = "PUT"; Ruta = "/Examenes/99999/resultado"; Cuerpo = '{"resultado":"x","costo":75}'; Esperado = 404 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro con monto -1.00"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro -1); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro con monto 0.00"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro 0); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro con monto 100,000,000.00"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro 100000000); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro con monto 10.555 (tres decimales)"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro 10.555); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro con monto «abc»"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro '"abc"'); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Cobro sin concepto"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro 100 ""); Esperado = 400 },
    @{ Caso = "FIN-06"; Descripcion = "Cobro ligado a una visita que no es de su interno"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = (Cobro 100 "Cargo médico" 99999); Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Donación con monto -100.00"; Metodo = "POST"; Ruta = "/Donaciones"; Cuerpo = '{"tipoDonante":"Particular","nombreDonante":"Prueba","monto":-100,"descripcion":""}'; Esperado = 400 },
    @{ Caso = "FIN-05"; Descripcion = "Gasto con monto -350.00"; Metodo = "POST"; Ruta = "/Gastos"; Cuerpo = '{"concepto":"Energía eléctrica","monto":-350}'; Esperado = 400 },
    @{ Caso = "RES-02"; Descripcion = "Interno sin nombres"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ nombres = "" }); Esperado = 400 },
    @{ Caso = "RES-02"; Descripcion = "Interno sin correo del familiar"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ correoFamiliar = "" }); Esperado = 400 },
    @{ Caso = "RES-03"; Descripcion = "Correo del familiar «familiar.ejemplo»"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ correoFamiliar = "familiar.ejemplo" }); Esperado = 400 },
    @{ Caso = "RES-03"; Descripcion = "Cuota mensual -1.00"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ cuotaMensual = -1 }); Esperado = 400 },
    @{ Caso = "RES-03"; Descripcion = "Fecha de nacimiento 01/01/2030"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ fechaNacimiento = "2030-01-01" }); Esperado = 400 },
    @{ Caso = "RES-03"; Descripcion = "Fecha de ingreso anterior al nacimiento"; Metodo = "POST"; Ruta = "/Internos"; Cuerpo = (Interno @{ fechaIngreso = "1940-01-01" }); Esperado = 400 },
    @{ Caso = "ROB-05"; Descripcion = "JSON mal formado"; Metodo = "POST"; Ruta = "/Cobros"; Cuerpo = '{"monto": 10,'; Esperado = 400 }
)

$antes = Foto
$filas = @()
$correctas = 0
foreach ($c in $casos) {
    $r = Enviar $c.Metodo $c.Ruta $c.Cuerpo
    $ok = $r.Codigo -eq $c.Esperado
    if ($ok) { $correctas++ }
    $filas += "| $($c.Caso) | $($c.Descripcion) | $($c.Metodo) $($c.Ruta) | $($c.Esperado) | $($r.Codigo) | $($r.Mensaje) | $(if ($ok) { 'Correcto' } else { '**FALLA**' }) |"
}
$despues = Foto

$sinCambios = ($antes | ConvertTo-Json) -eq ($despues | ConvertTo-Json)

$md = @()
$md += "# Resultados de las pruebas de validación en el servidor"
$md += ""
$md += "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm') · API: $Api"
$md += ""
$md += "**$correctas de $($casos.Count) correctas.** Datos sin cambios después de las pruebas: **$(if ($sinCambios) { 'sí' } else { 'NO' })**."
$md += ""
$md += "| Caso del plan | Prueba | Petición | Esperado | Obtenido | Mensaje del servidor | Resultado |"
$md += "|---|---|---|---|---|---|---|"
$md += $filas
$md += ""
$md += "## Datos antes y después"
$md += ""
$md += "| Dato | Antes | Después |"
$md += "|---|---|---|"
foreach ($k in $antes.Keys) { $md += "| $k | $($antes[$k]) | $($despues[$k]) |" }
$md | Set-Content -Path $Salida -Encoding utf8

"$correctas de $($casos.Count) correctas. Datos sin cambios: $sinCambios"
"Resultados en $Salida"
