# -----------------------------------------------------------------------------
# Pruebas de la semana 3 del plan de SQA: especialista asignado, estados de la solicitud
# y mensaje honesto del correo (AUT-08, EXP-05 a EXP-08, COR-02, COR-04)
#
# Qué hace: recorre el ciclo completo de UNA solicitud nueva y, en cada paso, intenta también
# lo que NO se debe poder hacer:
#   1. El médico general crea la solicitud (Pendiente).
#   2. Visita sobre la solicitud Pendiente                      -> 409 (EXP-07)
#   3. Asignar con fecha pasada, Farmacia como especialista
#      o Laboratorio como enfermero                             -> 400 (EXP-06)
#   4. Asignación correcta                                      -> 200 y mensaje honesto del correo (EXP-05, COR-04 / COR-02)
#   5. El Especialista 2 registra la visita de otra persona     -> 403 (AUT-08), también si
#      manda en el cuerpo el ID del especialista asignado
#   6. Visita correcta del especialista asignado                -> 200, saldo +Q150.00 una sola vez (EXP-08)
#   7. Segunda visita sobre la misma solicitud                  -> 409, el saldo no cambia (EXP-07)
#   8. Reasignar la solicitud ya atendida                       -> 409 (EXP-07)
#
# CREA DATOS (una solicitud y una visita de Q150.00 por ejecución). Ejecutarlo SOLO en el
# entorno local o de pruebas, nunca en Azure.
#
# Requisitos: la API corriendo en modo Development, los usuarios de prueba creados
# (POST /api/Seed/admin y /api/Seed/usuarios-prueba; este script vuelve a llamar al segundo
# para asegurar que exista «especialista2») y al menos un interno.
# Uso:  pwsh ./probar-semana-3.ps1
# -----------------------------------------------------------------------------

param(
    [string]$Api = "https://localhost:7022/api",
    [string]$Salida = "$PSScriptRoot/resultados-semana-3.md"
)

function Token($usuario, $clave) {
    $cuerpo = @{ nombreUsuario = $usuario; password = $clave } | ConvertTo-Json
    return (Invoke-RestMethod -Method Post -Uri "$Api/Auth/login" -ContentType "application/json" -Body $cuerpo -SkipCertificateCheck).token
}

# Envía la petición con el token indicado y devuelve el código HTTP y el cuerpo de la respuesta
function Enviar($token, $metodo, $ruta, $cuerpo = $null) {
    $p = @{ Method = $metodo; Uri = "$Api$ruta"; Headers = @{ Authorization = "Bearer $token" }; SkipCertificateCheck = $true; SkipHttpErrorCheck = $true }
    if ($cuerpo) { $p.ContentType = "application/json"; $p.Body = $cuerpo }
    $r = Invoke-WebRequest @p
    $datos = $null
    try { $datos = $r.Content | ConvertFrom-Json } catch { }
    return @{ Codigo = [int]$r.StatusCode; Datos = $datos }
}

function Leer($token, $ruta) { (Enviar $token "GET" $ruta).Datos | ForEach-Object { $_ } }

# Asegura que exista el segundo especialista (solo funciona en Development)
Invoke-WebRequest -Method Post -Uri "$Api/Seed/usuarios-prueba" -SkipCertificateCheck -SkipHttpErrorCheck | Out-Null

$admin = Token "admin" "admin123"
$medico = Token "medicogeneral" "prueba123"
$esp1 = Token "especialista" "prueba123"
$esp2 = Token "especialista2" "prueba123"

$usuarios = Leer $admin "/Usuarios"
function IdDe($nombreUsuario) { ($usuarios | Where-Object nombreUsuario -eq $nombreUsuario).idUsuario }
$idEsp1 = IdDe "especialista"; $idEnfermero = IdDe "enfermero"; $idFarmacia = IdDe "farmacia"; $idLaboratorio = IdDe "laboratorio"

$interno = @(Leer $admin "/Internos")[0]
function Saldo() { (@(Leer $admin "/Reportes/conciliacion-saldos") | Where-Object idFamiliar -eq $interno.idFamiliar).saldoGuardado }
function Estado($id) { (@(Leer $admin "/Solicitudes") | Where-Object idSolicitud -eq $id).estado }
function Visitas() { @(Leer $admin "/VisitasMedicas?idInterno=$($interno.idInterno)").Count }

$fechaFutura = (Get-Date).AddDays(7).Date.AddHours(9).ToString("s")
function Asignacion($idEspecialista, $idEnf, $fecha) {
    return (@{ idEspecialista = $idEspecialista; idEnfermero = $idEnf; fechaHoraCita = $fecha } | ConvertTo-Json)
}
function Visita($idSolicitud, $extra = @{}) {
    $v = [ordered]@{
        idSolicitud = $idSolicitud; diagnostico = "Lumbalgia mecánica"; observaciones = "Reposo relativo"; costoConsulta = 150
        examenes = @(@{ nombreExamen = "Hemograma" })
        prescripciones = @(@{ nombreMedicamento = "Diclofenaco"; dosis = "50 mg"; indicaciones = "Cada 12 horas por 5 días" })
    }
    foreach ($k in $extra.Keys) { $v[$k] = $extra[$k] }
    return ($v | ConvertTo-Json -Depth 4)
}

$filas = @(); $correctas = 0; $total = 0
function Revisar($caso, $descripcion, $esperado, $r, $extraOk = $true, $nota = "") {
    $script:total++
    $ok = ($r.Codigo -eq $esperado) -and $extraOk
    if ($ok) { $script:correctas++ }
    $mensaje = if ($r.Datos.mensaje) { $r.Datos.mensaje } else { "" }
    $script:filas += "| $caso | $descripcion | $esperado | $($r.Codigo) | $mensaje | $nota | $(if ($ok) { 'Correcto' } else { '**FALLA**' }) |"
}

$saldoInicial = Saldo
$visitasIniciales = Visitas

# 1. Crear la solicitud
$crear = Enviar $medico "POST" "/Solicitudes" (@{ idInterno = $interno.idInterno; motivo = "Prueba semana 3: dolor lumbar"; especialidadRequerida = "Geriatría" } | ConvertTo-Json)
$id = $crear.Datos.idSolicitud
Revisar "EXP-04" "El médico general crea la solicitud" 200 $crear ((Estado $id) -eq "Pendiente") "Estado: $(Estado $id)"

# 2. Visita sobre la solicitud Pendiente
$r = Enviar $esp1 "POST" "/VisitasMedicas" (Visita $id)
Revisar "EXP-07" "Visita sobre la solicitud Pendiente" 409 $r ((Visitas) -eq $visitasIniciales) "Visitas sin cambio"

# 3. Asignaciones inválidas
$r = Enviar $admin "PUT" "/Solicitudes/$id/asignar" (Asignacion $idEsp1 $idEnfermero "2020-01-01T09:00:00")
Revisar "EXP-06" "Asignar con fecha pasada (01/01/2020)" 400 $r ((Estado $id) -eq "Pendiente") "Sigue: $(Estado $id)"
$r = Enviar $admin "PUT" "/Solicitudes/$id/asignar" (Asignacion $idFarmacia $idEnfermero $fechaFutura)
Revisar "EXP-06" "Asignar un usuario de Farmacia como especialista" 400 $r ((Estado $id) -eq "Pendiente") "Sigue: $(Estado $id)"
$r = Enviar $admin "PUT" "/Solicitudes/$id/asignar" (Asignacion $idEsp1 $idLaboratorio $fechaFutura)
Revisar "EXP-06" "Asignar un usuario de Laboratorio como enfermero" 400 $r ((Estado $id) -eq "Pendiente") "Sigue: $(Estado $id)"

# 4. Asignación correcta: el mensaje debe coincidir con lo que pasó con el correo
$r = Enviar $admin "PUT" "/Solicitudes/$id/asignar" (Asignacion $idEsp1 $idEnfermero $fechaFutura)
$honesto = if ($r.Datos.correoEnviado) { $r.Datos.mensaje -match "correo enviado" } else { $r.Datos.mensaje -match "no se pudo enviar" }
Revisar "EXP-05 / COR" "Asignación correcta (mensaje del correo según lo que pasó)" 200 $r (((Estado $id) -eq "Asignada") -and $honesto) "correoEnviado = $($r.Datos.correoEnviado)"

# 5. Otro especialista intenta registrar la visita
$r = Enviar $esp2 "POST" "/VisitasMedicas" (Visita $id)
Revisar "AUT-08" "El Especialista 2 registra la visita de una solicitud del Especialista 1" 403 $r (((Visitas) -eq $visitasIniciales) -and ((Estado $id) -eq "Asignada")) "Visitas y estado sin cambio"
$r = Enviar $esp2 "POST" "/VisitasMedicas" (Visita $id @{ idEspecialista = $idEsp1 })
Revisar "AUT-08" "Igual, pero mandando en el cuerpo el ID del especialista asignado" 403 $r ((Visitas) -eq $visitasIniciales) "El ID sale del token, no del cuerpo"

# 6. Visita correcta
$r = Enviar $esp1 "POST" "/VisitasMedicas" (Visita $id)
$saldoTrasVisita = Saldo
Revisar "EXP-08" "El especialista asignado registra la visita" 200 $r ((((Estado $id) -eq "Atendida")) -and ([decimal]$saldoTrasVisita - [decimal]$saldoInicial -eq 150)) "Saldo: $saldoInicial -> $saldoTrasVisita"

# 7. Segunda visita sobre la misma solicitud
$r = Enviar $esp1 "POST" "/VisitasMedicas" (Visita $id)
Revisar "EXP-07" "Segunda visita sobre la solicitud Atendida" 409 $r (((Saldo) -eq $saldoTrasVisita) -and ((Visitas) -eq $visitasIniciales + 1)) "Saldo sin cambio: $(Saldo)"

# 8. Reasignar la solicitud atendida
$r = Enviar $admin "PUT" "/Solicitudes/$id/asignar" (Asignacion $idEsp1 $idEnfermero $fechaFutura)
Revisar "EXP-07" "Reasignar la solicitud ya Atendida" 409 $r ((Estado $id) -eq "Atendida") "Sigue: $(Estado $id)"

$cuadra = -not (@(Leer $admin "/Reportes/conciliacion-saldos") | Where-Object { -not $_.cuadra })

$md = @()
$md += "# Resultados de las pruebas de la semana 3 (plan de SQA)"
$md += ""
$md += "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm') · API: $Api · Solicitud de prueba: $id"
$md += ""
$md += "**$correctas de $total correctas.** Conciliación de saldos al final: **$(if ($cuadra) { 'cuadra' } else { 'NO cuadra' })**."
$md += ""
$md += "| Caso del plan | Prueba | Esperado | Obtenido | Mensaje del servidor | Comprobación extra | Resultado |"
$md += "|---|---|---|---|---|---|---|"
$md += $filas
$md | Set-Content -Path $Salida -Encoding utf8

"$correctas de $total correctas. Conciliación cuadra: $cuadra"
"Resultados en $Salida"
