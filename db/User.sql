USE [model]

CREATE TABLE [dbo].[User]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[Username] NVARCHAR(MAX) NULL, 
	[Password] NVARCHAR(MAX) NULL, 
	[Role] NVARCHAR(MAX) NULL
)

INSERT INTO [dbo].[User] (Username, Password, Role) VALUES ('admin', 'admin', 'admin')