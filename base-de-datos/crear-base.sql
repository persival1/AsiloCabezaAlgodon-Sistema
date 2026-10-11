IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908041356_InicialUsuarios'
)
BEGIN
    CREATE TABLE [Roles] (
        [IdRol] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([IdRol])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908041356_InicialUsuarios'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [IdUsuario] int NOT NULL IDENTITY,
        [NombreUsuario] nvarchar(450) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [NombreCompleto] nvarchar(max) NOT NULL,
        [Correo] nvarchar(max) NOT NULL,
        [IdRol] int NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([IdUsuario]),
        CONSTRAINT [FK_Usuarios_Roles_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Roles] ([IdRol]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908041356_InicialUsuarios'
)
BEGIN
    CREATE INDEX [IX_Usuarios_IdRol] ON [Usuarios] ([IdRol]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908041356_InicialUsuarios'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908041356_InicialUsuarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908041356_InicialUsuarios', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE TABLE [Internos] (
        [IdInterno] int NOT NULL IDENTITY,
        [Nombres] nvarchar(max) NOT NULL,
        [Apellidos] nvarchar(max) NOT NULL,
        [FechaNacimiento] datetime2 NOT NULL,
        [FechaIngreso] datetime2 NOT NULL,
        [MotivoIngreso] nvarchar(max) NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Internos] PRIMARY KEY ([IdInterno])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE TABLE [Familiares] (
        [IdFamiliar] int NOT NULL IDENTITY,
        [Nombres] nvarchar(max) NOT NULL,
        [Apellidos] nvarchar(max) NOT NULL,
        [Parentesco] nvarchar(max) NOT NULL,
        [Telefono] nvarchar(max) NOT NULL,
        [Correo] nvarchar(max) NOT NULL,
        [CuotaMensual] decimal(10,2) NOT NULL,
        [SaldoPendiente] decimal(10,2) NOT NULL,
        [IdInterno] int NOT NULL,
        CONSTRAINT [PK_Familiares] PRIMARY KEY ([IdFamiliar]),
        CONSTRAINT [FK_Familiares_Internos_IdInterno] FOREIGN KEY ([IdInterno]) REFERENCES [Internos] ([IdInterno]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE TABLE [MedicamentosCajon] (
        [IdMedicamentoCajon] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [Dosis] nvarchar(max) NOT NULL,
        [Frecuencia] nvarchar(max) NOT NULL,
        [IdInterno] int NOT NULL,
        CONSTRAINT [PK_MedicamentosCajon] PRIMARY KEY ([IdMedicamentoCajon]),
        CONSTRAINT [FK_MedicamentosCajon_Internos_IdInterno] FOREIGN KEY ([IdInterno]) REFERENCES [Internos] ([IdInterno]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE TABLE [Psicopatologias] (
        [IdPsicopatologia] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NOT NULL,
        [FechaDiagnostico] datetime2 NOT NULL,
        [IdInterno] int NOT NULL,
        CONSTRAINT [PK_Psicopatologias] PRIMARY KEY ([IdPsicopatologia]),
        CONSTRAINT [FK_Psicopatologias_Internos_IdInterno] FOREIGN KEY ([IdInterno]) REFERENCES [Internos] ([IdInterno]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE TABLE [Solicitudes] (
        [IdSolicitud] int NOT NULL IDENTITY,
        [Motivo] nvarchar(max) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [IdInterno] int NOT NULL,
        [IdMedicoGeneral] int NOT NULL,
        [IdEspecialista] int NULL,
        [IdEnfermero] int NULL,
        [FechaHoraCita] datetime2 NULL,
        CONSTRAINT [PK_Solicitudes] PRIMARY KEY ([IdSolicitud]),
        CONSTRAINT [FK_Solicitudes_Internos_IdInterno] FOREIGN KEY ([IdInterno]) REFERENCES [Internos] ([IdInterno]) ON DELETE CASCADE,
        CONSTRAINT [FK_Solicitudes_Usuarios_IdEnfermero] FOREIGN KEY ([IdEnfermero]) REFERENCES [Usuarios] ([IdUsuario]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_IdEspecialista] FOREIGN KEY ([IdEspecialista]) REFERENCES [Usuarios] ([IdUsuario]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_IdMedicoGeneral] FOREIGN KEY ([IdMedicoGeneral]) REFERENCES [Usuarios] ([IdUsuario]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Familiares_IdInterno] ON [Familiares] ([IdInterno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_MedicamentosCajon_IdInterno] ON [MedicamentosCajon] ([IdInterno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_Psicopatologias_IdInterno] ON [Psicopatologias] ([IdInterno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_IdEnfermero] ON [Solicitudes] ([IdEnfermero]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_IdEspecialista] ON [Solicitudes] ([IdEspecialista]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_IdInterno] ON [Solicitudes] ([IdInterno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_IdMedicoGeneral] ON [Solicitudes] ([IdMedicoGeneral]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925044634_AgregarSolicitudes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925044634_AgregarSolicitudes', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE TABLE [VisitasMedicas] (
        [IdVisitaMedica] int NOT NULL IDENTITY,
        [Diagnostico] nvarchar(max) NOT NULL,
        [Observaciones] nvarchar(max) NOT NULL,
        [FechaVisita] datetime2 NOT NULL,
        [IdSolicitud] int NOT NULL,
        CONSTRAINT [PK_VisitasMedicas] PRIMARY KEY ([IdVisitaMedica]),
        CONSTRAINT [FK_VisitasMedicas_Solicitudes_IdSolicitud] FOREIGN KEY ([IdSolicitud]) REFERENCES [Solicitudes] ([IdSolicitud]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE TABLE [ExamenesSolicitados] (
        [IdExamenSolicitado] int NOT NULL IDENTITY,
        [NombreExamen] nvarchar(max) NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [Resultado] nvarchar(max) NULL,
        [FechaResultado] datetime2 NULL,
        [IdVisitaMedica] int NOT NULL,
        CONSTRAINT [PK_ExamenesSolicitados] PRIMARY KEY ([IdExamenSolicitado]),
        CONSTRAINT [FK_ExamenesSolicitados_VisitasMedicas_IdVisitaMedica] FOREIGN KEY ([IdVisitaMedica]) REFERENCES [VisitasMedicas] ([IdVisitaMedica]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE TABLE [Prescripciones] (
        [IdPrescripcion] int NOT NULL IDENTITY,
        [NombreMedicamento] nvarchar(max) NOT NULL,
        [Dosis] nvarchar(max) NOT NULL,
        [Indicaciones] nvarchar(max) NOT NULL,
        [Estado] nvarchar(max) NOT NULL,
        [Costo] decimal(10,2) NULL,
        [FechaDespacho] datetime2 NULL,
        [IdVisitaMedica] int NOT NULL,
        CONSTRAINT [PK_Prescripciones] PRIMARY KEY ([IdPrescripcion]),
        CONSTRAINT [FK_Prescripciones_VisitasMedicas_IdVisitaMedica] FOREIGN KEY ([IdVisitaMedica]) REFERENCES [VisitasMedicas] ([IdVisitaMedica]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE INDEX [IX_ExamenesSolicitados_IdVisitaMedica] ON [ExamenesSolicitados] ([IdVisitaMedica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE INDEX [IX_Prescripciones_IdVisitaMedica] ON [Prescripciones] ([IdVisitaMedica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    CREATE INDEX [IX_VisitasMedicas_IdSolicitud] ON [VisitasMedicas] ([IdSolicitud]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052003_AgregarVisitasMedicas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925052003_AgregarVisitasMedicas', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052901_AgregarCaja'
)
BEGIN
    CREATE TABLE [Cobros] (
        [IdCobro] int NOT NULL IDENTITY,
        [Concepto] nvarchar(max) NOT NULL,
        [Monto] decimal(10,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [IdFamiliar] int NOT NULL,
        CONSTRAINT [PK_Cobros] PRIMARY KEY ([IdCobro]),
        CONSTRAINT [FK_Cobros_Familiares_IdFamiliar] FOREIGN KEY ([IdFamiliar]) REFERENCES [Familiares] ([IdFamiliar]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052901_AgregarCaja'
)
BEGIN
    CREATE TABLE [Donaciones] (
        [IdDonacion] int NOT NULL IDENTITY,
        [TipoDonante] nvarchar(max) NOT NULL,
        [NombreDonante] nvarchar(max) NOT NULL,
        [Monto] decimal(10,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Descripcion] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Donaciones] PRIMARY KEY ([IdDonacion])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052901_AgregarCaja'
)
BEGIN
    CREATE TABLE [Gastos] (
        [IdGasto] int NOT NULL IDENTITY,
        [Concepto] nvarchar(max) NOT NULL,
        [Monto] decimal(10,2) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Gastos] PRIMARY KEY ([IdGasto])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052901_AgregarCaja'
)
BEGIN
    CREATE INDEX [IX_Cobros_IdFamiliar] ON [Cobros] ([IdFamiliar]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925052901_AgregarCaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925052901_AgregarCaja', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925054126_AgregarCostosYReportes'
)
BEGIN
    ALTER TABLE [VisitasMedicas] ADD [CostoConsulta] decimal(10,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925054126_AgregarCostosYReportes'
)
BEGIN
    ALTER TABLE [ExamenesSolicitados] ADD [Costo] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925054126_AgregarCostosYReportes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925054126_AgregarCostosYReportes', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925234457_AgregarFichaMedicaYCargosPorVisita'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [Especialidad] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925234457_AgregarFichaMedicaYCargosPorVisita'
)
BEGIN
    ALTER TABLE [Cobros] ADD [IdVisitaMedica] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925234457_AgregarFichaMedicaYCargosPorVisita'
)
BEGIN
    CREATE INDEX [IX_Cobros_IdVisitaMedica] ON [Cobros] ([IdVisitaMedica]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925234457_AgregarFichaMedicaYCargosPorVisita'
)
BEGIN
    ALTER TABLE [Cobros] ADD CONSTRAINT [FK_Cobros_VisitasMedicas_IdVisitaMedica] FOREIGN KEY ([IdVisitaMedica]) REFERENCES [VisitasMedicas] ([IdVisitaMedica]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260925234457_AgregarFichaMedicaYCargosPorVisita'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260925234457_AgregarFichaMedicaYCargosPorVisita', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926000529_AgregarEspecialidadRequerida'
)
BEGIN
    ALTER TABLE [Solicitudes] ADD [EspecialidadRequerida] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926000529_AgregarEspecialidadRequerida'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926000529_AgregarEspecialidadRequerida', N'10.0.11');
END;

COMMIT;
GO

