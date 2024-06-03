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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240519132731_InitialCreate'
)
BEGIN
    CREATE TABLE [CurrencyRates] (
        [Id] bigint NOT NULL IDENTITY,
        [CurrencyCode] nvarchar(450) NOT NULL,
        [CurrencyName] nvarchar(max) NOT NULL,
        [RateToUSD] decimal(18,6) NOT NULL,
        [UpdatedAt] bigint NOT NULL,
        [NextUpdateAt] bigint NOT NULL,
        CONSTRAINT [PK_CurrencyRates] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240519132731_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CurrencyCode] ON [CurrencyRates] ([CurrencyCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240519132731_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240519132731_InitialCreate', N'8.0.5');
END;
GO

COMMIT;
GO

