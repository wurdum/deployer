CREATE DATABASE [DeployService]
GO
USE [DeployService]
GO
/****** Object:  Table [dbo].[User]    Script Date: 07/02/2012 13:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Login] [nvarchar](20) NOT NULL,
	[Password] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Login] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeployStatus]    Script Date: 07/02/2012 13:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeployStatus](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_DeployStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeployMode]    Script Date: 07/02/2012 13:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeployMode](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_DeployMode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Deploy]    Script Date: 07/02/2012 13:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Deploy](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[User] [nvarchar](20) NOT NULL,
	[Mode] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[SessionKey] [nvarchar](50) NOT NULL,
	[SurveyName] [nvarchar](50) NOT NULL,
	[StartUtc] [datetime] NULL,
	[EndUtc] [datetime] NULL,
 CONSTRAINT [PK_Deploy] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Exception]    Script Date: 07/02/2012 13:01:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exception](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeployId] [bigint] NOT NULL,
	[Source] [nvarchar](50) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[ExceptionData] [xml] NOT NULL,
	[TimeStamp] [datetime] NULL,
 CONSTRAINT [PK_Exception] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_Deploy_DeployMode]    Script Date: 07/02/2012 13:01:56 ******/
ALTER TABLE [dbo].[Deploy]  WITH CHECK ADD  CONSTRAINT [FK_Deploy_DeployMode] FOREIGN KEY([Mode])
REFERENCES [dbo].[DeployMode] ([Id])
GO
ALTER TABLE [dbo].[Deploy] CHECK CONSTRAINT [FK_Deploy_DeployMode]
GO
/****** Object:  ForeignKey [FK_Deploy_DeployStatus]    Script Date: 07/02/2012 13:01:56 ******/
ALTER TABLE [dbo].[Deploy]  WITH CHECK ADD  CONSTRAINT [FK_Deploy_DeployStatus] FOREIGN KEY([Status])
REFERENCES [dbo].[DeployStatus] ([Id])
GO
ALTER TABLE [dbo].[Deploy] CHECK CONSTRAINT [FK_Deploy_DeployStatus]
GO
/****** Object:  ForeignKey [FK_Exception_Deploy]    Script Date: 07/02/2012 13:01:56 ******/
ALTER TABLE [dbo].[Exception]  WITH CHECK ADD  CONSTRAINT [FK_Exception_Deploy] FOREIGN KEY([DeployId])
REFERENCES [dbo].[Deploy] ([Id])
GO
ALTER TABLE [dbo].[Exception] CHECK CONSTRAINT [FK_Exception_Deploy]
GO
/****** Object:  ForeignKey [FK_Deploy_User]    Script Date: 07/02/2012 13:01:56 ******/
ALTER TABLE [dbo].[Deploy]  WITH CHECK ADD  CONSTRAINT [FK_Deploy_User] FOREIGN KEY([User])
REFERENCES [dbo].[User] ([Login])
GO
ALTER TABLE [dbo].[Deploy] CHECK CONSTRAINT [FK_Deploy_User]
GO

INSERT INTO dbo.DeployStatus ([Id], [Name]) VALUES (1, 'InProcess')
GO
INSERT INTO dbo.DeployStatus ([Id], [Name]) VALUES (2, 'Failed')
GO
INSERT INTO dbo.DeployStatus ([Id], [Name]) VALUES (3, 'Succeed')
GO

INSERT INTO [DeployService].[dbo].[DeployMode] ([Id] ,[Name]) VALUES (0, 'Install')
GO
INSERT INTO [DeployService].[dbo].[DeployMode] ([Id] ,[Name]) VALUES (1, 'UpdateBin')
GO
INSERT INTO [DeployService].[dbo].[DeployMode] ([Id] ,[Name]) VALUES (2, 'UpdateAppData')
GO
INSERT INTO [DeployService].[dbo].[DeployMode] ([Id] ,[Name]) VALUES (3, 'UpdateAllExceptConf')
GO
INSERT INTO [DeployService].[dbo].[DeployMode] ([Id] ,[Name]) VALUES (4, 'UpdateAll')
GO