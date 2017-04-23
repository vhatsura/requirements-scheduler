CREATE TABLE [dbo].[ExperimentResult]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Stop1Percentage] INT NULL, 
	[Stop2Percentage] INT NULL, 
	[Stop3Percentage] INT NULL, 
	[Stop4Percentage] INT NULL, 
	[OnlineExecutionTime] TIME NULL, 
	[ExperimentId] UNIQUEIDENTIFIER NULL, 
	[OfflineResolvedConflictAmount] INT NULL, 
	[OnlineResolvedConflictAmount] INT NULL, 
	[OnlineUnResolvedConflictAmount] INT NULL, 
	[DeltaCmaxMax] FLOAT NULL, 
	[DeltaCmaxAverage] FLOAT NULL, 
	CONSTRAINT [FK_ExperimentResult_Experiment] FOREIGN KEY ([ExperimentId]) REFERENCES [dbo].[Experiment]([Id])
)
