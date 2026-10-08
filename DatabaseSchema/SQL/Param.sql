/****** AppVision 4.10 - database Param - tables scripted from a 4.10 database on 2026-10-08 ******/
/****** Documentation only: this script DROPS and recreates the database. Never run it on an AppVision server. ******/

USE master;
GO

DROP DATABASE Param
GO

CREATE DATABASE Param
GO

ALTER DATABASE Param SET RECOVERY SIMPLE
GO

USE Param
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Actions](
	[ActionName] [nvarchar](128) NOT NULL,
	[ActionDescription] [nvarchar](max) NULL,
	[ActionContext] [int] NOT NULL,
	[ActionType] [int] NOT NULL,
	[ActionIsActive] [bit] NOT NULL,
	[ActionParameterDescriptions] [nvarchar](max) NULL,
	[ActionReturnDescription] [nvarchar](max) NULL,
	[ActionScript] [nvarchar](max) NULL,
	[ActionExtension] [nvarchar](max) NULL,
	[ActionUrl] [nvarchar](max) NULL,
	[ActionCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Actions] PRIMARY KEY CLUSTERED 
(
	[ActionName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Areas](
	[AreaId] [int] NOT NULL,
	[AreaName] [nvarchar](max) NULL,
	[AreaDescription] [nvarchar](max) NULL,
	[AreaIcon] [nvarchar](max) NULL,
	[AreaParameters] [nvarchar](max) NULL,
	[AreaExtension] [nvarchar](max) NULL,
	[AreaUrl] [nvarchar](max) NULL,
	[AreaCustomExtension] [nvarchar](max) NULL,
	[AreaCustomField1] [nvarchar](max) NULL,
	[AreaCustomField2] [nvarchar](max) NULL,
	[AreaCustomField3] [nvarchar](max) NULL,
	[AreaCustomField4] [nvarchar](max) NULL,
	[AreaCustomField5] [nvarchar](max) NULL,
	[AreaSynoptic] [nvarchar](max) NULL,
	[AreaSound] [nvarchar](max) NULL,
	[AreaSiteInfo] [nvarchar](max) NULL,
	[AreaColorInfo] [nvarchar](max) NULL,
	[AreaGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Areas] PRIMARY KEY CLUSTERED 
(
	[AreaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileInfos](
	[FileInfoId] [int] NOT NULL,
	[FileInfoName] [nvarchar](128) NOT NULL,
	[FileInfoDescription] [nvarchar](max) NULL,
	[FileInfoType] [int] NOT NULL,
	[FileInfoContext] [int] NOT NULL,
	[FileInfoSiteId] [int] NULL,
	[FileInfoParameters] [nvarchar](max) NULL,
	[FileInfoExtension] [nvarchar](max) NULL,
	[FileInfoUrl] [nvarchar](max) NULL,
	[FileInfoCustomExtension] [nvarchar](max) NULL,
	[FileInfoGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_FileInfos] PRIMARY KEY CLUSTERED 
(
	[FileInfoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Groups](
	[GroupId] [int] NOT NULL,
	[GroupName] [nvarchar](max) NULL,
	[GroupDescription] [nvarchar](max) NULL,
	[GroupIcon] [nvarchar](max) NULL,
	[GroupParameters] [nvarchar](max) NULL,
	[GroupExtension] [nvarchar](max) NULL,
	[GroupUrl] [nvarchar](max) NULL,
	[GroupCustomExtension] [nvarchar](max) NULL,
	[GroupCustomField1] [nvarchar](max) NULL,
	[GroupCustomField2] [nvarchar](max) NULL,
	[GroupCustomField3] [nvarchar](max) NULL,
	[GroupCustomField4] [nvarchar](max) NULL,
	[GroupCustomField5] [nvarchar](max) NULL,
	[GroupSynoptic] [nvarchar](max) NULL,
	[GroupSound] [nvarchar](max) NULL,
	[GroupColorInfo] [nvarchar](max) NULL,
	[GroupGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Groups] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupVariables](
	[VariableId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.GroupVariables] PRIMARY KEY CLUSTERED 
(
	[VariableId] ASC,
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_GroupId] ON [dbo].[GroupVariables]
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_VariableId] ON [dbo].[GroupVariables]
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Holidays](
	[HolidayId] [int] NOT NULL,
	[HolidayDate] [datetime] NOT NULL,
	[HolidayType] [int] NOT NULL,
	[HolidayIsAnnual] [bit] NOT NULL,
	[HolidayCulture] [nvarchar](max) NULL,
	[HolidayExtension] [nvarchar](max) NULL,
	[HolidayUrl] [nvarchar](max) NULL,
	[HolidayCustomExtension] [nvarchar](max) NULL,
	[HolidayGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Holidays] PRIMARY KEY CLUSTERED 
(
	[HolidayId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Instructions](
	[InstructionId] [int] NOT NULL,
	[InstructionName] [nvarchar](max) NULL,
	[InstructionValue] [nvarchar](max) NULL,
	[InstructionType] [int] NOT NULL,
	[InstructionNeedValidation] [int] NOT NULL,
	[InstructionParameters] [nvarchar](max) NULL,
	[InstructionExtension] [nvarchar](max) NULL,
	[InstructionUrl] [nvarchar](max) NULL,
	[InstructionCustomExtension] [nvarchar](max) NULL,
	[InstructionGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Instructions] PRIMARY KEY CLUSTERED 
(
	[InstructionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstructionVariables](
	[VariableId] [int] NOT NULL,
	[InstructionId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.InstructionVariables] PRIMARY KEY CLUSTERED 
(
	[VariableId] ASC,
	[InstructionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_InstructionId] ON [dbo].[InstructionVariables]
(
	[InstructionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_VariableId] ON [dbo].[InstructionVariables]
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Links](
	[LinkId] [int] NOT NULL,
	[LinkName] [nvarchar](max) NULL,
	[LinkDescription] [nvarchar](max) NULL,
	[LinkType] [int] NOT NULL,
	[LinkDate] [datetime] NULL,
	[LinkIsActive] [bit] NOT NULL,
	[LinkSource] [nvarchar](max) NULL,
	[LinkAction] [nvarchar](max) NULL,
	[LinkParameters] [nvarchar](max) NULL,
	[LinkExtension] [nvarchar](max) NULL,
	[LinkUrl] [nvarchar](max) NULL,
	[LinkCustomExtension] [nvarchar](max) NULL,
	[FilterServerName] [nvarchar](256) NULL,
	[LinkGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Links] PRIMARY KEY CLUSTERED 
(
	[LinkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mailings](
	[MailingId] [int] NOT NULL,
	[MailingName] [nvarchar](max) NULL,
	[MailingDescription] [nvarchar](max) NULL,
	[MailingType] [int] NOT NULL,
	[MailingTo] [nvarchar](max) NULL,
	[MailingFilterTimeTableId] [int] NULL,
	[MailingFilters] [nvarchar](max) NULL,
	[MailingFilterSeverity] [nvarchar](max) NULL,
	[MailingConditions] [nvarchar](max) NULL,
	[MailingGrouping] [nvarchar](max) NULL,
	[MailingIsActive] [bit] NOT NULL,
	[MailingTempoRetry] [int] NOT NULL,
	[MailingMaxRetry] [int] NOT NULL,
	[MailingParameters] [nvarchar](max) NULL,
	[MailingExtension] [nvarchar](max) NULL,
	[MailingUrl] [nvarchar](max) NULL,
	[MailingCustomExtension] [nvarchar](max) NULL,
	[MailingModelEmail] [nvarchar](max) NULL,
	[MailingModelSMS] [nvarchar](max) NULL,
	[MailingModelOther] [nvarchar](max) NULL,
	[MailingGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Mailings] PRIMARY KEY CLUSTERED 
(
	[MailingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_MailingFilterTimeTableId] ON [dbo].[Mailings]
(
	[MailingFilterTimeTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Options](
	[OptionName] [nvarchar](128) NOT NULL,
	[OptionValue] [nvarchar](max) NULL,
	[OptionType] [int] NOT NULL,
	[OptionExtension] [nvarchar](max) NULL,
	[OptionUrl] [nvarchar](max) NULL,
	[OptionCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Options] PRIMARY KEY CLUSTERED 
(
	[OptionName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Passwords](
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Type] [int] NULL,
	[Extension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Messages] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfileUsers](
	[ProfileId] [int] NOT NULL,
	[ProfileName] [nvarchar](max) NULL,
	[ProfileDescription] [nvarchar](max) NULL,
	[ProfileIsAdmin] [bit] NOT NULL,
	[ProfilePriorityLevel] [int] NOT NULL,
	[ProfileOptions] [nvarchar](max) NULL,
	[ProfileRoles] [nvarchar](max) NULL,
	[ProfileFilters] [nvarchar](max) NULL,
	[ProfileFiltersWrite] [nvarchar](max) NULL,
	[ProfileFiltersFile] [nvarchar](max) NULL,
	[ProfileWebHomePage] [nvarchar](max) NULL,
	[ProfileWindowsHomePage] [nvarchar](max) NULL,
	[ProfileExtension] [nvarchar](max) NULL,
	[ProfileParameters] [nvarchar](max) NULL,
	[ProfileUrl] [nvarchar](max) NULL,
	[ProfileCustomExtension] [nvarchar](max) NULL,
	[ProfileGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.ProfileUsers] PRIMARY KEY CLUSTERED 
(
	[ProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Protocols](
	[ProtocolId] [int] NOT NULL,
	[ProtocolName] [nvarchar](max) NULL,
	[ProtocolDescription] [nvarchar](max) NULL,
	[ProtocolAddress] [nvarchar](max) NULL,
	[ProtocolParameters] [nvarchar](max) NULL,
	[ProtocolExecutable] [nvarchar](max) NULL,
	[ProtocolIsExclude] [bit] NOT NULL,
	[ProtocolAutoStart] [bit] NOT NULL,
	[ProtocolLogLevel] [int] NOT NULL,
	[ProtocolLogFilename] [nvarchar](max) NULL,
	[ProtocolMaxRetry] [int] NOT NULL,
	[ProtocolTimeout] [int] NOT NULL,
	[ProtocolExtension] [nvarchar](max) NULL,
	[ProtocolUrl] [nvarchar](max) NULL,
	[ProtocolCustomExtension] [nvarchar](max) NULL,
	[ProtocolGuid] [nvarchar](64) NULL,
	[ProtocolIcon] [nvarchar](max) NULL,
	[ProtocolPassword] [nvarchar](256) NULL,
	[ProtocolTempoStart] [int] NULL,
 CONSTRAINT [PK_dbo.Protocols] PRIMARY KEY CLUSTERED 
(
	[ProtocolId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Scenarios](
	[ScenarioId] [int] NOT NULL,
	[Activated] [bit] NOT NULL,
	[StoreEvent] [bit] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Execution] [nvarchar](max) NULL,
	[Designer] [nvarchar](max) NULL,
	[TimeTableVarName] [nvarchar](max) NULL,
	[Triggers] [nvarchar](max) NULL,
	[Unique] [bit] NOT NULL,
	[Extension] [nvarchar](max) NULL,
	[FilterServerName] [nvarchar](256) NULL,
	[ScenarioGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.Scenarios] PRIMARY KEY CLUSTERED 
(
	[ScenarioId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[Scenarios] ADD  DEFAULT ((0)) FOR [Unique]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TextReports](
	[TextReportId] [int] NOT NULL,
	[TextReportValue] [nvarchar](max) NULL,
	[TextReportType] [int] NOT NULL,
	[TextReportExtension] [nvarchar](max) NULL,
	[TextReportUrl] [nvarchar](max) NULL,
	[TextReportCustomExtension] [nvarchar](max) NULL,
	[TextReportName] [nvarchar](256) NULL,
	[TextReportGuid] [nvarchar](64) NULL,
 CONSTRAINT [PK_dbo.TextReports] PRIMARY KEY CLUSTERED 
(
	[TextReportId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] NOT NULL,
	[UserName] [nvarchar](max) NULL,
	[UserLastName] [nvarchar](max) NULL,
	[UserFirstName] [nvarchar](max) NULL,
	[UserPassword] [nvarchar](max) NULL,
	[ProfileId] [int] NOT NULL,
	[UserIsLocked] [bit] NOT NULL,
	[UserLanguage] [nvarchar](max) NULL,
	[UserCulture] [nvarchar](max) NULL,
	[UserEmail] [nvarchar](max) NULL,
	[UserPhone] [nvarchar](max) NULL,
	[UserNotes] [nvarchar](max) NULL,
	[UserFilterClients] [nvarchar](max) NULL,
	[UserFilterTimeTableId] [int] NULL,
	[UserValidityStart] [datetime] NOT NULL,
	[UserValidityEnd] [datetime] NOT NULL,
	[UserCreateDate] [datetime] NOT NULL,
	[UserExtension] [nvarchar](max) NULL,
	[UserParameters] [nvarchar](max) NULL,
	[UserOptions] [nvarchar](max) NULL,
	[UserRoles] [nvarchar](max) NULL,
	[UserUrl] [nvarchar](max) NULL,
	[UserCustomExtension] [nvarchar](max) NULL,
	[UserPasswordDate] [datetime] NULL,
	[UserGuid] [nvarchar](64) NULL,
	[UserPicture] [nvarchar](max) NULL,
	[UserSecurity] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_ProfileId] ON [dbo].[Users]
(
	[ProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variables](
	[VariableId] [int] NOT NULL,
	[VariableName] [nvarchar](max) NOT NULL,
	[VariableDescription] [nvarchar](max) NULL,
	[VariableType] [int] NOT NULL,
	[SiteId] [int] NULL,
	[AreaId] [int] NULL,
	[VariableStateDescs] [nvarchar](max) NULL,
	[VariableSendMailing] [bit] NOT NULL,
	[VariableStoreEvent] [bit] NOT NULL,
	[VariableStoreValue] [bit] NOT NULL,
	[VariableStoreTempo] [int] NOT NULL,
	[VariableStoreDeltaMin] [float] NOT NULL,
	[VariableStoreDeltaMax] [float] NOT NULL,
	[VariableIsExclude] [bit] NOT NULL,
	[VariableStateType] [int] NOT NULL,
	[VariableAddress] [nvarchar](max) NULL,
	[VariableIsAlarm] [bit] NOT NULL,
	[VariableInstructions] [nvarchar](max) NULL,
	[VariableIsAlarmMultiple] [bit] NOT NULL,
	[VariableAlarmNeedAcknowledge] [bit] NOT NULL,
	[VariableAlarmResetOnAcknowledge] [bit] NOT NULL,
	[VariableAlarmNeedReport] [bit] NOT NULL,
	[VariableAlarmSeverity] [int] NOT NULL,
	[VariableAlarmSeverity2] [int] NOT NULL,
	[VariableAlarmTempo] [int] NOT NULL,
	[VariableAlarmMinValue] [float] NOT NULL,
	[VariableAlarmMaxValue] [float] NOT NULL,
	[VariableAlarmMinValue2] [float] NOT NULL,
	[VariableAlarmMaxValue2] [float] NOT NULL,
	[VariableSynoptic] [nvarchar](max) NULL,
	[VariableSound] [nvarchar](max) NULL,
	[VariableGraphMin] [float] NOT NULL,
	[VariableGraphMax] [float] NOT NULL,
	[VariableGraphScale] [float] NOT NULL,
	[VariableVideoLink] [nvarchar](max) NULL,
	[VariableParameters] [nvarchar](max) NULL,
	[VariableSymbol] [nvarchar](max) NULL,
	[VariableIcon] [nvarchar](max) NULL,
	[VariableNoVisible] [bit] NOT NULL,
	[VariableExpression] [nvarchar](max) NULL,
	[VariableScript] [nvarchar](max) NULL,
	[VariablePosition] [nvarchar](max) NULL,
	[VariableNotes] [nvarchar](max) NULL,
	[VariableBufferLength] [int] NOT NULL,
	[VariableBufferType] [int] NOT NULL,
	[VariableExtension] [nvarchar](max) NULL,
	[VariableUrl] [nvarchar](max) NULL,
	[VariableCustomExtension] [nvarchar](max) NULL,
	[VariableCustomField1] [nvarchar](max) NULL,
	[VariableCustomField2] [nvarchar](max) NULL,
	[VariableCustomField3] [nvarchar](max) NULL,
	[VariableCustomField4] [nvarchar](max) NULL,
	[VariableCustomField5] [nvarchar](max) NULL,
	[VariableGuid] [nvarchar](64) NULL,
	[VariableLinks] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Variables] PRIMARY KEY CLUSTERED 
(
	[VariableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AreaId] ON [dbo].[Variables]
(
	[AreaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE [dbo].[GroupVariables]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupVariables_dbo.Groups_GroupId] FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupVariables] CHECK CONSTRAINT [FK_dbo.GroupVariables_dbo.Groups_GroupId]
GO
ALTER TABLE [dbo].[GroupVariables]  WITH CHECK ADD  CONSTRAINT [FK_dbo.GroupVariables_dbo.Variables_VariableId] FOREIGN KEY([VariableId])
REFERENCES [dbo].[Variables] ([VariableId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GroupVariables] CHECK CONSTRAINT [FK_dbo.GroupVariables_dbo.Variables_VariableId]
GO
ALTER TABLE [dbo].[InstructionVariables]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InstructionVariables_dbo.Instructions_InstructionId] FOREIGN KEY([InstructionId])
REFERENCES [dbo].[Instructions] ([InstructionId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InstructionVariables] CHECK CONSTRAINT [FK_dbo.InstructionVariables_dbo.Instructions_InstructionId]
GO
ALTER TABLE [dbo].[InstructionVariables]  WITH CHECK ADD  CONSTRAINT [FK_dbo.InstructionVariables_dbo.Variables_VariableId] FOREIGN KEY([VariableId])
REFERENCES [dbo].[Variables] ([VariableId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InstructionVariables] CHECK CONSTRAINT [FK_dbo.InstructionVariables_dbo.Variables_VariableId]
GO
ALTER TABLE [dbo].[Mailings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Mailings_dbo.Variables_MailingFilterTimeTableId] FOREIGN KEY([MailingFilterTimeTableId])
REFERENCES [dbo].[Variables] ([VariableId])
GO
ALTER TABLE [dbo].[Mailings] CHECK CONSTRAINT [FK_dbo.Mailings_dbo.Variables_MailingFilterTimeTableId]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Users_dbo.ProfileUsers_ProfileId] FOREIGN KEY([ProfileId])
REFERENCES [dbo].[ProfileUsers] ([ProfileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_dbo.Users_dbo.ProfileUsers_ProfileId]
GO
ALTER TABLE [dbo].[Variables]  WITH NOCHECK ADD  CONSTRAINT [FK_dbo.Variables_dbo.Areas_AreaId] FOREIGN KEY([AreaId])
REFERENCES [dbo].[Areas] ([AreaId])
GO
ALTER TABLE [dbo].[Variables] CHECK CONSTRAINT [FK_dbo.Variables_dbo.Areas_AreaId]
GO
