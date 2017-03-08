CREATE TABLE [dbo].[ExperimentResult]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
	[Stop1Percentage] INT NULL, 
	[Stop2Percentage] INT NULL, 
	[Stop12Percentage] INT NULL, 
	[Stop21Percentage] INT NULL, 
	[ExecutionTime] TIME NULL, 
	[ExperimentId] UNIQUEIDENTIFIER NULL, 
	CONSTRAINT [FK_ExperimentResult_Experiment] FOREIGN KEY ([ExperimentId]) REFERENCES [dbo].[Experiment]([Id])
)
