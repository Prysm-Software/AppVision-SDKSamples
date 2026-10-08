/****** AppVision 4.10 - database State - tables scripted from a 4.10 database on 2026-10-08 ******/
/****** Documentation only: this script DROPS and recreates the database. Never run it on an AppVision server. ******/

USE master;
GO

DROP DATABASE State
GO

CREATE DATABASE State
GO

ALTER DATABASE State SET RECOVERY SIMPLE
GO

USE State
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Alarms](
	[AlarmId] [bigint] NOT NULL,
	[AlarmDateReceive] [datetimeoffset](7) NOT NULL,
	[AlarmDateStart] [datetimeoffset](7) NOT NULL,
	[AlarmDateAck] [datetimeoffset](7) NOT NULL,
	[AlarmDateReport] [datetimeoffset](7) NOT NULL,
	[AlarmDateEnd] [datetimeoffset](7) NOT NULL,
	[AlarmSeverity] [int] NOT NULL,
	[AlarmCount] [int] NOT NULL,
	[ParentId] [int] NULL,
	[AlarmCustomId] [int] NULL,
	[AreaId] [int] NULL,
	[SiteId] [int] NULL,
	[SiteAlarmId] [int] NULL,
	[UserIdAck] [int] NULL,
	[UserIdReport] [int] NULL,
	[UserNameAck] [nvarchar](max) NULL,
	[UserNameReport] [nvarchar](max) NULL,
	[AlarmType] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[SourceId2] [int] NOT NULL,
	[SourceId3] [int] NOT NULL,
	[SourceId4] [int] NULL,
	[AlarmStateDesc] [nvarchar](max) NULL,
	[AlarmAreaDesc] [nvarchar](max) NULL,
	[AlarmDescription] [nvarchar](max) NULL,
	[AlarmInfo] [nvarchar](max) NULL,
	[AlarmOriginDesc] [nvarchar](max) NULL,
	[AlarmInstructions] [nvarchar](max) NULL,
	[AlarmReport] [nvarchar](max) NULL,
	[AlarmUrl] [nvarchar](max) NULL,
	[AlarmGroupNames] [nvarchar](max) NULL,
	[AlarmGroupDesc] [nvarchar](max) NULL,
	[AlarmAttachments] [nvarchar](max) NULL,
	[AlarmPhoto] [nvarchar](max) NULL,
	[AlarmVideo] [nvarchar](max) NULL,
	[AlarmExtension] [nvarchar](max) NULL,
	[AlarmCustomExtension] [nvarchar](max) NULL,
	[AlarmParameters] [nvarchar](max) NULL,
	[SourceName] [nvarchar](max) NULL,
	[WaitAcknowledge] [bit] NOT NULL,
	[WaitReport] [bit] NOT NULL,
	[WaitProcess] [bit] NULL,
	[ServerName] [nvarchar](256) NULL,
	[DateWrite] [datetime] NULL,
	[AlarmGuid] [nvarchar](64) NULL,
	[AlarmDateClose] [datetimeoffset](7) NULL,
	[UserNameClose] [nvarchar](64) NULL,
	[ReadyToClose] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Alarms] PRIMARY KEY CLUSTERED 
(
	[AlarmId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[Alarms] ADD  CONSTRAINT [ReadyToClose_NotNull]  DEFAULT ((0)) FOR [ReadyToClose]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CurrentStates](
	[VariableId] [int] NOT NULL,
	[VariableName] [nvarchar](max) NULL,
	[Value] [ntext] NULL,
	[Date] [datetimeoffset](7) NOT NULL,
	[Info] [nvarchar](max) NULL,
	[Quality] [int] NULL,
	[OriginId] [int] NOT NULL,
	[IsLocked] [bit] NOT NULL,
	[IsAlarmMasked] [bit] NOT NULL,
	[Parameters] [nvarchar](max) NULL,
	[ServerName] [nvarchar](256) NULL,
	[VariableGuid] [nvarchar](64) NULL,
	[DateUnlock] [datetime2](7) NULL,
	[DateUnmask] [datetime2](7) NULL,
	[OriginAction] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.CurrentStates] PRIMARY KEY CLUSTERED 
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Overlays](
	[Name] [nvarchar](255) NOT NULL,
	[Date] [datetimeoffset](7) NULL,
	[Synoptic] [nvarchar](max) NULL,
	[Recipient] [nvarchar](max) NULL,
	[Symbol] [nvarchar](max) NULL,
	[PositionString] [nvarchar](max) NULL,
	[SymbolParameters] [nvarchar](max) NULL,
	[SymbolProperties] [nvarchar](max) NULL,
	[Parameters] [nvarchar](max) NULL,
	[SizeString] [nvarchar](max) NULL,
	[Layers] [nvarchar](max) NULL,
	[ZIndex] [int] NULL,
	[SymbolZoomLevel] [float] NULL,
 CONSTRAINT [PK_dbo.Overlays] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScenarioStates](
	[Id] [int] NOT NULL,
	[ScenarioId] [int] NOT NULL,
	[TriggerSerialized] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ScenarioStates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tasks](
	[TaskId] [int] NOT NULL,
	[TaskDate] [datetimeoffset](7) NOT NULL,
	[TaskDateAck] [datetimeoffset](7) NOT NULL,
	[TaskDateReport] [datetimeoffset](7) NOT NULL,
	[TaskType] [int] NOT NULL,
	[TaskCount] [int] NOT NULL,
	[AreaId] [int] NULL,
	[SiteId] [int] NULL,
	[SiteTaskId] [int] NULL,
	[TaskGroupNames] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[UserIdAck] [int] NULL,
	[UserIdReport] [int] NULL,
	[TaskContent] [nvarchar](max) NULL,
	[TaskInfo] [nvarchar](max) NULL,
	[TaskSourceItems] [nvarchar](max) NULL,
	[TaskAreaDesc] [nvarchar](max) NULL,
	[TaskGroupDesc] [nvarchar](max) NULL,
	[TaskOriginDesc] [nvarchar](max) NULL,
	[TaskAttachments] [nvarchar](max) NULL,
	[TaskUrl] [nvarchar](max) NULL,
	[TaskPhoto] [nvarchar](max) NULL,
	[TaskVideo] [nvarchar](max) NULL,
	[TaskExtension] [nvarchar](max) NULL,
	[TaskCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Tasks] PRIMARY KEY CLUSTERED 
(
	[TaskId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserStates](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Parameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.UserStates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
