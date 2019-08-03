CREATE TABLE [dbo].[ExperimentsFailures]
(
    [Id] INT NOT NULL PRIMARY KEY, 
    [ExperimentId] UNIQUEIDENTIFIER NOT NULL, 
    [ErrorMessage] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_ExperimentsFailures_ToExperiment] FOREIGN KEY ([ExperimentId]) REFERENCES [Experiment]([Id])
)
