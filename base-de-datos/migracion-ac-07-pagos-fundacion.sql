BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110233622_AgregarPagosFundacion'
)
BEGIN
    CREATE TABLE [PagosFundacion] (
        [IdPagoFundacion] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [Monto] decimal(10,2) NOT NULL,
        [NumeroComprobante] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PagosFundacion] PRIMARY KEY ([IdPagoFundacion])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261110233622_AgregarPagosFundacion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261110233622_AgregarPagosFundacion', N'10.0.11');
END;

COMMIT;
GO

