# Copia los datos de una base SQL Server local a la base de Azure, con los mismos Id,
# dentro de una sola transacción (o se copia todo o no se copia nada).
# Se ejecuta con Windows PowerShell 5.1 (usa System.Data.SqlClient).
param([string]$Config, [string]$Origen)

$cfg = Get-Content $Config -Raw | ConvertFrom-Json
$destino = "Server=tcp:$($cfg.servidorSql).database.windows.net,1433;Database=$($cfg.baseDatos);User ID=$($cfg.adminSql);Password=$($cfg.claveSql);Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;"

# Orden que respeta las llaves foráneas
$tablas = @("Roles","Usuarios","Internos","Familiares","Psicopatologias","MedicamentosCajon",
            "Solicitudes","VisitasMedicas","ExamenesSolicitados","Prescripciones",
            "Donaciones","Gastos","Cobros")

$cnO = New-Object System.Data.SqlClient.SqlConnection $Origen;  $cnO.Open()
$cnD = New-Object System.Data.SqlClient.SqlConnection $destino; $cnD.Open()

# Seguridad: solo copia si la base de destino está vacía (no duplica ni pisa datos)
$chk = $cnD.CreateCommand(); $chk.CommandText = "SELECT (SELECT COUNT(*) FROM Usuarios) + (SELECT COUNT(*) FROM Internos)"
if ([int]$chk.ExecuteScalar() -gt 0) { throw "La base de destino ya tiene datos. No se copia nada." }

$tx = $cnD.BeginTransaction()
try {
    foreach ($t in $tablas) {
        $cmd = $cnO.CreateCommand(); $cmd.CommandText = "SELECT * FROM [$t]"
        $tabla = New-Object System.Data.DataTable
        $tabla.Load($cmd.ExecuteReader())
        $bc = New-Object System.Data.SqlClient.SqlBulkCopy($cnD, [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity, $tx)
        $bc.DestinationTableName = "[$t]"
        foreach ($col in $tabla.Columns) { [void]$bc.ColumnMappings.Add($col.ColumnName, $col.ColumnName) }
        $bc.WriteToServer($tabla)
        "{0,-22} {1,4} filas" -f $t, $tabla.Rows.Count
    }
    $tx.Commit()
} catch {
    $tx.Rollback()
    throw "Se deshizo la copia completa: " + $_.Exception.Message
}
$cnO.Close(); $cnD.Close()
