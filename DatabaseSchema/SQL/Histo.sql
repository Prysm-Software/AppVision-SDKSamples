/****** AppVision 4.10 - database Histo - tables scripted from a 4.10 database on 2026-10-08 ******/
/****** Documentation only: this script DROPS and recreates the database. Never run it on an AppVision server. ******/

USE master;
GO

DROP DATABASE Histo
GO

CREATE DATABASE Histo
GO

ALTER DATABASE Histo SET RECOVERY SIMPLE
GO

USE Histo
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
	[AlarmGuid] [nvarchar](256) NULL,
	[AlarmDateClose] [datetimeoffset](7) NULL,
	[UserNameClose] [nvarchar](64) NULL,
	[ReadyToClose] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.Alarms] PRIMARY KEY CLUSTERED 
(
	[AlarmId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByAreaId] ON [dbo].[Alarms]
(
	[AreaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByDateReceive] ON [dbo].[Alarms]
(
	[AlarmDateReceive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByDateStart] ON [dbo].[Alarms]
(
	[AlarmDateStart] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByDateWrite] ON [dbo].[Alarms]
(
	[DateWrite] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByParentId] ON [dbo].[Alarms]
(
	[ParentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IdxAlarmsByServerName] ON [dbo].[Alarms]
(
	[ServerName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsBySourceId] ON [dbo].[Alarms]
(
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsBySourceId2] ON [dbo].[Alarms]
(
	[SourceId2] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsBySourceId3] ON [dbo].[Alarms]
(
	[SourceId3] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxAlarmsBySourceId4] ON [dbo].[Alarms]
(
	[SourceId4] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Alarms] ADD  CONSTRAINT [ReadyToClose_NotNull]  DEFAULT ((0)) FOR [ReadyToClose]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatMessages](
	[Id] [bigint] NOT NULL,
	[TalkId] [bigint] NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[MessageType] [nvarchar](max) NULL,
	[Date] [datetimeoffset](7) NULL,
	[AuthorId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.ChatMessages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatTalkMembers](
	[Id] [bigint] NOT NULL,
	[TalkId] [bigint] NOT NULL,
	[UserId] [int] NOT NULL,
	[LastMessageRead] [bigint] NULL,
 CONSTRAINT [PK_dbo.ChatTalkMembers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatTalks](
	[Id] [bigint] NOT NULL,
	[CreatedDate] [datetimeoffset](7) NOT NULL,
	[CreatorId] [int] NOT NULL,
	[Title] [nvarchar](max) NULL,
	[Icon] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ChatTalks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[EventId] [bigint] NOT NULL,
	[EventDate] [datetimeoffset](7) NOT NULL,
	[EventDateReceive] [datetimeoffset](7) NOT NULL,
	[EventType] [int] NOT NULL,
	[EventSubType] [int] NOT NULL,
	[SourceId] [int] NOT NULL,
	[SourceId2] [int] NOT NULL,
	[SourceId3] [int] NOT NULL,
	[SourceId4] [int] NULL,
	[AlarmId] [bigint] NULL,
	[EventCustomId] [int] NULL,
	[AreaId] [int] NULL,
	[SiteId] [int] NULL,
	[SiteEventId] [int] NULL,
	[OriginId] [int] NOT NULL,
	[EventValue] [int] NOT NULL,
	[EventValueString] [nvarchar](max) NULL,
	[EventStateDesc] [nvarchar](max) NULL,
	[EventAreaDesc] [nvarchar](max) NULL,
	[EventDescription] [nvarchar](max) NULL,
	[EventInfo] [nvarchar](max) NULL,
	[EventOriginDesc] [nvarchar](max) NULL,
	[EventReport] [nvarchar](max) NULL,
	[EventGroupNames] [nvarchar](max) NULL,
	[EventGroupDesc] [nvarchar](max) NULL,
	[EventUrl] [nvarchar](max) NULL,
	[EventAttachments] [nvarchar](max) NULL,
	[EventPhoto] [nvarchar](max) NULL,
	[EventVideo] [nvarchar](max) NULL,
	[EventQuality] [int] NULL,
	[EventExtension] [nvarchar](max) NULL,
	[EventCustomExtension] [nvarchar](max) NULL,
	[EventParameters] [nvarchar](max) NULL,
	[SourceName] [nvarchar](max) NULL,
	[ServerName] [nvarchar](256) NULL,
	[DateWrite] [datetime] NULL,
	[EventGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Events] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IdxEventsByAlarmId] ON [dbo].[Events]
(
	[AlarmId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsByAreaId] ON [dbo].[Events]
(
	[AreaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsByDate] ON [dbo].[Events]
(
	[EventDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsByDateReceive] ON [dbo].[Events]
(
	[EventDateReceive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsByDateWrite] ON [dbo].[Events]
(
	[DateWrite] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IdxEventsByServerName] ON [dbo].[Events]
(
	[ServerName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsBySourceId] ON [dbo].[Events]
(
	[SourceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsBySourceId2] ON [dbo].[Events]
(
	[SourceId2] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsBySourceId3] ON [dbo].[Events]
(
	[SourceId3] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxEventsBySourceId4] ON [dbo].[Events]
(
	[SourceId4] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Modifs](
	[ModifId] [bigint] IDENTITY(1,1) NOT NULL,
	[ModifDate] [datetimeoffset](7) NOT NULL,
	[ModifType] [int] NOT NULL,
	[ModifTable] [nvarchar](max) NULL,
	[SourceId] [int] NOT NULL,
	[SourceId2] [int] NOT NULL,
	[SourceId3] [int] NOT NULL,
	[ModifDescription] [nvarchar](max) NULL,
	[ModifInfo] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[ModifExtension] [nvarchar](max) NULL,
	[ModifCustomExtension] [nvarchar](max) NULL,
	[SiteId] [int] NULL,
	[SiteModifId] [int] NULL,
	[ServerName] [nvarchar](256) NULL,
	[UserName] [nvarchar](256) NULL,
	[ModifGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Modifs] PRIMARY KEY CLUSTERED 
(
	[ModifId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IdxModifsByDate] ON [dbo].[Modifs]
(
	[ModifDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IdxModifsByServerName] ON [dbo].[Modifs]
(
	[ServerName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
CREATE TABLE [dbo].[UserMessages](
	[UserMessageDateCreate] [datetimeoffset](7) NOT NULL,
	[UserMessageDateClose] [datetimeoffset](7) NOT NULL,
	[AlarmId] [bigint] NULL,
	[AreaId] [int] NULL,
	[UserMessageAreaDesc] [nvarchar](max) NULL,
	[UserMessageDescription] [nvarchar](max) NULL,
	[UserMessageGroupNames] [nvarchar](max) NULL,
	[UserMessageGroupDesc] [nvarchar](max) NULL,
	[UserMessageAttachments] [nvarchar](max) NULL,
	[UserMessagePhoto] [nvarchar](max) NULL,
	[UserMessageVideo] [nvarchar](max) NULL,
	[UserIdCreate] [int] NULL,
	[UserIdClose] [int] NULL,
	[UserNameCreate] [nvarchar](max) NULL,
	[UserNameClose] [nvarchar](max) NULL,
	[UserMessageSeverity] [int] NOT NULL,
	[UserMessageStatus] [int] NOT NULL,
	[UserMessageTo] [nvarchar](max) NULL,
	[UserMessageCategory] [nvarchar](max) NULL,
	[UserMessageComment] [nvarchar](max) NULL,
	[UserMessageContentCategory] [nvarchar](max) NULL,
	[UserMessageContentReferences] [nvarchar](max) NULL,
	[UserMessageIsIncident] [bit] NOT NULL,
	[UserMessageExtension] [nvarchar](max) NULL,
	[UserMessageCustomExtension] [nvarchar](max) NULL,
	[UserMessageId] [bigint] NOT NULL,
	[ServerName] [nvarchar](256) NULL,
	[DateWrite] [datetime] NULL,
	[UserMessageGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.UserMessages] PRIMARY KEY CLUSTERED 
(
	[UserMessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Values](
	[ValueId] [bigint] IDENTITY(1,1) NOT NULL,
	[VariableId] [int] NOT NULL,
	[ValueDate] [datetimeoffset](7) NOT NULL,
	[ValueValue] [float] NOT NULL,
	[ValueQuality] [int] NULL,
	[SiteId] [int] NULL,
	[SiteValueId] [int] NULL,
	[ServerName] [nvarchar](256) NULL,
	[DateWrite] [datetime] NULL,
	[ValueGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Values] PRIMARY KEY CLUSTERED 
(
	[ValueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IdxValuesByDate] ON [dbo].[Values]
(
	[ValueDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxValuesByDateWrite] ON [dbo].[Values]
(
	[DateWrite] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IdxValuesByServerName] ON [dbo].[Values]
(
	[ServerName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IdxValuesByVariable] ON [dbo].[Values]
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ChatTalkMembers]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ChatTalkMembers] FOREIGN KEY([TalkId])
REFERENCES [dbo].[ChatTalks] ([Id])
GO
ALTER TABLE [dbo].[ChatTalkMembers] CHECK CONSTRAINT [FK_dbo.ChatTalkMembers]
GO
