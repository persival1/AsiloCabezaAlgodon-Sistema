# -----------------------------------------------------------------------------
# Pruebas de autorización por rol (respuesta a la observación SQA #3)
#
# Qué hace: inicia sesión con cada uno de los 6 roles (y también SIN sesión), llama a los
# endpoints de la API y compara el código HTTP obtenido con el esperado.
#   200 = permitido    401 = no hay sesión    403 = hay sesión pero el rol no tiene permiso
#
# Es seguro ejecutarlo varias veces: los endpoints de lectura (GET) no cambian datos y los
# de escritura (POST/PUT) solo se llaman con roles que NO deben poder usarlos, así que el
# servidor responde 403 antes de tocar la base de datos.
#
# Requisitos: la API corriendo (https://localhost:7022) y los usuarios de prueba creados
# (POST /api/Seed/admin y /api/Seed/usuarios-prueba).
# Uso:  pwsh ./probar-permisos.ps1
# -----------------------------------------------------------------------------

param(
    [string]$Api = "https://localhost:7022/api",
    [string]$Salida = "$PSScriptRoot/resultados-permisos.md",
    # Opcional: archivo con líneas "usuario clave" para probar un servidor donde las
    # contraseñas no son las de prueba (por ejemplo Azure). Ese archivo NO va en el repositorio.
    [string]$ArchivoClaves = ""
)

# Usuarios de prueba (los mismos que crea /api/Seed en el entorno local)
$usuarios = [ordered]@{
    "Administrador" = @("admin", "admin123")
    "MedicoGeneral" = @("medicogeneral", "prueba123")
    "Especialista"  = @("especialista", "prueba123")
    "Enfermero"     = @("enfermero", "prueba123")
    "Laboratorio"   = @("laboratorio", "prueba123")
    "Farmacia"      = @("farmacia", "prueba123")
}
if ($ArchivoClaves) {
    foreach ($linea in Get-Content $ArchivoClaves) {
        if ($linea -match "^(\w+)\s+(\S+)$") {
            foreach ($rol in @($usuarios.Keys)) { if ($usuarios[$rol][0] -eq $matches[1]) { $usuarios[$rol][1] = $matches[2] } }
        }
    }
}
$roles = @($usuarios.Keys)

# Endpoints de LECTURA: ruta y lista de roles que SÍ deben poder entrar (el resto, 403).
$lecturas = @(
    @{ Ruta = "/Internos";                         Permitidos = @("Administrador", "MedicoGeneral", "Especialista") },
    @{ Ruta = "/Solicitudes";                      Permitidos = @("Administrador", "MedicoGeneral", "Especialista", "Enfermero") },
    @{ Ruta = "/VisitasMedicas";                   Permitidos = @("Administrador", "MedicoGeneral", "Especialista") },
    @{ Ruta = "/Examenes";                         Permitidos = @("Administrador", "Laboratorio") },
    @{ Ruta = "/Prescripciones";                   Permitidos = @("Administrador", "Farmacia") },
    @{ Ruta = "/Donaciones";                       Permitidos = @("Administrador") },
    @{ Ruta = "/Gastos";                           Permitidos = @("Administrador") },
    @{ Ruta = "/Cobros";                           Permitidos = @("Administrador") },
    @{ Ruta = "/PagosFundacion";                   Permitidos = @("Administrador") },
    @{ Ruta = "/Usuarios";                         Permitidos = @("Administrador") },
    @{ Ruta = "/Reportes/costos-por-cita";         Permitidos = @("Administrador") },
    @{ Ruta = "/Reportes/entradas";                Permitidos = @("Administrador") },
    # AC-10: informes financieros, solo Administrador (los clínicos se agregan abajo, por paciente)
    @{ Ruta = "/Reportes/pagos-fundacion";         Permitidos = @("Administrador") },
    @{ Ruta = "/Reportes/conciliacion-saldos";     Permitidos = @("Administrador") },
    @{ Ruta = "/Reportes/cobros-por-paciente?idInterno=1&fechaInicio=2026-01-01&fechaFin=2026-12-31"; Permitidos = @("Administrador") }
)

# Endpoints de ESCRITURA: solo se prueban los roles que NO tienen permiso (deben dar 403).
$escrituras = @(
    @{ Metodo = "POST"; Ruta = "/Internos";                      Prohibidos = @("Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "POST"; Ruta = "/Solicitudes";                   Prohibidos = @("Administrador", "Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "PUT";  Ruta = "/Solicitudes/1/asignar";         Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "POST"; Ruta = "/VisitasMedicas";                Prohibidos = @("Administrador", "MedicoGeneral", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "PUT";  Ruta = "/Examenes/1/resultado";          Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Farmacia") },
    @{ Metodo = "PUT";  Ruta = "/Prescripciones/1/despachar";    Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio") },
    @{ Metodo = "POST"; Ruta = "/Donaciones";                    Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "POST"; Ruta = "/Gastos";                        Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "POST"; Ruta = "/Cobros";                        Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia") },
    @{ Metodo = "POST"; Ruta = "/PagosFundacion";                Prohibidos = @("MedicoGeneral", "Especialista", "Enfermero", "Laboratorio", "Farmacia") }
)

function Obtener-Token($usuario, $clave) {
    $cuerpo = @{ nombreUsuario = $usuario; password = $clave } | ConvertTo-Json
    $r = Invoke-RestMethod -Method Post -Uri "$Api/Auth/login" -ContentType "application/json" -Body $cuerpo -SkipCertificateCheck
    return $r.token
}

function Llamar($metodo, $ruta, $token) {
    $cabeceras = @{}
    if ($token) { $cabeceras["Authorization"] = "Bearer $token" }
    $params = @{ Method = $metodo; Uri = "$Api$ruta"; Headers = $cabeceras; SkipCertificateCheck = $true; SkipHttpErrorCheck = $true }
    if ($metodo -ne "GET") { $params["ContentType"] = "application/json"; $params["Body"] = "{}" }
    return [int](Invoke-WebRequest @params).StatusCode
}

# 1) Iniciar sesión con cada rol
$tokens = @{}
foreach ($rol in $roles) {
    $tokens[$rol] = Obtener-Token $usuarios[$rol][0] $usuarios[$rol][1]
    if (-not $tokens[$rol]) { throw "No se pudo iniciar sesión como $rol. ¿La API está corriendo y existen los usuarios de prueba?" }
}

# AC-04 y AC-10: acceso horizontal. El especialista solo ve la ficha y los informes clínicos
# de los internos de sus solicitudes. Se buscan en los datos un interno "propio" y uno "ajeno".
$cab = { param($rol) @{ Authorization = "Bearer $($tokens[$rol])" } }
$solEsp = @((Invoke-RestMethod -Uri "$Api/Solicitudes" -Headers (& $cab "Especialista") -SkipCertificateCheck) | ForEach-Object { $_ })
$todos = @((Invoke-RestMethod -Uri "$Api/Internos" -Headers (& $cab "Administrador") -SkipCertificateCheck) | ForEach-Object { $_ })
$propio = ($solEsp | Select-Object -First 1).idInterno
$ajeno = ($todos | Where-Object { $solEsp.idInterno -notcontains $_.idInterno } | Select-Object -First 1).idInterno
$medicos = @("Administrador", "MedicoGeneral", "Especialista")
if ($propio) {
    "Interno propio del especialista: $propio"
    $lecturas += @(
        @{ Ruta = "/Internos/$propio"; Permitidos = $medicos },
        @{ Ruta = "/Reportes/ficha-medica/$propio"; Permitidos = $medicos },
        @{ Ruta = "/Reportes/examenes-por-paciente/$propio"; Permitidos = $medicos },
        @{ Ruta = "/Reportes/medicamentos-por-paciente/$propio"; Permitidos = $medicos })
} else { Write-Warning "El especialista no tiene solicitudes: no se prueban sus pacientes propios." }
if ($ajeno) {
    "Interno ajeno al especialista: $ajeno"
    $lecturas += @(
        @{ Ruta = "/Internos/$ajeno"; Permitidos = @("Administrador", "MedicoGeneral") },
        @{ Ruta = "/Reportes/ficha-medica/$ajeno"; Permitidos = @("Administrador", "MedicoGeneral") })
} else { Write-Warning "Todos los internos son del especialista: no se prueba el acceso a un interno ajeno." }

$filas = New-Object System.Collections.Generic.List[object]

function Registrar($metodo, $ruta, $actor, $esperado, $obtenido) {
    $filas.Add([pscustomobject]@{
        Metodo = $metodo; Ruta = $ruta; Actor = $actor
        Esperado = $esperado; Obtenido = $obtenido
        Resultado = $(if ($esperado -eq $obtenido) { "OK" } else { "FALLA" })
    })
}

# 2) Lecturas: cada rol, más "SinSesion"
foreach ($l in $lecturas) {
    foreach ($rol in $roles) {
        $esperado = if ($l.Permitidos -contains $rol) { 200 } else { 403 }
        # Con permiso, un 404 (por ejemplo, el interno 1 no existe) cuenta como OK de autorización
        $obtenido = Llamar "GET" $l.Ruta $tokens[$rol]
        if ($esperado -eq 200 -and $obtenido -eq 404) { $obtenido = 200 }
        Registrar "GET" $l.Ruta $rol $esperado $obtenido
    }
    Registrar "GET" $l.Ruta "SinSesion" 401 (Llamar "GET" $l.Ruta $null)
}

# 3) Escrituras: solo los roles prohibidos (deben dar 403)
foreach ($e in $escrituras) {
    foreach ($rol in $e.Prohibidos) {
        Registrar $e.Metodo $e.Ruta $rol 403 (Llamar $e.Metodo $e.Ruta $tokens[$rol])
    }
    Registrar $e.Metodo $e.Ruta "SinSesion" 401 (Llamar $e.Metodo $e.Ruta $null)
}

# 4) Resumen en pantalla y en archivo
$total = $filas.Count
$fallas = @($filas | Where-Object { $_.Resultado -eq "FALLA" })
$filas | Format-Table -AutoSize
"Total de pruebas: $total | Correctas: $($total - $fallas.Count) | Fallidas: $($fallas.Count)"

$md = New-Object System.Text.StringBuilder
[void]$md.AppendLine("# Resultados de las pruebas de autorización por rol")
[void]$md.AppendLine("")
[void]$md.AppendLine("Ejecutado el $(Get-Date -Format 'dd/MM/yyyy HH:mm') contra $Api")
[void]$md.AppendLine("")
[void]$md.AppendLine("**Total: $total pruebas. Correctas: $($total - $fallas.Count). Fallidas: $($fallas.Count).**")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Método | Endpoint | Actor | Esperado | Obtenido | Resultado |")
[void]$md.AppendLine("|---|---|---|---|---|---|")
foreach ($f in $filas) {
    [void]$md.AppendLine("| $($f.Metodo) | $($f.Ruta) | $($f.Actor) | $($f.Esperado) | $($f.Obtenido) | $($f.Resultado) |")
}
[IO.File]::WriteAllText($Salida, $md.ToString(), (New-Object System.Text.UTF8Encoding($false)))
"Resultados guardados en: $Salida"
if ($fallas.Count -gt 0) { exit 1 }
