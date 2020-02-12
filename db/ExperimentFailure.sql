USE [model]

CREATE TABLE [dbo].[ExperimentsFailures]
( 
    [ExperimentId] UNIQUEIDENTIFIER NOT NULL, 
    [ErrorMessage] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_ExperimentsFailures_ToExperiment] FOREIGN KEY ([ExperimentId]) REFERENCES [Experiment]([Id])
)
