"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EventRow = exports.EventSubType = exports.EventType = void 0;
const dateHelper_1 = require("../common/dateHelper");
const helper_1 = require("../common/helper");
const variableRow_1 = require("./variableRow");
const reader_1 = require("./reader");
var EventType;
(function (EventType) {
    EventType[EventType["None"] = 0] = "None";
    EventType[EventType["Server"] = 100] = "Server";
    EventType[EventType["Protocol"] = 200] = "Protocol";
    EventType[EventType["User"] = 300] = "User";
    EventType[EventType["Variable"] = 400] = "Variable";
    EventType[EventType["VariableReaderGranted"] = 500] = "VariableReaderGranted";
    EventType[EventType["VariableReaderDenied"] = 550] = "VariableReaderDenied";
    EventType[EventType["AlarmChanged"] = 600] = "AlarmChanged";
    EventType[EventType["AreaOrGroupChanged"] = 700] = "AreaOrGroupChanged";
})(EventType = exports.EventType || (exports.EventType = {}));
var EventSubType;
(function (EventSubType) {
    /// <summary>
    /// None
    /// </summary>
    EventSubType[EventSubType["None"] = 0] = "None";
    /// <summary>
    /// ServerMin
    /// </summary>
    EventSubType[EventSubType["ServerMin"] = 100] = "ServerMin";
    /// <summary>
    /// Server info
    /// </summary>
    EventSubType[EventSubType["ServerInfo"] = 101] = "ServerInfo";
    /// <summary>
    /// Server error
    /// </summary>
    EventSubType[EventSubType["ServerError"] = 102] = "ServerError";
    /// <summary>
    /// Server store error
    /// </summary>
    EventSubType[EventSubType["ServerStoreError"] = 103] = "ServerStoreError";
    /// <summary>
    /// Server starting 
    /// </summary>
    EventSubType[EventSubType["ServerStarting"] = 109] = "ServerStarting";
    /// <summary>
    /// Server started (activation)
    /// </summary>
    EventSubType[EventSubType["ServerStarted"] = 110] = "ServerStarted";
    /// <summary>
    /// Server stop (desactivation)
    /// </summary>
    EventSubType[EventSubType["ServerStop"] = 111] = "ServerStop";
    /// <summary>
    /// Server close
    /// </summary>
    EventSubType[EventSubType["ServerClose"] = 112] = "ServerClose";
    /// <summary>
    /// Server synchronizing
    /// </summary>
    EventSubType[EventSubType["ServerSynchronizing"] = 113] = "ServerSynchronizing";
    /// <summary>
    /// Server initializing
    /// </summary>
    EventSubType[EventSubType["ServerInitializing"] = 114] = "ServerInitializing";
    /// <summary>
    /// Server execute process
    /// </summary>
    EventSubType[EventSubType["ServerExecuteProcess"] = 115] = "ServerExecuteProcess";
    /// <summary>
    /// Server invoke method
    /// </summary>
    EventSubType[EventSubType["ServerInvokeMethod"] = 116] = "ServerInvokeMethod";
    /// <summary>
    /// Server save configuration
    /// </summary>
    EventSubType[EventSubType["ServerSaveConfiguration"] = 117] = "ServerSaveConfiguration";
    /// <summary>
    /// Server import table
    /// </summary>
    EventSubType[EventSubType["ServerImportTable"] = 118] = "ServerImportTable";
    /// <summary>
    /// Server export table
    /// </summary>
    EventSubType[EventSubType["ServerExportTable"] = 119] = "ServerExportTable";
    /// <summary>
    /// Server user lock
    /// </summary>
    EventSubType[EventSubType["ServerUserLock"] = 120] = "ServerUserLock";
    /// <summary>
    /// Occurs when an email message has been sent by the mailing engine.
    /// </summary>
    EventSubType[EventSubType["ServerSendMail"] = 121] = "ServerSendMail";
    /// <summary>
    /// Occurs when a SMS has been sent by the mailing engine.
    /// </summary>
    EventSubType[EventSubType["ServerSendSMS"] = 122] = "ServerSendSMS";
    EventSubType[EventSubType["RedundantChecking"] = 123] = "RedundantChecking";
    EventSubType[EventSubType["ServerActiveLicenseByCode"] = 130] = "ServerActiveLicenseByCode";
    EventSubType[EventSubType["ServerCheckDatabase"] = 131] = "ServerCheckDatabase";
    EventSubType[EventSubType["ServerRestoreConfiguration"] = 132] = "ServerRestoreConfiguration";
    EventSubType[EventSubType["ServerRestoreHisto"] = 133] = "ServerRestoreHisto";
    EventSubType[EventSubType["ServerSaveHisto"] = 134] = "ServerSaveHisto";
    EventSubType[EventSubType["ServerRebuildIndexes"] = 135] = "ServerRebuildIndexes";
    EventSubType[EventSubType["ServerReorganizeIndexes"] = 136] = "ServerReorganizeIndexes";
    EventSubType[EventSubType["ServerExecuteSql"] = 137] = "ServerExecuteSql";
    EventSubType[EventSubType["ServerCyclePresets"] = 138] = "ServerCyclePresets";
    EventSubType[EventSubType["ServerScenario"] = 139] = "ServerScenario";
    /// <summary>
    /// Occurs when a client connects to the server.
    /// </summary>
    EventSubType[EventSubType["ClientOpen"] = 151] = "ClientOpen";
    /// <summary>
    ///  Occurs when a client disconnects itself from the server.
    /// </summary>
    EventSubType[EventSubType["ClientClose"] = 152] = "ClientClose";
    /// <summary>
    /// Occurs when no keep-alive was received from the client during the time-out period.
    /// </summary>
    EventSubType[EventSubType["ClientTimeout"] = 153] = "ClientTimeout";
    /// <summary>
    /// Client send to
    /// </summary>
    EventSubType[EventSubType["ClientCommand"] = 154] = "ClientCommand";
    /// <summary>
    /// ServerMax
    /// </summary>
    EventSubType[EventSubType["ServerMax"] = 199] = "ServerMax";
    /// <summary>
    /// ProtocolMin
    /// </summary>
    EventSubType[EventSubType["ProtocolMin"] = 200] = "ProtocolMin";
    /// <summary>
    /// Protocol start
    /// </summary>
    EventSubType[EventSubType["ProtocolStart"] = 201] = "ProtocolStart";
    /// <summary>
    /// Protocol stop
    /// </summary>
    EventSubType[EventSubType["ProtocolStop"] = 202] = "ProtocolStop";
    /// <summary>
    /// Protocol restart
    /// </summary>
    EventSubType[EventSubType["ProtocolRestart"] = 203] = "ProtocolRestart";
    /// <summary>
    /// Protocol login
    /// </summary>
    EventSubType[EventSubType["ProtocolLogin"] = 210] = "ProtocolLogin";
    /// <summary>
    /// Protocol synchronise
    /// </summary>
    EventSubType[EventSubType["ProtocolSynchronise"] = 211] = "ProtocolSynchronise";
    /// <summary>
    /// Protocol logout
    /// </summary>
    EventSubType[EventSubType["ProtocolLogout"] = 212] = "ProtocolLogout";
    /// <summary>
    /// Protocol error
    /// </summary>
    EventSubType[EventSubType["ProtocolError"] = 215] = "ProtocolError";
    /// <summary>
    /// Protocol info
    /// </summary>
    EventSubType[EventSubType["ProtocolInfo"] = 216] = "ProtocolInfo";
    /// <summary>
    /// Protocol refresh
    /// </summary>
    EventSubType[EventSubType["ProtocolRefresh"] = 217] = "ProtocolRefresh";
    /// <summary>
    /// Protocol command download
    /// </summary>
    EventSubType[EventSubType["ProtocolDownload"] = 218] = "ProtocolDownload";
    /// <summary>
    /// Protocol download starting
    /// </summary>
    EventSubType[EventSubType["ProtocolDownloadStarting"] = 219] = "ProtocolDownloadStarting";
    /// <summary>
    /// Protocol download complete
    /// </summary>
    EventSubType[EventSubType["ProtocolDownloadComplete"] = 220] = "ProtocolDownloadComplete";
    /// <summary>
    /// Protocol start by user
    /// </summary>
    EventSubType[EventSubType["ProtocolStartByUser"] = 251] = "ProtocolStartByUser";
    /// <summary>
    /// Protocol stop by user
    /// </summary>
    EventSubType[EventSubType["ProtocolStopByUser"] = 252] = "ProtocolStopByUser";
    /// <summary>
    /// ProtocolMax
    /// </summary>
    EventSubType[EventSubType["ProtocolMax"] = 299] = "ProtocolMax";
    /// <summary>
    /// UserMin
    /// </summary>
    EventSubType[EventSubType["UserMin"] = 300] = "UserMin";
    /// <summary>
    /// User login
    /// </summary>
    EventSubType[EventSubType["UserLogin"] = 301] = "UserLogin";
    /// <summary>
    /// User logout
    /// </summary>
    EventSubType[EventSubType["UserLogout"] = 302] = "UserLogout";
    /// <summary>
    /// User alarm acknowledge
    /// </summary>
    EventSubType[EventSubType["UserAckAlarm"] = 303] = "UserAckAlarm";
    /// <summary>
    /// User alarm report
    /// </summary>
    EventSubType[EventSubType["UserReportAlarm"] = 304] = "UserReportAlarm";
    /// <summary>
    /// User alarm delete
    /// </summary>
    EventSubType[EventSubType["UserCancelAlarm"] = 305] = "UserCancelAlarm";
    /// <summary>
    /// User mask alarm
    /// </summary>
    EventSubType[EventSubType["UserMaskAlarm"] = 306] = "UserMaskAlarm";
    /// <summary>
    /// User resume alarm
    /// </summary>
    EventSubType[EventSubType["UserResumeAlarm"] = 307] = "UserResumeAlarm";
    /// <summary>
    /// User update alarm
    /// </summary>
    EventSubType[EventSubType["UserUpdateAlarm"] = 308] = "UserUpdateAlarm";
    /// <summary>
    /// User waiting alarm
    /// </summary>
    EventSubType[EventSubType["UserWaitingAlarm"] = 309] = "UserWaitingAlarm";
    /// <summary>
    /// User lock variable
    /// </summary>
    EventSubType[EventSubType["UserLockVar"] = 310] = "UserLockVar";
    /// <summary>
    /// User stop
    /// </summary>
    EventSubType[EventSubType["UserStop"] = 311] = "UserStop";
    /// <summary>
    /// User driver download
    /// </summary>
    EventSubType[EventSubType["UserDriverDownload"] = 312] = "UserDriverDownload";
    /// <summary>
    /// User driver upload
    /// </summary>
    EventSubType[EventSubType["UserDriverUpload"] = 313] = "UserDriverUpload";
    /// <summary>
    /// User driver refresh
    /// </summary>
    EventSubType[EventSubType["UserDriverRefresh"] = 314] = "UserDriverRefresh";
    /// <summary>
    /// User driver execute
    /// </summary>
    EventSubType[EventSubType["UserDriverExecute"] = 315] = "UserDriverExecute";
    /// <summary>
    /// User remove all alarms
    /// </summary>
    EventSubType[EventSubType["UserRemoveAllAlarms"] = 316] = "UserRemoveAllAlarms";
    /// <summary>
    /// User update event
    /// </summary>
    EventSubType[EventSubType["UserUpdateEvent"] = 317] = "UserUpdateEvent";
    /// <summary>
    /// User alarm transfer
    /// </summary>
    EventSubType[EventSubType["UserTransferAlarm"] = 318] = "UserTransferAlarm";
    /// <summary>
    /// User login error
    /// </summary>
    EventSubType[EventSubType["UserLoginError"] = 319] = "UserLoginError";
    /// <summary>
    /// User change password
    /// </summary>
    EventSubType[EventSubType["UserChangePassword"] = 320] = "UserChangePassword";
    /// <summary>
    /// User message creation
    /// </summary>
    EventSubType[EventSubType["UserMessageCreate"] = 321] = "UserMessageCreate";
    /// <summary>
    /// User message alarm creation
    /// </summary>
    EventSubType[EventSubType["UserMessageAlarmCreate"] = 322] = "UserMessageAlarmCreate";
    /// <summary>
    /// User message update
    /// </summary>
    EventSubType[EventSubType["UserMessageUpdate"] = 323] = "UserMessageUpdate";
    /// <summary>
    /// User video
    /// </summary>
    EventSubType[EventSubType["UserVideo"] = 330] = "UserVideo";
    /// <summary>
    /// User video
    /// </summary>
    EventSubType[EventSubType["UserClientClose"] = 350] = "UserClientClose";
    /// <summary>
    /// User print
    /// </summary>
    EventSubType[EventSubType["UserPrint"] = 351] = "UserPrint";
    /// <summary>
    /// User export
    /// </summary>
    EventSubType[EventSubType["UserExport"] = 352] = "UserExport";
    /// <summary>
    /// User read history
    /// </summary>
    EventSubType[EventSubType["UserReadHisto"] = 353] = "UserReadHisto";
    /// <summary>
    /// UserMax
    /// </summary>
    EventSubType[EventSubType["UserMax"] = 399] = "UserMax";
    /// <summary>
    /// VarUpdateMin
    /// </summary>
    EventSubType[EventSubType["VarUpdateMin"] = 400] = "VarUpdateMin";
    /// <summary>
    /// Variable update
    /// </summary>
    EventSubType[EventSubType["VarUpdate"] = 401] = "VarUpdate";
    /// <summary>
    /// Variable update by user
    /// </summary>
    EventSubType[EventSubType["VarUpdateByUser"] = 402] = "VarUpdateByUser";
    /// <summary>
    /// Variable update alarm
    /// </summary>
    EventSubType[EventSubType["VarUpdateAlarm"] = 403] = "VarUpdateAlarm";
    /// <summary>
    /// Variable update alarm
    /// </summary>
    EventSubType[EventSubType["VarUpdateAlarmByUser"] = 404] = "VarUpdateAlarmByUser";
    /// <summary>
    /// VarUpdateMax
    /// </summary>
    EventSubType[EventSubType["VarUpdateMax"] = 499] = "VarUpdateMax";
    // réservé aux événements contrôle d'accès
    // cf. enum ReaderStatus pour avoir le détail des valeurs
    /// <summary>
    /// VarReaderMin
    /// </summary>
    EventSubType[EventSubType["VarReaderMin"] = 500] = "VarReaderMin";
    /// <summary>
    /// VarReaderNok
    /// </summary>
    EventSubType[EventSubType["VarReaderNok"] = 550] = "VarReaderNok";
    /// <summary>
    /// VarReaderMax
    /// </summary>
    EventSubType[EventSubType["VarReaderMax"] = 599] = "VarReaderMax";
    /// <summary>
    /// Group changed
    /// </summary>
    EventSubType[EventSubType["GroupChanged"] = 701] = "GroupChanged";
    /// <summary>
    /// Area changed
    /// </summary>
    EventSubType[EventSubType["AreaChanged"] = 702] = "AreaChanged";
})(EventSubType = exports.EventSubType || (exports.EventSubType = {}));
class EventRow {
    constructor(event) {
        Object.assign(this, event);
        if (this.Info)
            this.Info = this.Info.substring(0, 50);
    }
    get type() {
        return EventType[this.TypeInt];
    }
    ;
    get subType() {
        return EventSubType[this.SubTypeInt];
    }
    ;
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
    get groups() {
        return this.GroupNames !== '' && this.GroupNames ? this.GroupNames.split(',') : [];
    }
    get attachments() {
        return this.Attachments !== '' && this.Attachments ? this.Attachments.split('\n') : [];
    }
    getParameter(paramName) {
        if (this.Parameters) {
            const param = this.Parameters.find(p => p.Name === paramName);
            if (param)
                return param.Value;
        }
        return null;
    }
    static parseFromVarState(varState) {
        var _a, _b, _c, _d, _e;
        const evt = new EventRow({
            Description: varState.OriginDesc,
            Date: varState.Date,
            Info: varState.Info,
            TypeInt: EventType.Variable,
            SubTypeInt: varState.OriginId > 0 ? EventSubType.VarUpdateByUser : EventSubType.VarUpdate,
            OriginId: varState.OriginId,
            OriginDesc: varState.OriginDesc,
            StateDesc: varState.valueDesc,
            Parameters: varState.Parameters,
            Source: varState.Name,
            SourceId: varState.Id,
            Quality: varState.Quality,
            AreaId: (_a = varState.Row) === null || _a === void 0 ? void 0 : _a.AreaId,
            AreaDesc: (_b = varState.Row) === null || _b === void 0 ? void 0 : _b.AreaDescription,
            GroupNames: (_c = varState.Row) === null || _c === void 0 ? void 0 : _c.FullGroupNames.join(','),
            GroupDesc: (_d = varState.Row) === null || _d === void 0 ? void 0 : _d.FullGroupDescriptions,
        });
        if (((_e = varState.Row) === null || _e === void 0 ? void 0 : _e.Type) === variableRow_1.VariableType.Reader) {
            const v = helper_1.Helper.parseInt(varState.Value);
            evt.TypeInt = v < reader_1.ReaderStatus.BadgeNok ? EventType.VariableReaderGranted : EventType.VariableReaderDenied;
            evt.SubTypeInt = EventSubType.VarReaderMin + v;
            evt.StateDesc = 'ReaderStatus.' + reader_1.ReaderStatus[v];
        }
        evt.Photo = evt.getParameter("Photo") || evt.getParameter("@Photo");
        if (varState.Row) {
            evt.Video = varState.Row.FullVideoLink;
            evt.Description = varState.Row.FullDescription;
        }
        return evt;
    }
}
exports.EventRow = EventRow;
