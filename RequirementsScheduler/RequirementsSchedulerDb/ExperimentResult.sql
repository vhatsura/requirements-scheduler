CREATE TABLE [dbo].[ExperimentResult]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Stop1Percentage] FLOAT NULL, 
	[Stop2Percentage] FLOAT NULL, 
	[Stop3Percentage] FLOAT NULL, 
	[Stop4Percentage] FLOAT NULL,
	[OfflineExecutionTime] TIME NOT NULL,
	[OnlineExecutionTime] TIME NULL, 
	[ExperimentId] UNIQUEIDENTIFIER NULL, 
	[OfflineResolvedConflictAmount] INT NULL, 
	[OnlineResolvedConflictAmount] INT NULL, 
	[OnlineUnResolvedConflictAmount] INT NULL, 
	[DeltaCmaxMax] FLOAT NULL, 
	[DeltaCmaxAverage] FLOAT NULL, 
	CONSTRAINT [FK_ExperimentResult_Experiment] FOREIGN KEY ([ExperimentId]) REFERENCES [dbo].[Experiment]([Id])
)
