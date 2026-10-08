/****** AppVision 4.10 - database AppControl - tables scripted from a 4.10 database on 2026-10-08 ******/
/****** Documentation only: this script DROPS and recreates the database. Never run it on an AppVision server. ******/

USE master;
GO

DROP DATABASE AppControl
GO

CREATE DATABASE AppControl
GO

ALTER DATABASE AppControl SET RECOVERY SIMPLE
GO

USE AppControl
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_AccessProfileRights](
	[AccessProfileId] [int] NOT NULL,
	[ReaderId] [int] NOT NULL,
	[AccessProfileRightCrisisLevel] [int] NOT NULL,
	[TimetableId] [int] NULL,
	[AccessProfileRightValidityStart] [datetime] NULL,
	[AccessProfileRightValidityEnd] [datetime] NULL,
	[AccessProfileRightValidityInfo] [nvarchar](max) NULL,
	[AccessProfileRightExtension] [nvarchar](max) NULL,
	[AccessProfileRightCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_AccessProfileRights] PRIMARY KEY CLUSTERED 
(
	[AccessProfileId] ASC,
	[ReaderId] ASC,
	[AccessProfileRightCrisisLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AccessProfileId] ON [dbo].[Apc_AccessProfileRights]
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_AccessProfiles](
	[AccessProfileId] [int] NOT NULL,
	[AccessProfileName] [nvarchar](max) NULL,
	[AccessProfileDescription] [nvarchar](max) NULL,
	[AccessProfileExtension] [nvarchar](max) NULL,
	[AccessProfileCustomExtension] [nvarchar](max) NULL,
	[AccessProfileParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_AccessProfiles] PRIMARY KEY CLUSTERED 
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_AccessProfileZoneRights](
	[AccessProfileId] [int] NOT NULL,
	[ZoneId] [int] NOT NULL,
	[AccessProfileZoneRightCrisisLevel] [int] NOT NULL,
	[TimetableId] [int] NULL,
	[AccessProfileZoneRightValidityStart] [datetime] NULL,
	[AccessProfileZoneRightValidityEnd] [datetime] NULL,
	[AccessProfileZoneRightValidityInfo] [nvarchar](max) NULL,
	[AccessProfileZoneRightExtension] [nvarchar](max) NULL,
	[AccessProfileZoneRightCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_AccessProfileZoneRights] PRIMARY KEY CLUSTERED 
(
	[AccessProfileId] ASC,
	[ZoneId] ASC,
	[AccessProfileZoneRightCrisisLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AccessProfileId] ON [dbo].[Apc_AccessProfileZoneRights]
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ZoneId] ON [dbo].[Apc_AccessProfileZoneRights]
(
	[ZoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Badges](
	[BadgeCode] [nvarchar](128) NOT NULL,
	[BadgeFormat] [nvarchar](128) NOT NULL,
	[BadgeTypeId] [int] NOT NULL,
	[PersonId] [int] NULL,
	[BadgeStatus] [int] NOT NULL,
	[BadgeInfo] [nvarchar](max) NULL,
	[BadgeExtension] [nvarchar](max) NULL,
	[BadgeExtId] [int] NULL,
	[BadgeCustomExtension] [nvarchar](max) NULL,
	[BadgeParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_Badges] PRIMARY KEY CLUSTERED 
(
	[BadgeCode] ASC,
	[BadgeFormat] ASC,
	[BadgeTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_BadgeTypeId] ON [dbo].[Apc_Badges]
(
	[BadgeTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[Apc_Badges]
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_BadgeTypes](
	[BadgeTypeId] [int] NOT NULL,
	[BadgeTypeDescription] [nvarchar](max) NULL,
	[BadgeTypeLength] [int] NOT NULL,
	[BadgeTypeParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_BadgeTypes] PRIMARY KEY CLUSTERED 
(
	[BadgeTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Categories](
	[CategoryId] [int] NOT NULL,
	[CategoryName] [nvarchar](max) NULL,
	[CategoryDescription] [nvarchar](max) NULL,
	[CategoryBadgeDesign] [nvarchar](max) NULL,
	[CategoryExtension] [nvarchar](max) NULL,
	[CategoryCustomExtension] [nvarchar](max) NULL,
	[AccessProfileId] [int] NULL,
	[CategoryIsVisitor] [bit] NOT NULL,
	[CategoryDefaultValidityPeriod] [nvarchar](max) NULL,
	[CategoryParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AccessProfileId] ON [dbo].[Apc_Categories]
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Departments](
	[DepartmentId] [int] NOT NULL,
	[DepartmentName] [nvarchar](max) NULL,
	[DepartmentDescription] [nvarchar](max) NULL,
	[DepartmentBadgeDesign] [nvarchar](max) NULL,
	[DepartmentExtension] [nvarchar](max) NULL,
	[DepartmentCustomExtension] [nvarchar](max) NULL,
	[AccessProfileId] [int] NULL,
	[DepartmentParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_Departments] PRIMARY KEY CLUSTERED 
(
	[DepartmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AccessProfileId] ON [dbo].[Apc_Departments]
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Location](
	[PersonId] [int] IDENTITY(1,1) NOT NULL,
	[ReaderId] [int] NOT NULL,
	[LocationDate] [datetimeoffset](7) NOT NULL,
	[ReaderIdOk] [int] NOT NULL,
	[LocationDateOk] [datetimeoffset](7) NOT NULL,
	[LocationExtension] [nvarchar](max) NULL,
	[LocationCustomExtension] [nvarchar](max) NULL,
	[Person_Id] [int] NULL,
 CONSTRAINT [PK_dbo.Apc_Location] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_Person_Id] ON [dbo].[Apc_Location]
(
	[Person_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_People](
	[PersonId] [int] NOT NULL,
	[PersonFirstName] [nvarchar](max) NULL,
	[PersonLastName] [nvarchar](max) NULL,
	[CategoryId] [int] NULL,
	[DepartmentId] [int] NULL,
	[PersonCompanyId] [nvarchar](max) NULL,
	[PersonAddress] [nvarchar](max) NULL,
	[PersonPhone] [nvarchar](max) NULL,
	[PersonEmail] [nvarchar](max) NULL,
	[PersonInfo1] [nvarchar](max) NULL,
	[PersonInfo2] [nvarchar](max) NULL,
	[PersonInfo3] [nvarchar](max) NULL,
	[PersonInfo4] [nvarchar](max) NULL,
	[PersonInfo5] [nvarchar](max) NULL,
	[PersonInfo6] [nvarchar](max) NULL,
	[PersonInfo7] [nvarchar](max) NULL,
	[PersonInfo8] [nvarchar](max) NULL,
	[PersonInfo9] [nvarchar](max) NULL,
	[PersonInfo10] [nvarchar](max) NULL,
	[PersonPhoto] [nvarchar](max) NULL,
	[PersonNotes] [nvarchar](max) NULL,
	[PersonParentId] [int] NULL,
	[PersonLanguage] [nvarchar](max) NULL,
	[PersonCulture] [nvarchar](max) NULL,
	[PersonValidityStart] [datetime] NOT NULL,
	[PersonValidityEnd] [datetime] NOT NULL,
	[PersonValidityInfo] [nvarchar](max) NULL,
	[PersonActive] [bit] NOT NULL,
	[PersonPass] [bit] NOT NULL,
	[PersonAlarm] [bit] NOT NULL,
	[PersonAntipassback] [bit] NOT NULL,
	[PersonEscortRequired] [bit] NOT NULL,
	[PersonEscortId] [int] NULL,
	[PersonCrisisLevel] [int] NULL,
	[PersonPinCode] [nvarchar](max) NULL,
	[UserIdCreation] [int] NOT NULL,
	[UserIdModification] [int] NOT NULL,
	[PersonDateCreation] [datetime] NOT NULL,
	[PersonDateModification] [datetime] NOT NULL,
	[PersonExtension] [nvarchar](max) NULL,
	[PersonExtId1] [int] NULL,
	[PersonExtId2] [int] NULL,
	[PersonCustomExtension] [nvarchar](max) NULL,
	[PersonInfo11] [nvarchar](max) NULL,
	[PersonInfo12] [nvarchar](max) NULL,
	[PersonInfo13] [nvarchar](max) NULL,
	[PersonInfo14] [nvarchar](max) NULL,
	[PersonInfo15] [nvarchar](max) NULL,
	[PersonInfo16] [nvarchar](max) NULL,
	[PersonInfo17] [nvarchar](max) NULL,
	[PersonInfo18] [nvarchar](max) NULL,
	[PersonInfo19] [nvarchar](max) NULL,
	[PersonInfo20] [nvarchar](max) NULL,
	[PersonBadgeDesign] [nvarchar](max) NULL,
	[Attachments] [nvarchar](max) NULL,
	[PersonCompany] [nvarchar](max) NULL,
	[PersonIdNumber] [nvarchar](256) NULL,
 CONSTRAINT [PK_dbo.Apc_People] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_CategoryId] ON [dbo].[Apc_People]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_DepartmentId] ON [dbo].[Apc_People]
(
	[DepartmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonEscortId] ON [dbo].[Apc_People]
(
	[PersonEscortId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
CREATE NONCLUSTERED INDEX [IX_PersonIdNumber] ON [dbo].[Apc_People]
(
	[PersonIdNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_PersonProfiles](
	[PersonId] [int] NOT NULL,
	[AccessProfileId] [int] NOT NULL,
	[PersonProfileExtension] [nvarchar](max) NULL,
	[PersonProfileCustomExtension] [nvarchar](max) NULL,
	[PersonProfileValidityStart] [datetime2](7) NULL,
	[PersonProfileValidityEnd] [datetime2](7) NULL,
 CONSTRAINT [PK_dbo.Apc_PersonProfiles] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_AccessProfileId] ON [dbo].[Apc_PersonProfiles]
(
	[AccessProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[Apc_PersonProfiles]
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_PersonRights](
	[PersonId] [int] NOT NULL,
	[ReaderId] [int] NOT NULL,
	[PersonRightCrisisLevel] [int] NOT NULL,
	[TimetableId] [int] NULL,
	[PersonRightValidityStart] [datetime] NULL,
	[PersonRightValidityEnd] [datetime] NULL,
	[PersonRightValidityInfo] [nvarchar](max) NULL,
	[PersonRightExtension] [nvarchar](max) NULL,
	[PersonRightCustomExtension] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_PersonRights] PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC,
	[ReaderId] ASC,
	[PersonRightCrisisLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[Apc_PersonRights]
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_PersonZones](
	[ZoneId] [int] NOT NULL,
	[PersonId] [int] NOT NULL,
	[PersonZoneDateInput] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_dbo.Apc_PersonZones] PRIMARY KEY CLUSTERED 
(
	[ZoneId] ASC,
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[Apc_PersonZones]
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ZoneId] ON [dbo].[Apc_PersonZones]
(
	[ZoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Visitors](
	[VisitorId] [int] NOT NULL,
	[VisitorFirstName] [nvarchar](max) NULL,
	[VisitorLastName] [nvarchar](max) NULL,
	[CategoryId] [int] NULL,
	[DepartmentId] [int] NULL,
	[VisitorCompany] [nvarchar](max) NULL,
	[VisitorCompanyId] [nvarchar](max) NULL,
	[VisitorReason] [nvarchar](max) NULL,
	[VisitorAddress] [nvarchar](max) NULL,
	[VisitorPhone] [nvarchar](max) NULL,
	[VisitorEmail] [nvarchar](max) NULL,
	[VisitorInfo1] [nvarchar](max) NULL,
	[VisitorInfo2] [nvarchar](max) NULL,
	[VisitorInfo3] [nvarchar](max) NULL,
	[VisitorInfo4] [nvarchar](max) NULL,
	[VisitorInfo5] [nvarchar](max) NULL,
	[VisitorInfo6] [nvarchar](max) NULL,
	[VisitorInfo7] [nvarchar](max) NULL,
	[VisitorInfo8] [nvarchar](max) NULL,
	[VisitorInfo9] [nvarchar](max) NULL,
	[VisitorInfo10] [nvarchar](max) NULL,
	[VisitorPhoto] [nvarchar](max) NULL,
	[VisitorLanguage] [nvarchar](max) NULL,
	[VisitorCulture] [nvarchar](max) NULL,
	[PersonId] [int] NULL,
	[PersonVisitedId] [int] NULL,
	[VisitorVisitedLocation] [nvarchar](max) NULL,
	[VisitorScan1] [nvarchar](max) NULL,
	[VisitorScan2] [nvarchar](max) NULL,
	[VisitorValidityStart] [datetime] NOT NULL,
	[VisitorValidityEnd] [datetime] NOT NULL,
	[VisitorValidityInfo] [nvarchar](max) NULL,
	[VisitorReceptionNotes] [nvarchar](max) NULL,
	[VisitorNotes] [nvarchar](max) NULL,
	[VisitorBadgeReturn] [nvarchar](max) NULL,
	[VisitorEscortRequired] [bit] NOT NULL,
	[UserIdCreation] [int] NOT NULL,
	[VisitorDateCreation] [datetime] NOT NULL,
	[UserIdValidation] [int] NOT NULL,
	[VisitorDateValidation] [datetime] NOT NULL,
	[UserIdModification] [int] NOT NULL,
	[VisitorDateModification] [datetime] NOT NULL,
	[VisitorStatus] [nvarchar](max) NULL,
	[VisitorExtension] [nvarchar](max) NULL,
	[VisitorExtId1] [int] NULL,
	[VisitorExtId2] [int] NULL,
	[VisitorCustomExtension] [nvarchar](max) NULL,
	[Attachments] [nvarchar](max) NULL,
	[VisitorIdNumber] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_Visitors] PRIMARY KEY CLUSTERED 
(
	[VisitorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_CategoryId] ON [dbo].[Apc_Visitors]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_DepartmentId] ON [dbo].[Apc_Visitors]
(
	[DepartmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[Apc_Visitors]
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PersonVisitedId] ON [dbo].[Apc_Visitors]
(
	[PersonVisitedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_ZoneReaderIns](
	[ZoneId] [int] NOT NULL,
	[ReaderId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Apc_ZoneReaderIns] PRIMARY KEY CLUSTERED 
(
	[ZoneId] ASC,
	[ReaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_ZoneId] ON [dbo].[Apc_ZoneReaderIns]
(
	[ZoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_ZoneReaderOuts](
	[ZoneId] [int] NOT NULL,
	[ReaderId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Apc_ZoneReaderOuts] PRIMARY KEY CLUSTERED 
(
	[ZoneId] ASC,
	[ReaderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX [IX_ZoneId] ON [dbo].[Apc_ZoneReaderOuts]
(
	[ZoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Apc_Zones](
	[ZoneId] [int] NOT NULL,
	[ZoneName] [nvarchar](max) NULL,
	[ZoneDescription] [nvarchar](max) NULL,
	[ZoneCheckAntipassbackInput] [bit] NOT NULL,
	[ZoneCheckAntipassbackOutput] [bit] NOT NULL,
	[ZoneExtension] [nvarchar](max) NULL,
	[ZoneCustomExtension] [nvarchar](max) NULL,
	[ZoneParameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Apc_Zones] PRIMARY KEY CLUSTERED 
(
	[ZoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Apc_AccessProfileRights]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_AccessProfileRights_dbo.Apc_AccessProfiles_AccessProfileId] FOREIGN KEY([AccessProfileId])
REFERENCES [dbo].[Apc_AccessProfiles] ([AccessProfileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_AccessProfileRights] CHECK CONSTRAINT [FK_dbo.Apc_AccessProfileRights_dbo.Apc_AccessProfiles_AccessProfileId]
GO
ALTER TABLE [dbo].[Apc_AccessProfileZoneRights]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_AccessProfileZoneRights_dbo.Apc_AccessProfiles_AccessProfileId] FOREIGN KEY([AccessProfileId])
REFERENCES [dbo].[Apc_AccessProfiles] ([AccessProfileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_AccessProfileZoneRights] CHECK CONSTRAINT [FK_dbo.Apc_AccessProfileZoneRights_dbo.Apc_AccessProfiles_AccessProfileId]
GO
ALTER TABLE [dbo].[Apc_AccessProfileZoneRights]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_AccessProfileZoneRights_dbo.Apc_Zones_ZoneId] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[Apc_Zones] ([ZoneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_AccessProfileZoneRights] CHECK CONSTRAINT [FK_dbo.Apc_AccessProfileZoneRights_dbo.Apc_Zones_ZoneId]
GO
ALTER TABLE [dbo].[Apc_Badges]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Badges_dbo.Apc_BadgeTypes_BadgeTypeId] FOREIGN KEY([BadgeTypeId])
REFERENCES [dbo].[Apc_BadgeTypes] ([BadgeTypeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_Badges] CHECK CONSTRAINT [FK_dbo.Apc_Badges_dbo.Apc_BadgeTypes_BadgeTypeId]
GO
ALTER TABLE [dbo].[Apc_Badges]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Badges_dbo.Apc_People_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Apc_People] ([PersonId])
GO
ALTER TABLE [dbo].[Apc_Badges] CHECK CONSTRAINT [FK_dbo.Apc_Badges_dbo.Apc_People_PersonId]
GO
ALTER TABLE [dbo].[Apc_Categories]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Categories_dbo.Apc_AccessProfiles_AccessProfileId] FOREIGN KEY([AccessProfileId])
REFERENCES [dbo].[Apc_AccessProfiles] ([AccessProfileId])
GO
ALTER TABLE [dbo].[Apc_Categories] CHECK CONSTRAINT [FK_dbo.Apc_Categories_dbo.Apc_AccessProfiles_AccessProfileId]
GO
ALTER TABLE [dbo].[Apc_Departments]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Departments_dbo.Apc_AccessProfiles_AccessProfileId] FOREIGN KEY([AccessProfileId])
REFERENCES [dbo].[Apc_AccessProfiles] ([AccessProfileId])
GO
ALTER TABLE [dbo].[Apc_Departments] CHECK CONSTRAINT [FK_dbo.Apc_Departments_dbo.Apc_AccessProfiles_AccessProfileId]
GO
ALTER TABLE [dbo].[Apc_Location]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Location_dbo.Apc_People_Person_Id] FOREIGN KEY([Person_Id])
REFERENCES [dbo].[Apc_People] ([PersonId])
GO
ALTER TABLE [dbo].[Apc_Location] CHECK CONSTRAINT [FK_dbo.Apc_Location_dbo.Apc_People_Person_Id]
GO
ALTER TABLE [dbo].[Apc_People]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Apc_Categories] ([CategoryId])
GO
ALTER TABLE [dbo].[Apc_People] CHECK CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_Categories_CategoryId]
GO
ALTER TABLE [dbo].[Apc_People]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_Departments_DepartmentId] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Apc_Departments] ([DepartmentId])
GO
ALTER TABLE [dbo].[Apc_People] CHECK CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_Departments_DepartmentId]
GO
ALTER TABLE [dbo].[Apc_People]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_People_PersonEscortId] FOREIGN KEY([PersonEscortId])
REFERENCES [dbo].[Apc_People] ([PersonId])
GO
ALTER TABLE [dbo].[Apc_People] CHECK CONSTRAINT [FK_dbo.Apc_People_dbo.Apc_People_PersonEscortId]
GO
ALTER TABLE [dbo].[Apc_PersonProfiles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_PersonProfiles_dbo.Apc_AccessProfiles_AccessProfileId] FOREIGN KEY([AccessProfileId])
REFERENCES [dbo].[Apc_AccessProfiles] ([AccessProfileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_PersonProfiles] CHECK CONSTRAINT [FK_dbo.Apc_PersonProfiles_dbo.Apc_AccessProfiles_AccessProfileId]
GO
ALTER TABLE [dbo].[Apc_PersonProfiles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_PersonProfiles_dbo.Apc_People_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Apc_People] ([PersonId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_PersonProfiles] CHECK CONSTRAINT [FK_dbo.Apc_PersonProfiles_dbo.Apc_People_PersonId]
GO
ALTER TABLE [dbo].[Apc_PersonRights]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_PersonRights_dbo.Apc_People_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Apc_People] ([PersonId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_PersonRights] CHECK CONSTRAINT [FK_dbo.Apc_PersonRights_dbo.Apc_People_PersonId]
GO
ALTER TABLE [dbo].[Apc_PersonZones]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_PersonZones_dbo.Apc_People_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Apc_People] ([PersonId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_PersonZones] CHECK CONSTRAINT [FK_dbo.Apc_PersonZones_dbo.Apc_People_PersonId]
GO
ALTER TABLE [dbo].[Apc_PersonZones]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_PersonZones_dbo.Apc_Zones_ZoneId] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[Apc_Zones] ([ZoneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_PersonZones] CHECK CONSTRAINT [FK_dbo.Apc_PersonZones_dbo.Apc_Zones_ZoneId]
GO
ALTER TABLE [dbo].[Apc_Visitors]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Apc_Categories] ([CategoryId])
GO
ALTER TABLE [dbo].[Apc_Visitors] CHECK CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_Categories_CategoryId]
GO
ALTER TABLE [dbo].[Apc_Visitors]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_Departments_DepartmentId] FOREIGN KEY([DepartmentId])
REFERENCES [dbo].[Apc_Departments] ([DepartmentId])
GO
ALTER TABLE [dbo].[Apc_Visitors] CHECK CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_Departments_DepartmentId]
GO
ALTER TABLE [dbo].[Apc_Visitors]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_People_PersonId] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Apc_People] ([PersonId])
GO
ALTER TABLE [dbo].[Apc_Visitors] CHECK CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_People_PersonId]
GO
ALTER TABLE [dbo].[Apc_Visitors]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_People_PersonVisitedId] FOREIGN KEY([PersonVisitedId])
REFERENCES [dbo].[Apc_People] ([PersonId])
GO
ALTER TABLE [dbo].[Apc_Visitors] CHECK CONSTRAINT [FK_dbo.Apc_Visitors_dbo.Apc_People_PersonVisitedId]
GO
ALTER TABLE [dbo].[Apc_ZoneReaderIns]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_ZoneReaderIns_dbo.Apc_Zones_ZoneId] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[Apc_Zones] ([ZoneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_ZoneReaderIns] CHECK CONSTRAINT [FK_dbo.Apc_ZoneReaderIns_dbo.Apc_Zones_ZoneId]
GO
ALTER TABLE [dbo].[Apc_ZoneReaderOuts]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Apc_ZoneReaderOuts_dbo.Apc_Zones_ZoneId] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[Apc_Zones] ([ZoneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Apc_ZoneReaderOuts] CHECK CONSTRAINT [FK_dbo.Apc_ZoneReaderOuts_dbo.Apc_Zones_ZoneId]
GO
