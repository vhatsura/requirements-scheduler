CREATE TABLE [dbo].[Experiment]
(
	[Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), 
	[TestsAmount] INT NOT NULL, 
	[RequirementsAmount] INT NOT NULL, 
	[N1] INT NOT NULL, 
	[N2] INT NOT NULL, 
	[N12] INT NOT NULL, 
	[N21] INT NOT NULL, 
	[MinBoundaryRange] INT NOT NULL, 
	[MaxBoundaryRange] INT NOT NULL, 
	[MinPercentageFromA] INT NOT NULL, 
	[MaxPercentageFromA] INT NOT NULL, 
	[Status] INT NOT NULL DEFAULT 0, 
	[UserId] INT NOT NULL, 
	[Created] DATETIME2 NOT NULL DEFAULT GETDATE(), 
	CONSTRAINT [FK_Experiment_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id])
)
