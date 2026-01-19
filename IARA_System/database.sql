IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IARA_DB')
BEGIN
    CREATE DATABASE [IARA_DB];
END
GO

USE [IARA_DB];
GO

-- Fishers (Рибари)
CREATE TABLE [dbo].[Fishers] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [PersonalNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [DateOfBirth] DATETIME2 NULL,
    [Address] NVARCHAR(500) NULL,
    [Phone] NVARCHAR(50) NULL,
    [Email] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive] BIT NOT NULL DEFAULT 1
);
GO

CREATE INDEX [IX_Fishers_PersonalNumber] ON [dbo].[Fishers]([PersonalNumber]);
CREATE INDEX [IX_Fishers_Email] ON [dbo].[Fishers]([Email]);
GO

-- Ships (Кораби)
CREATE TABLE [dbo].[Ships] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [InternationalNumber] NVARCHAR(100) NOT NULL UNIQUE,
    [CallSign] NVARCHAR(50) NULL,
    [Marking] NVARCHAR(50) NULL,
    [RegistrationNumber] NVARCHAR(100) NULL,
    [HomePort] NVARCHAR(100) NULL,
    [Length] DECIMAL(10,2) NULL,
    [Width] DECIMAL(10,2) NULL,
    [GrossTonnage] DECIMAL(10,2) NULL,
    [Draught] DECIMAL(10,2) NULL,
    [EnginePower] DECIMAL(10,2) NULL,
    [EngineType] NVARCHAR(50) NULL,
    [FuelType] NVARCHAR(50) NULL,
    [AverageFuelConsumptionPerHour] DECIMAL(10,2) NULL,
    [MaxFuelCapacity] DECIMAL(10,2) NULL,
    [BuiltYear] DATETIME2 NULL,
    [MaxCrew] INT NULL,
    [IsLargeShip] BIT NOT NULL DEFAULT 0,
    [OwnerFisherId] INT NULL,
    [CaptainFisherId] INT NULL,
    [OperatorFisherId] INT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    CONSTRAINT [FK_Ships_OwnerFisher] FOREIGN KEY ([OwnerFisherId]) REFERENCES [dbo].[Fishers]([Id]),
    CONSTRAINT [FK_Ships_CaptainFisher] FOREIGN KEY ([CaptainFisherId]) REFERENCES [dbo].[Fishers]([Id]),
    CONSTRAINT [FK_Ships_OperatorFisher] FOREIGN KEY ([OperatorFisherId]) REFERENCES [dbo].[Fishers]([Id])
);
GO

CREATE INDEX [IX_Ships_InternationalNumber] ON [dbo].[Ships]([InternationalNumber]);
CREATE INDEX [IX_Ships_RegistrationNumber] ON [dbo].[Ships]([RegistrationNumber]);
CREATE INDEX [IX_Ships_OwnerFisherId] ON [dbo].[Ships]([OwnerFisherId]);
CREATE INDEX [IX_Ships_Status] ON [dbo].[Ships]([Status]);
GO

-- Licenses (Разрешителни)
CREATE TABLE [dbo].[Licenses] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [LicenseNumber] NVARCHAR(100) NOT NULL UNIQUE,
    [FisherId] INT NOT NULL,
    [ShipId] INT NULL,
    [IssueDate] DATETIME2 NOT NULL,
    [ExpiryDate] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [LicenseType] NVARCHAR(200) NULL,
    [IssuingAuthority] NVARCHAR(200) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Licenses_Fisher] FOREIGN KEY ([FisherId]) REFERENCES [dbo].[Fishers]([Id]),
    CONSTRAINT [FK_Licenses_Ship] FOREIGN KEY ([ShipId]) REFERENCES [dbo].[Ships]([Id])
);
GO

CREATE INDEX [IX_Licenses_LicenseNumber] ON [dbo].[Licenses]([LicenseNumber]);
CREATE INDEX [IX_Licenses_FisherId] ON [dbo].[Licenses]([FisherId]);
CREATE INDEX [IX_Licenses_ShipId] ON [dbo].[Licenses]([ShipId]);
CREATE INDEX [IX_Licenses_ExpiryDate] ON [dbo].[Licenses]([ExpiryDate]);
CREATE INDEX [IX_Licenses_Status] ON [dbo].[Licenses]([Status]);
GO

-- AmateurTickets (Билети за любители)
CREATE TABLE [dbo].[AmateurTickets] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FisherId] INT NOT NULL,
    [TicketNumber] NVARCHAR(100) NOT NULL UNIQUE,
    [IssueDate] DATETIME2 NOT NULL,
    [ExpiryDate] DATETIME2 NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Active',
    [IssuingAuthority] NVARCHAR(200) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_AmateurTickets_Fisher] FOREIGN KEY ([FisherId]) REFERENCES [dbo].[Fishers]([Id])
);
GO

CREATE INDEX [IX_AmateurTickets_TicketNumber] ON [dbo].[AmateurTickets]([TicketNumber]);
CREATE INDEX [IX_AmateurTickets_FisherId] ON [dbo].[AmateurTickets]([FisherId]);
CREATE INDEX [IX_AmateurTickets_ExpiryDate] ON [dbo].[AmateurTickets]([ExpiryDate]);
GO

-- LogbookEntries (Дневници)
CREATE TABLE [dbo].[LogbookEntries] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [LicenseId] INT NOT NULL,
    [FishingDate] DATETIME2 NOT NULL,
    [StartTime] TIME NULL,
    [EndTime] TIME NULL,
    [FishingArea] NVARCHAR(500) NULL,
    [FuelConsumptionLiters] DECIMAL(10,2) NULL,
    [DistanceTraveled] DECIMAL(10,2) NULL,
    [WeatherConditions] NVARCHAR(200) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_LogbookEntries_License] FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[Licenses]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_LogbookEntries_LicenseId] ON [dbo].[LogbookEntries]([LicenseId]);
CREATE INDEX [IX_LogbookEntries_FishingDate] ON [dbo].[LogbookEntries]([FishingDate]);
GO

-- CatchDetails (Детайли за улов)
CREATE TABLE [dbo].[CatchDetails] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [LogbookEntryId] INT NOT NULL,
    [FishSpecies] NVARCHAR(200) NOT NULL,
    [WeightKgs] DECIMAL(10,2) NULL,
    [Quantity] INT NULL,
    [FishingGear] NVARCHAR(200) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_CatchDetails_LogbookEntry] FOREIGN KEY ([LogbookEntryId]) REFERENCES [dbo].[LogbookEntries]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CatchDetails_LogbookEntryId] ON [dbo].[CatchDetails]([LogbookEntryId]);
CREATE INDEX [IX_CatchDetails_FishSpecies] ON [dbo].[CatchDetails]([FishSpecies]);
GO

-- AmateurCatches (Любителски улов)
CREATE TABLE [dbo].[AmateurCatches] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AmateurTicketId] INT NULL,
    [UserId] NVARCHAR(450) NOT NULL,
    [CatchDate] DATETIME2 NOT NULL,
    [FishSpecies] NVARCHAR(200) NOT NULL,
    [WeightKgs] DECIMAL(10,2) NULL,
    [Quantity] INT NULL,
    [FishingLocation] NVARCHAR(500) NULL,
    [FishingMethod] NVARCHAR(200) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_AmateurCatches_AmateurTicket] FOREIGN KEY ([AmateurTicketId]) REFERENCES [dbo].[AmateurTickets]([Id]) ON DELETE SET NULL
);
GO

CREATE INDEX [IX_AmateurCatches_AmateurTicketId] ON [dbo].[AmateurCatches]([AmateurTicketId]);
CREATE INDEX [IX_AmateurCatches_UserId] ON [dbo].[AmateurCatches]([UserId]);
CREATE INDEX [IX_AmateurCatches_CatchDate] ON [dbo].[AmateurCatches]([CatchDate]);
CREATE INDEX [IX_AmateurCatches_FishSpecies] ON [dbo].[AmateurCatches]([FishSpecies]);
GO

-- Inspectors (Инспектори)
CREATE TABLE [dbo].[Inspectors] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [BadgeNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [Department] NVARCHAR(200) NULL,
    [Phone] NVARCHAR(50) NULL,
    [Email] NVARCHAR(255) NULL,
    [HireDate] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [UserId] NVARCHAR(450) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX [IX_Inspectors_BadgeNumber] ON [dbo].[Inspectors]([BadgeNumber]);
CREATE INDEX [IX_Inspectors_UserId] ON [dbo].[Inspectors]([UserId]);
GO

-- Inspections (Инспекции)
CREATE TABLE [dbo].[Inspections] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [InspectorId] INT NULL,
    [ShipId] INT NULL,
    [LicenseId] INT NULL,
    [InspectionDate] DATETIME2 NOT NULL,
    [InspectionType] NVARCHAR(100) NOT NULL DEFAULT 'Planned',
    [Location] NVARCHAR(500) NULL,
    [Findings] NVARCHAR(MAX) NULL,
    [Violations] NVARCHAR(MAX) NULL,
    [ActionsTaken] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Planned',
    [Notes] NVARCHAR(MAX) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Inspections_Inspector] FOREIGN KEY ([InspectorId]) REFERENCES [dbo].[Inspectors]([Id]),
    CONSTRAINT [FK_Inspections_Ship] FOREIGN KEY ([ShipId]) REFERENCES [dbo].[Ships]([Id]),
    CONSTRAINT [FK_Inspections_License] FOREIGN KEY ([LicenseId]) REFERENCES [dbo].[Licenses]([Id])
);
GO

CREATE INDEX [IX_Inspections_InspectorId] ON [dbo].[Inspections]([InspectorId]);
CREATE INDEX [IX_Inspections_ShipId] ON [dbo].[Inspections]([ShipId]);
CREATE INDEX [IX_Inspections_LicenseId] ON [dbo].[Inspections]([LicenseId]);
CREATE INDEX [IX_Inspections_InspectionDate] ON [dbo].[Inspections]([InspectionDate]);
CREATE INDEX [IX_Inspections_Status] ON [dbo].[Inspections]([Status]);
GO

-- Съставни индекси
CREATE NONCLUSTERED INDEX [IX_Ships_Owner_Status] 
ON [dbo].[Ships]([OwnerFisherId], [Status])
INCLUDE ([Name], [InternationalNumber]);
GO

CREATE NONCLUSTERED INDEX [IX_Licenses_Fisher_Status] 
ON [dbo].[Licenses]([FisherId], [Status])
INCLUDE ([LicenseNumber], [ExpiryDate]);
GO
