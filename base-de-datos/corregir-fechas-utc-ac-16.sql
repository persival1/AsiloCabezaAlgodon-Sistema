-- AC-16: corrige las fechas que la aplicación guardó en UTC en Azure (6 horas adelante de Guatemala)
-- antes de configurar TZ=America/Guatemala en el App Service (30/10/2026).
--
-- PELIGRO: modifica datos. Se usa SOLO con aprobación expresa y después de un respaldo.
--
-- Cómo usarlo:
--   1. Llenar @Desde y @Hasta:
--      @Desde = cuando terminó la copia de datos inicial a Azure (21/10). Las filas copiadas desde
--               la base local ya tenían la hora de Guatemala y NO se deben mover.
--      @Hasta = cuando se aplicó TZ=America/Guatemala (registro de actividad del App Service, 30/10).
--      Las dos fechas se escriben tal como quedaron guardadas, es decir, en UTC.
--   2. Ejecutar con @Aplicar = 0: solo muestra las filas que se moverían. Revisarlas una por una.
--   3. Si la lista es correcta, ejecutar con @Aplicar = 1. Todo va en una transacción.
--
-- La base de pruebas local no necesita esto: siempre guardó la hora de Guatemala.

DECLARE @Desde datetime2 = NULL;  -- por ejemplo '2026-10-21T23:00:00'
DECLARE @Hasta datetime2 = NULL;  -- por ejemplo '2026-10-30T21:30:00'
DECLARE @Aplicar bit = 0;

IF @Desde IS NULL OR @Hasta IS NULL
BEGIN
    RAISERROR('Llená @Desde y @Hasta antes de ejecutar.', 16, 1);
    RETURN;
END;

SELECT 'Cobros' AS Tabla, IdCobro AS Id, Fecha AS FechaGuardada, DATEADD(HOUR, -6, Fecha) AS FechaCorregida FROM Cobros WHERE Fecha >= @Desde AND Fecha < @Hasta
UNION ALL SELECT 'Donaciones', IdDonacion, Fecha, DATEADD(HOUR, -6, Fecha) FROM Donaciones WHERE Fecha >= @Desde AND Fecha < @Hasta
UNION ALL SELECT 'Gastos', IdGasto, Fecha, DATEADD(HOUR, -6, Fecha) FROM Gastos WHERE Fecha >= @Desde AND Fecha < @Hasta
UNION ALL SELECT 'Solicitudes', IdSolicitud, FechaCreacion, DATEADD(HOUR, -6, FechaCreacion) FROM Solicitudes WHERE FechaCreacion >= @Desde AND FechaCreacion < @Hasta
UNION ALL SELECT 'VisitasMedicas', IdVisitaMedica, FechaVisita, DATEADD(HOUR, -6, FechaVisita) FROM VisitasMedicas WHERE FechaVisita >= @Desde AND FechaVisita < @Hasta
UNION ALL SELECT 'ExamenesSolicitados', IdExamenSolicitado, FechaResultado, DATEADD(HOUR, -6, FechaResultado) FROM ExamenesSolicitados WHERE FechaResultado >= @Desde AND FechaResultado < @Hasta
UNION ALL SELECT 'Prescripciones', IdPrescripcion, FechaDespacho, DATEADD(HOUR, -6, FechaDespacho) FROM Prescripciones WHERE FechaDespacho >= @Desde AND FechaDespacho < @Hasta
ORDER BY Tabla, Id;

IF @Aplicar = 1
BEGIN
    BEGIN TRANSACTION;
    UPDATE Cobros SET Fecha = DATEADD(HOUR, -6, Fecha) WHERE Fecha >= @Desde AND Fecha < @Hasta;
    UPDATE Donaciones SET Fecha = DATEADD(HOUR, -6, Fecha) WHERE Fecha >= @Desde AND Fecha < @Hasta;
    UPDATE Gastos SET Fecha = DATEADD(HOUR, -6, Fecha) WHERE Fecha >= @Desde AND Fecha < @Hasta;
    UPDATE Solicitudes SET FechaCreacion = DATEADD(HOUR, -6, FechaCreacion) WHERE FechaCreacion >= @Desde AND FechaCreacion < @Hasta;
    UPDATE VisitasMedicas SET FechaVisita = DATEADD(HOUR, -6, FechaVisita) WHERE FechaVisita >= @Desde AND FechaVisita < @Hasta;
    UPDATE ExamenesSolicitados SET FechaResultado = DATEADD(HOUR, -6, FechaResultado) WHERE FechaResultado >= @Desde AND FechaResultado < @Hasta;
    UPDATE Prescripciones SET FechaDespacho = DATEADD(HOUR, -6, FechaDespacho) WHERE FechaDespacho >= @Desde AND FechaDespacho < @Hasta;
    COMMIT TRANSACTION;
    PRINT 'Fechas corregidas.';
END
ELSE
    PRINT 'Vista previa: no se cambió nada. Para aplicar, poné @Aplicar = 1.';
