GO
CREATE DATABASE <%DbName%>
GO
USE <%DbName%>
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionTemplate](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Order] [int] NOT NULL,
	[Name] [nvarchar](128) NULL,
	[Description] [nvarchar](max) NULL,
	[QuestionData] [xml] NOT NULL,
	[Disabled] [bit] NOT NULL,
 CONSTRAINT [PK_QuestionTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[QuestionTemplate] ON
INSERT [dbo].[QuestionTemplate] ([Id], [Key], [Order], [Name], [Description], [QuestionData], [Disabled]) VALUES (2, N'Q1', 1, N'Question 1', NULL, N'<question key="Q1" type="Matrix"><property name="QuestionText">My first question</property><property name="RandomizeRows">True</property><property name="Rows" type="collection"><item><property name="RowId">1</property><property name="Text">row 1</property></item><item><property name="RowId">2</property><property name="Text">row 2</property></item></property><property name="Variants" type="collection"><item><property name="VariantId">1</property><property name="Text">Variant 1</property></item><item><property name="VariantId">1</property><property name="Text">Variant 2</property></item></property></question>', 0)
INSERT [dbo].[QuestionTemplate] ([Id], [Key], [Order], [Name], [Description], [QuestionData], [Disabled]) VALUES (3, N'Q2', 2, N'Question 2', NULL, N'<question key="Q1" type="Matrix"><property name="QuestionText">My second question</property><property name="RandomizeRows">True</property><property name="Rows" type="collection"><item><property name="RowId">1</property><property name="Text">row 1</property></item><item><property name="RowId">2</property><property name="Text">row 2</property></item><item><property name="RowId">3</property><property name="Text">row 3</property></item><item><property name="RowId">4</property><property name="Text">row 4</property></item></property><property name="Variants" type="collection"><item><property name="VariantId">1</property><property name="Text">Variant 1</property></item><item><property name="VariantId">1</property><property name="Text">Variant 2</property></item></property></question>', 0)
SET IDENTITY_INSERT [dbo].[QuestionTemplate] OFF
/****** Object:  Table [dbo].[RecordStatus]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordStatus](
	[Status] [int] NOT NULL,
	[Text] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_AnswerStatus] PRIMARY KEY CLUSTERED 
(
	[Status] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[RecordStatus] ([Status], [Text]) VALUES (-2, N'Deleted')
INSERT [dbo].[RecordStatus] ([Status], [Text]) VALUES (-1, N'Skiped')
INSERT [dbo].[RecordStatus] ([Status], [Text]) VALUES (0, N'Blank')
INSERT [dbo].[RecordStatus] ([Status], [Text]) VALUES (1, N'Answered')
/****** Object:  Table [dbo].[Survey]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Survey](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Description] [nvarchar](500) NULL,
	[Bonus] [int] NULL,
	[Status] [smallint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RespondentStatus]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RespondentStatus](
	[Status] [int] NOT NULL,
	[Text] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_RespondentStatus] PRIMARY KEY CLUSTERED 
(
	[Status] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (0, N'InProcess')
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (1, N'Qualified')
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (2, N'Terminated')
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (3, N'TerminatedByQuota')
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (-1, N'Rejected')
INSERT [dbo].[RespondentStatus] ([Status], [Text]) VALUES (-10, N'Removed')
/****** Object:  Table [dbo].[Respondent]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Respondent](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](128) NOT NULL,
	[Url] [nvarchar](max) NULL,
	[Country] [int] NULL,
	[Status] [int] NOT NULL,
	[StartUtc] [datetime] NOT NULL,
	[UpdatedUtc] [datetime] NOT NULL,
	[EndUtc] [datetime] NULL,
	[TerminatedOnQuestion] [nvarchar](max) NULL,
	[PanelVariables] [xml] NOT NULL,
	[CurrentQuestionIndex] [int] NULL,
	[CurrentPageIndex] [int] NULL,
 CONSTRAINT [PK__responde__3213E83F0519C6AF] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Record]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Record](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RespondentId] [bigint] NOT NULL,
	[QuestionKey] [nvarchar](50) NOT NULL,
	[QuestionType] [nvarchar](max) NOT NULL,
	[QuestionIndex] [bigint] NOT NULL,
	[QuestionVersion] [int] NOT NULL,
	[Question] [xml] NULL,
	[Answer] [xml] NULL,
	[AnswerText] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_answer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Parameter]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Parameter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RespondentId] [bigint] NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Value] [varchar](max) NULL,
 CONSTRAINT [PK__survey_p__3213E83F0CBAE877] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Parameter]
ADD CONSTRAINT [IDX_RespondentId_Name] 
UNIQUE NONCLUSTERED ([RespondentId], [Name])
WITH (
  PAD_INDEX = OFF,
  IGNORE_DUP_KEY = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
GO

SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Answer]    Script Date: 01/20/2010 13:33:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Answer](
	[RecordId] [bigint] NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_Answer_1] PRIMARY KEY CLUSTERED 
(
	[RecordId] ASC,
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_QuestionTemplate_Order]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[QuestionTemplate] ADD  CONSTRAINT [DF_QuestionTemplate_Order]  DEFAULT ((0)) FOR [Order]
GO
/****** Object:  Default [DF_QuestionTemplate_Disabled]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[QuestionTemplate] ADD  CONSTRAINT [DF_QuestionTemplate_Disabled]  DEFAULT ((0)) FOR [Disabled]
GO
/****** Object:  ForeignKey [FK_Answer_Record]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Answer]  WITH CHECK ADD  CONSTRAINT [FK_Answer_Record] FOREIGN KEY([RecordId])
REFERENCES [dbo].[Record] ([Id])
GO
ALTER TABLE [dbo].[Answer] CHECK CONSTRAINT [FK_Answer_Record]
GO
/****** Object:  ForeignKey [FK_Parameter_Respondent]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Parameter]  WITH CHECK ADD  CONSTRAINT [FK_Parameter_Respondent] FOREIGN KEY([RespondentId])
REFERENCES [dbo].[Respondent] ([Id])
GO
ALTER TABLE [dbo].[Parameter] CHECK CONSTRAINT [FK_Parameter_Respondent]
GO
/****** Object:  ForeignKey [FK_Answer_AnswerStatus]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Record]  WITH CHECK ADD  CONSTRAINT [FK_Answer_AnswerStatus] FOREIGN KEY([Status])
REFERENCES [dbo].[RecordStatus] ([Status])
GO
ALTER TABLE [dbo].[Record] CHECK CONSTRAINT [FK_Answer_AnswerStatus]
GO
/****** Object:  ForeignKey [FK_Answer_Respondent]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Record]  WITH CHECK ADD  CONSTRAINT [FK_Answer_Respondent] FOREIGN KEY([RespondentId])
REFERENCES [dbo].[Respondent] ([Id])
GO
ALTER TABLE [dbo].[Record] CHECK CONSTRAINT [FK_Answer_Respondent]
GO
/****** Object:  ForeignKey [FK_Respondent_RespondentStatus]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Respondent]  WITH CHECK ADD  CONSTRAINT [FK_Respondent_RespondentStatus] FOREIGN KEY([Status])
REFERENCES [dbo].[RespondentStatus] ([Status])
GO
ALTER TABLE [dbo].[Respondent] CHECK CONSTRAINT [FK_Respondent_RespondentStatus]
GO

CREATE TABLE [dbo].[Constraint](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[NotSatisfiedConditionAction] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
CONSTRAINT [PK_constraint] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Condition](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ConstraintId] [bigint] NOT NULL,
	[Values] [xml] NOT NULL,
	[Quota] [int] NOT NULL,
	[Action] [int] NOT NULL,
CONSTRAINT [PK_condition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  ForeignKey [FK_Answer_Respondent]    Script Date: 01/20/2010 13:33:51 ******/
ALTER TABLE [dbo].[Condition]  WITH CHECK ADD  CONSTRAINT [FK_Condition_Constraint] FOREIGN KEY([ConstraintId])
REFERENCES [dbo].[Constraint] ([Id])
GO
ALTER TABLE [dbo].[Condition] CHECK CONSTRAINT [FK_Condition_Constraint]
GO

/****** Object:  Table [dbo].[ExportTask]    Script Date: 02/18/2010 20:31:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ExportTask](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreateUtc] [datetime] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[Progress] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[StartUtc] [datetime] NULL,
	[EndUtc] [datetime] NULL,
	[ErrorText] [nvarchar](max) NULL,
	[ErrorHandle] [nvarchar](128) NULL,
 CONSTRAINT [PK_ExportTask] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Settings]    Script Date: 11/11/2010 21:44:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[Name] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](1024) NOT NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[RespondentIP]    Script Date: 07/22/2011 11:38:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RespondentIP](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RespondentId] [bigint] NOT NULL,
	[IP] [nvarchar](15) NOT NULL,
 CONSTRAINT [PK_RespondentIP] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[RespondentIP]  WITH CHECK ADD  CONSTRAINT [FK_RespondentIP_Respondent] FOREIGN KEY([RespondentId])
REFERENCES [dbo].[Respondent] ([Id])
GO

ALTER TABLE [dbo].[RespondentIP] CHECK CONSTRAINT [FK_RespondentIP_Respondent]
GO

/********************************** OPTIMISATION ****************************************/

CREATE NONCLUSTERED INDEX [IX_RESPONDENT_STATUS_CURRENT_QUESTION_INDEX] ON [dbo].[Respondent] 
(
	[Status] ASC,
	[CurrentQuestionIndex] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_KEY] ON [dbo].[Respondent]
  ([Key])
WITH (
  PAD_INDEX = OFF,
  IGNORE_DUP_KEY = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
  
GO

CREATE NONCLUSTERED INDEX [IX_RECORD_STATUS] ON [dbo].[Record] 
(
	[Status] ASC
)
INCLUDE ( [Id],
[RespondentId],
[QuestionKey],
[QuestionType],
[QuestionIndex],
[QuestionVersion],
[Question],
[Answer],
[AnswerText]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_RESPONDENT_STATUS] ON [dbo].[Respondent] 
(
	[Status] ASC
)
INCLUDE ( [Id],
[Key],
[Url],
[Country],
[StartUtc],
[UpdatedUtc],
[EndUtc],
[TerminatedOnQuestion],
[PanelVariables],
[CurrentQuestionIndex],
[CurrentPageIndex]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_PARAMETER_RESPONDENT_ID] ON [dbo].[Parameter] 
(
	[RespondentId] ASC
)
INCLUDE ( [Id],
[Name],
[Value]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_PARAMETER_RESPONDENT_ID_2] ON [dbo].[Parameter] 
(
	[RespondentId] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_RECORD_RESPONDENT_ID_QUESTION_INDEX] ON [dbo].[Record] 
(
	[RespondentId] ASC,
	[QuestionIndex] ASC
)
INCLUDE ( [Id],
[QuestionKey],
[QuestionType],
[QuestionVersion],
[Question],
[Answer],
[AnswerText],
[Status]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO