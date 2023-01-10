import { DateTimeOffset } from "./dateTimeOffset";
import { NameValue } from "./nameValue";
import { VariableState } from "./variableState";
export declare enum EventType {
    None = 0,
    Server = 100,
    Protocol = 200,
    User = 300,
    Variable = 400,
    VariableReaderGranted = 500,
    VariableReaderDenied = 550,
    AlarmChanged = 600,
    AreaOrGroupChanged = 700
}
export declare enum EventSubType {
    None = 0,
    ServerMin = 100,
    ServerInfo = 101,
    ServerError = 102,
    ServerStoreError = 103,
    ServerStarting = 109,
    ServerStarted = 110,
    ServerStop = 111,
    ServerClose = 112,
    ServerSynchronizing = 113,
    ServerInitializing = 114,
    ServerExecuteProcess = 115,
    ServerInvokeMethod = 116,
    ServerSaveConfiguration = 117,
    ServerImportTable = 118,
    ServerExportTable = 119,
    ServerUserLock = 120,
    ServerSendMail = 121,
    ServerSendSMS = 122,
    RedundantChecking = 123,
    ServerActiveLicenseByCode = 130,
    ServerCheckDatabase = 131,
    ServerRestoreConfiguration = 132,
    ServerRestoreHisto = 133,
    ServerSaveHisto = 134,
    ServerRebuildIndexes = 135,
    ServerReorganizeIndexes = 136,
    ServerExecuteSql = 137,
    ServerCyclePresets = 138,
    ServerScenario = 139,
    ClientOpen = 151,
    ClientClose = 152,
    ClientTimeout = 153,
    ClientCommand = 154,
    ServerMax = 199,
    ProtocolMin = 200,
    ProtocolStart = 201,
    ProtocolStop = 202,
    ProtocolRestart = 203,
    ProtocolLogin = 210,
    ProtocolSynchronise = 211,
    ProtocolLogout = 212,
    ProtocolError = 215,
    ProtocolInfo = 216,
    ProtocolRefresh = 217,
    ProtocolDownload = 218,
    ProtocolDownloadStarting = 219,
    ProtocolDownloadComplete = 220,
    ProtocolStartByUser = 251,
    ProtocolStopByUser = 252,
    ProtocolMax = 299,
    UserMin = 300,
    UserLogin = 301,
    UserLogout = 302,
    UserAckAlarm = 303,
    UserReportAlarm = 304,
    UserCancelAlarm = 305,
    UserMaskAlarm = 306,
    UserResumeAlarm = 307,
    UserUpdateAlarm = 308,
    UserWaitingAlarm = 309,
    UserLockVar = 310,
    UserStop = 311,
    UserDriverDownload = 312,
    UserDriverUpload = 313,
    UserDriverRefresh = 314,
    UserDriverExecute = 315,
    UserRemoveAllAlarms = 316,
    UserUpdateEvent = 317,
    UserTransferAlarm = 318,
    UserLoginError = 319,
    UserChangePassword = 320,
    UserMessageCreate = 321,
    UserMessageAlarmCreate = 322,
    UserMessageUpdate = 323,
    UserVideo = 330,
    UserClientClose = 350,
    UserPrint = 351,
    UserExport = 352,
    UserReadHisto = 353,
    UserMax = 399,
    VarUpdateMin = 400,
    VarUpdate = 401,
    VarUpdateByUser = 402,
    VarUpdateAlarm = 403,
    VarUpdateAlarmByUser = 404,
    VarUpdateMax = 499,
    VarReaderMin = 500,
    VarReaderNok = 550,
    VarReaderMax = 599,
    GroupChanged = 701,
    AreaChanged = 702
}
export declare class EventRow {
    constructor(event: Partial<EventRow>);
    Id: number;
    Info: string;
    Description: string;
    Attachments: string;
    AreaId: number;
    AreaDesc: string;
    GroupDesc: string;
    GroupNames: string;
    OriginId: number;
    OriginDesc: string;
    StateDesc: string;
    Parameters: NameValue[];
    Photo: string;
    Date: DateTimeOffset;
    TypeInt: number;
    SubTypeInt: number;
    Video: string;
    Quality: number;
    Source: string;
    SourceId: number;
    SourceId2: number;
    SourceId3: number;
    SourceId4: number;
    Value: number;
    ValueString: string;
    Tag: any;
    get type(): string;
    get subType(): string;
    get date(): Date;
    get groups(): string[];
    get attachments(): string[];
    getParameter(paramName: string): string;
    static parseFromVarState(varState: VariableState): EventRow;
}
