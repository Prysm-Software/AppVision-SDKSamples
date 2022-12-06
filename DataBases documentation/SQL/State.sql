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

CREATE TABLE [dbo].[Alarms](
	[AlarmId] [bigint] NOT NULL,
	[AlarmDateReceive] [datetimeoffset](7) NOT NULL,
	[AlarmDateStart] [datetimeoffset](7) NOT NULL,
	[AlarmDateAck] [datetimeoffset](7) NOT NULL,
	[AlarmDateReport] [datetimeoffset](7) NOT NULL,
	[AlarmDateEnd] [datetimeoffset](7) NOT NULL,
	[AlarmSeverity] [int] NOT NULL,
	[AlarmCount] [int] NOT NULL,
	[AreaId] [int] NULL,
	[SiteId] [int] NULL,
	[SiteAlarmId] [int] NULL,
	[UserIdAck] [int] NULL,
	[UserIdReport] [int] NULL,
	[AlarmType] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[SourceId2] [int] NOT NULL,
	[SourceId3] [int] NOT NULL,
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
	[WaitAcknowledge] [bit] NOT NULL,
	[WaitReport] [bit] NOT NULL,
	[ParentId] [int] NULL,
	[AlarmCustomId] [int] NULL,
	[SourceId4] [int] NULL,
	[AlarmParameters] [nvarchar](max) NULL,
	[WaitProcess] [bit] NULL,
	[UserNameAck] [nvarchar](max) NULL,
	[UserNameReport] [nvarchar](max) NULL,
	[SourceName] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Alarms] PRIMARY KEY CLUSTERED 
(
	[AlarmId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
 CONSTRAINT [PK_dbo.CurrentStates] PRIMARY KEY CLUSTERED 
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
