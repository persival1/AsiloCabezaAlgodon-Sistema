-- ---------------------------------------------------------------------------
-- Conciliación de saldos de los familiares (observación SQA #4 / O-01)
--
-- Qué hace: recalcula el saldo de cada familiar a partir de sus movimientos
--   saldo correcto = costos médicos (consultas + exámenes + medicamentos)
--                    - cobros ligados a una visita
--   Regla del saldo (AC-01, 10/11/2026): la cuota mensual no paga cargos médicos, por eso
--   no se resta. Positivo = el familiar debe; 0 = al día; negativo = pagó de más.
-- y corrige el saldo guardado (Familiares.SaldoPendiente) SOLO donde no coincide.
--
-- Por qué hizo falta: el Ibuprofeno (Q25.50) se despachó antes de que existiera el
-- cargo automático a la cuenta del familiar, así que su costo nunca se sumó al saldo.
--
-- Cómo usarlo (respaldar antes con respaldo-y-restauracion.ps1 -Accion Respaldar):
--   sqlcmd -S localhost\SQLEXPRESS -d asilo_cabeza_algodon -E -C -f 65001 -i conciliar-saldos.sql
--   (-f 65001 hace que sqlcmd lea el archivo como UTF-8 y muestre bien los acentos)
--
-- Es seguro ejecutarlo varias veces: si todo cuadra, no cambia nada.
-- ---------------------------------------------------------------------------
SET NOCOUNT ON;

-- 1) Cálculo del saldo correcto por familiar (solo montos mayores que 0, igual que el sistema)
IF OBJECT_ID('tempdb..#conciliacion') IS NOT NULL DROP TABLE #conciliacion;

SELECT
    f.IdFamiliar,
    f.SaldoPendiente AS SaldoGuardado,
    ISNULL(consultas.Total, 0) + ISNULL(examenes.Total, 0) + ISNULL(medicamentos.Total, 0) AS Cargos,
    ISNULL(cobros.Total, 0) AS Cobros
INTO #conciliacion
FROM Familiares f
OUTER APPLY (SELECT SUM(v.CostoConsulta) AS Total
             FROM VisitasMedicas v JOIN Solicitudes s ON s.IdSolicitud = v.IdSolicitud
             WHERE s.IdInterno = f.IdInterno AND v.CostoConsulta > 0) consultas
OUTER APPLY (SELECT SUM(e.Costo) AS Total
             FROM ExamenesSolicitados e
             JOIN VisitasMedicas v ON v.IdVisitaMedica = e.IdVisitaMedica
             JOIN Solicitudes s ON s.IdSolicitud = v.IdSolicitud
             WHERE s.IdInterno = f.IdInterno AND e.Costo > 0) examenes
OUTER APPLY (SELECT SUM(p.Costo) AS Total
             FROM Prescripciones p
             JOIN VisitasMedicas v ON v.IdVisitaMedica = p.IdVisitaMedica
             JOIN Solicitudes s ON s.IdSolicitud = v.IdSolicitud
             WHERE s.IdInterno = f.IdInterno AND p.Costo > 0) medicamentos
OUTER APPLY (SELECT SUM(c.Monto) AS Total FROM Cobros c WHERE c.IdFamiliar = f.IdFamiliar AND c.IdVisitaMedica IS NOT NULL) cobros;

PRINT '--- ANTES de la corrección ---';
SELECT IdFamiliar, SaldoGuardado, Cargos - Cobros AS SaldoCorrecto, SaldoGuardado - (Cargos - Cobros) AS Diferencia
FROM #conciliacion;

-- 2) Corrección dentro de una transacción: o se aplica completa o no se aplica nada
BEGIN TRANSACTION;

UPDATE f
SET f.SaldoPendiente = c.Cargos - c.Cobros
FROM Familiares f
JOIN #conciliacion c ON c.IdFamiliar = f.IdFamiliar
WHERE f.SaldoPendiente <> c.Cargos - c.Cobros;

PRINT CONCAT('Familiares corregidos: ', @@ROWCOUNT);

COMMIT TRANSACTION;

-- 3) Verificación
PRINT '--- DESPUÉS de la corrección ---';
SELECT f.IdFamiliar, f.SaldoPendiente AS SaldoGuardado, c.Cargos - c.Cobros AS SaldoCorrecto,
       CASE WHEN f.SaldoPendiente = c.Cargos - c.Cobros THEN 'CUADRA' ELSE 'NO CUADRA' END AS Resultado
FROM Familiares f JOIN #conciliacion c ON c.IdFamiliar = f.IdFamiliar;

DROP TABLE #conciliacion;
