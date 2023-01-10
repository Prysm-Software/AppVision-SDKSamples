"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.AppServer = void 0;
const variableManager_1 = require("./managers/variableManager");
const groupManager_1 = require("./managers/groupManager");
const optionManager_1 = require("./managers/optionManager");
const userManager_1 = require("./managers/userManager");
const fileManager_1 = require("./managers/fileManager");
const eventManager_1 = require("./managers/eventManager");
const alarmManager_1 = require("./managers/alarmManager");
const operandManager_1 = require("./managers/operandManager");
const rtManager_1 = require("./managers/rtManager");
const protocolManager_1 = require("./managers/protocolManager");
const rxjs_1 = require("rxjs");
const scenarioManager_1 = require("./managers/scenarioManager");
const areaManager_1 = require("./managers/areaManager");
const eventRow_1 = require("./data/eventRow");
const alarmInfo_1 = require("./data/alarmInfo");
const variableState_1 = require("./data/variableState");
const variableRow_1 = require("./data/variableRow");
const areaRow_1 = require("./data/areaRow");
const areaState_1 = require("./data/areaState");
const groupRow_1 = require("./data/groupRow");
const groupState_1 = require("./data/groupState");
const protocolState_1 = require("./data/protocolState");
const scenarioState_1 = require("./data/scenarioState");
const fileInfoRow_1 = require("./data/fileInfoRow");
const commandManager_1 = require("./managers/commandManager");
const videoManager_1 = require("./managers/videoManager");
const httpService_1 = require("./services/httpService");
const environmentManager_1 = require("./managers/environmentManager");
const base64Helper_1 = require("./common/base64Helper");
const categoryManager_1 = require("./managers/categoryManager");
const peopleManager_1 = require("./managers/peopleManager");
const departmentManager_1 = require("./managers/departmentManager");
const accessProfileManager_1 = require("./managers/accessProfileManager");
const badgeManager_1 = require("./managers/badgeManager");
const rightManager_1 = require("./managers/rightManager");
const badgeTypeManager_1 = require("./managers/badgeTypeManager");
const zoneManager_1 = require("./managers/zoneManager");
const optionRow_1 = require("./data/optionRow");
const personZoneManager_1 = require("./managers/personZoneManager");
const serverState_1 = require("./data/serverState");
const holidayManager_1 = require("./managers/holidayManager");
const userMessageManager_1 = require("./managers/userMessageManager");
const visitorManager_1 = require("./managers/visitorManager");
const licenseManager_1 = require("./managers/licenseManager");
const chatManager_1 = require("./managers/chatManager");
const chat_1 = require("./data/chat");
const userState_1 = require("./data/userState");
const userRow_1 = require("./data/userRow");
const deviceManager_1 = require("./managers/deviceManager");
const rowCustomColorManager_1 = require("./managers/rowCustomColorManager");
const overlayManager_1 = require("./managers/overlayManager");
const overlayItem_1 = require("./data/overlayItem");
/**
 * Object to connect to an appvision server
 */
class AppServer {
    constructor() {
        this.tempoKeepalive = 2000;
        this.keepAliveId = -1;
        this.error = new rxjs_1.Subject();
        this.connectionLost = new rxjs_1.Subject();
        this.newState = new rxjs_1.Subject();
    }
    /**
     * Current state of the server
     */
    get state() { return this.lastState; }
    ;
    /**
     * Tell if the appServer object is connected or not
     */
    get isConnected() { return this.connected; }
    ;
    /**
     * Login informations
     * @param options
     */
    login(options) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            // Clear old use
            this.dispose();
            let username = options.username.toUpperCase();
            this.http = new httpService_1.HttpService(options.host);
            const openExt = [
                { Name: "clientProductDescription", Value: options.productName },
                { Name: "clientProductName", Value: options.productName },
                { Name: "clientProductCompany", Value: options.productCompany },
                { Name: "clientProductVersion", Value: options.applicationVersion },
                { Name: "clientProcessName", Value: navigator.userAgent },
                { Name: "noCookie", Value: true }
            ];
            if (options.datas)
                for (const key of Object.getOwnPropertyNames(options.datas))
                    openExt.push({ Name: key, Value: (_a = options.datas[key]) === null || _a === void 0 ? void 0 : _a.toString() });
            // if (clientContext) {
            //     openExt.push({ Name: "clientContext", Value: clientContext });
            // }
            let resp = yield this.http.post("AppVisionService.svc/OpenExt", JSON.stringify(openExt));
            const sessionId = yield resp.json();
            this.http.addGlobalHeaders("SessionID", sessionId);
            let headers = { 'Authorization': 'Basic ' + base64Helper_1.Base64Helper.encode(username + ':' + options.password) };
            // SSO "Authorization: Negotiate" handled by browser
            if (username === "@SINGLESIGNON") {
                headers = null;
            }
            resp = yield this.http.post(`AppVisionService.svc/Login`, JSON.stringify([{ Name: "username", Value: username }, { Name: "password", Value: options.password }]), "POST", headers);
            // register for all server events (filter = All)
            yield this.http.post('AppVisionService.svc/AddFilterNotifications', '"All"');
            yield this.http.get('AppVisionService.svc/StartNotifications?send=false');
            this.areaManager = new areaManager_1.AreaManager(this.http);
            yield this.areaManager.loadInternal();
            this.groupManager = new groupManager_1.GroupManager(this.http);
            yield this.groupManager.loadInternal();
            this.variableManager = new variableManager_1.VariableManager(this.http);
            yield this.variableManager.loadInternal();
            this.userManager = new userManager_1.UserManager(this.http);
            yield this.userManager.loadInternal();
            this.optionManager = new optionManager_1.OptionManager(this.http);
            yield this.optionManager.loadInternal(this.userManager);
            this.fileManager = new fileManager_1.FileManager(this.http);
            yield this.fileManager.loadInternal();
            this.eventManager = new eventManager_1.EventManager(this.http);
            this.alarmManager = new alarmManager_1.AlarmManager(this.http);
            yield this.alarmManager.loadInternal();
            this.protocolManager = new protocolManager_1.ProtocolManager(this.http);
            yield this.protocolManager.loadInternal();
            this.scenarioManager = new scenarioManager_1.ScenarioManager(this.http);
            yield this.scenarioManager.loadInternal();
            this.environmentManager = new environmentManager_1.EnvironmentManager(this.http, this.fileManager, this.optionManager);
            yield this.environmentManager.loadInternal();
            this.licenseManager = new licenseManager_1.LicenseManager(this.http);
            yield this.licenseManager.loadInternal();
            this.overlayManager = new overlayManager_1.OverlayManager(this.http);
            yield this.overlayManager.loadInternal();
            this.operandManager = new operandManager_1.OperandManager(this.variableManager, this.areaManager, this.alarmManager, this.groupManager, this.userManager, this.protocolManager);
            this.commandManager = new commandManager_1.CommandManager();
            this.videoManager = new videoManager_1.VideoManager(this.http, this.variableManager);
            this.holidayManager = new holidayManager_1.HolidayManager(this.http);
            this.userMessageManager = new userMessageManager_1.UserMessageManager(this.http, this.optionManager);
            this.rtManager = new rtManager_1.RTManager(this.http);
            this.chatManager = new chatManager_1.ChatManager(this.http, this.fileManager);
            this.deviceManager = new deviceManager_1.DeviceManager(this.http);
            this.customColorManager = new rowCustomColorManager_1.RowCustomColorManager(this.optionManager, this.areaManager, this.alarmManager);
            // Access control odata
            this.httpOData = new httpService_1.HttpService(options.host);
            let password = options.password;
            if (username === "@SINGLESIGNON") {
                username = this.userManager.currentUser.Name;
                password = "@SingleSignOn";
            }
            this.httpOData.addGlobalHeaders("Authorization", "Basic " + base64Helper_1.Base64Helper.encode(username + ':' + password));
            this.visitorManager = new visitorManager_1.VisitorManager(this.httpOData);
            this.categoryManager = new categoryManager_1.CategoryManager(this.httpOData);
            this.departmentManager = new departmentManager_1.DepartmentManager(this.httpOData);
            this.rightManager = new rightManager_1.RightManager(this.httpOData);
            this.badgesManager = new badgeManager_1.BadgeManager(this.httpOData);
            this.accessProfileManager = new accessProfileManager_1.AccessProfileManager(this.httpOData, this.rightManager);
            this.badgeTypeManager = new badgeTypeManager_1.BadgeTypeManager(this.httpOData, this.rightManager);
            this.zoneManager = new zoneManager_1.ZoneManager(this.httpOData);
            this.personZoneManager = new personZoneManager_1.PersonZoneManager(this.httpOData);
            this.peopleManager = new peopleManager_1.PeopleManager(this.httpOData);
            this.connected = true;
            this.keepAliveId = setTimeout(() => this.keepAlive(), this.tempoKeepalive);
        });
    }
    /**
     * Logout
     */
    logout() {
        return __awaiter(this, void 0, void 0, function* () {
            this.connected = false;
            clearTimeout(this.keepAliveId);
            this.dispose();
            try {
                yield this.http.get("AppVisionService.svc/Close");
            }
            catch (_a) { }
        });
    }
    keepAlive() {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                const resp = yield this.http.get('AppVisionService.svc/KeepAlive');
                const notifCount = yield resp.json();
                if (!this.connected)
                    return;
                if (notifCount < 0)
                    throw new Error("Unknown session id");
                if (notifCount > 0) {
                    const notifsResp = yield this.http.get('AppVisionService.svc/GetNotifications?count=100');
                    // Keep connection with parsing failed
                    try {
                        const json = yield notifsResp.json();
                        if (!this.connected)
                            return;
                        const events = [];
                        for (const n of json) {
                            if (!Array.isArray(n.Data)) {
                                events.push(n.Data);
                            }
                            else {
                                for (const d of n.Data) {
                                    events.push(d);
                                }
                            }
                        }
                        if (events.length > 0)
                            this.handleNewEvents(events);
                    }
                    catch (error) {
                        console.error("[AppServer] - Failed parse notifications.", error);
                        this.error.next("Error parse notif resp : " + error.toString());
                    }
                }
            }
            catch (err) {
                if (!this.connected)
                    return;
                console.error("[AppServer] - Connection lost with appServer, keepalive failed", err);
                this.connected = false;
                this.connectionLost.next(err);
                return;
            }
            this.keepAliveId = setTimeout(() => this.keepAlive(), this.tempoKeepalive);
        });
    }
    handleNewEvents(events) {
        const eventsRows = [];
        const alarmInfos = [];
        const variableStates = [];
        const variableRows = [];
        const areaRows = [];
        const areaStates = [];
        const groupRows = [];
        const groupStates = [];
        const protocolStates = [];
        const scenarioStates = [];
        const fileInfos = [];
        const options = [];
        const chatMessages = [];
        const talks = [];
        const userRows = [];
        const overlays = [];
        let userState = null;
        let state = null;
        events.forEach(event => {
            try {
                if (event.__type !== undefined) {
                    switch (event.__type) {
                        case 'ServerState:#Prysm.AppVision.Data':
                            state = new serverState_1.ServerState(event);
                            break;
                        case 'AlarmInfo:#Prysm.AppVision.Data':
                            alarmInfos.push(new alarmInfo_1.AlarmInfo(event));
                            break;
                        case 'EventRow:#Prysm.AppVision.Data':
                            eventsRows.push(new eventRow_1.EventRow(event));
                            break;
                        case 'VariableState:#Prysm.AppVision.Data':
                            variableStates.push(new variableState_1.VariableState(event));
                            break;
                        case 'VariableRow:#Prysm.AppVision.Data':
                            variableRows.push(new variableRow_1.VariableRow(event));
                            break;
                        case 'AreaRow:#Prysm.AppVision.Data':
                            areaRows.push(new areaRow_1.AreaRow(event));
                            break;
                        case 'AreaState:#Prysm.AppVision.Data':
                            areaStates.push(new areaState_1.AreaState(event));
                            break;
                        case 'GroupRow:#Prysm.AppVision.Data':
                            groupRows.push(new groupRow_1.GroupRow(event));
                            break;
                        case 'GroupState:#Prysm.AppVision.Data':
                            groupStates.push(new groupState_1.GroupState(event));
                            break;
                        case 'ProtocolState:#Prysm.AppVision.Data':
                            protocolStates.push(new protocolState_1.ProtocolState(event));
                            break;
                        case 'ScenarioState:#Prysm.AppVision.Data':
                            scenarioStates.push(new scenarioState_1.ScenarioState(event));
                            break;
                        case 'FileInfoRow:#Prysm.AppVision.Data':
                            fileInfos.push(new fileInfoRow_1.FileInfoRow(event));
                            break;
                        case 'OptionRow:#Prysm.AppVision.Data':
                            options.push(new optionRow_1.OptionRow(event));
                            break;
                        case 'ChatMessageRow:#Prysm.AppVision.Data':
                            chatMessages.push(new chat_1.ChatMessage(event));
                            break;
                        case 'Talk:#Prysm.AppVision.Data':
                            talks.push(new chat_1.Talk(event));
                            break;
                        case 'UserState:#Prysm.AppVision.Data':
                            userState = new userState_1.UserState(event);
                            break;
                        case 'UserRow:#Prysm.AppVision.Data':
                            userRows.push(new userRow_1.UserRow(event));
                            break;
                        case 'OverlayItem:#Prysm.AppVision.Data':
                            overlays.push(new overlayItem_1.OverlayItem(event));
                            break;
                    }
                }
                else if (typeof event === "string" && event.startsWith("CommandClient;")) {
                    console.log("[AppServer] - Command receive : " + event);
                    this.commandManager.updateCommand(event);
                }
            }
            catch (err) {
                console.error("[AppServer] - Error reading event : " + err.toString());
                this.error.next("Error reading event : " + err.toString());
            }
        });
        if (variableRows.length > 0) {
            this.variableManager.updateRowsInternal(variableRows);
        }
        if (variableStates.length > 0) {
            this.variableManager.updateStatesInternal(variableStates);
            variableStates.forEach(vs => eventsRows.push(eventRow_1.EventRow.parseFromVarState(vs)));
        }
        if (eventsRows.length > 0) {
            this.eventManager.updateEventsInternal(eventsRows);
        }
        if (alarmInfos.length > 0) {
            this.alarmManager.updateAlarmsInternal(alarmInfos);
        }
        if (areaRows.length > 0) {
            this.areaManager.updateRowsInternal(areaRows);
        }
        if (areaStates.length > 0) {
            this.areaManager.updateStatesInternal(areaStates);
        }
        if (groupRows.length > 0) {
            this.groupManager.updateRowsInternal(groupRows);
        }
        if (groupStates.length > 0) {
            this.groupManager.updateStatesInternal(groupStates);
        }
        if (protocolStates.length > 0) {
            this.protocolManager.updateStatesInternal(protocolStates);
        }
        if (scenarioStates.length > 0) {
            this.scenarioManager.updateStatesInternal(scenarioStates);
        }
        if (fileInfos.length > 0) {
            this.fileManager.updateRowsInternal(fileInfos);
        }
        if (options.length > 0) {
            this.optionManager.updateRowsInternal(options);
        }
        if (chatMessages.length > 0) {
            this.chatManager.updateMessagesInternal(chatMessages);
        }
        if (talks.length > 0) {
            this.chatManager.updateTalksInternal(talks);
        }
        if (userRows.length > 0) {
            this.userManager.updateRowsInternal(userRows);
        }
        if (overlays.length > 0) {
            this.overlayManager.updateInternal(overlays);
        }
        if (state) {
            this.lastState = state;
            this.newState.next(state);
        }
        if (userState) {
            this.userManager.updateStateInternal(userState);
        }
    }
    dispose() {
        if (this.connected)
            this.connected = false;
        if (this.keepAliveId > 0)
            clearTimeout(this.keepAliveId);
        this.alarmManager = null;
        this.optionManager = null;
        this.userManager = null;
        this.variableManager = null;
        this.groupManager = null;
        this.areaManager = null;
        this.fileManager = null;
        this.eventManager = null;
        this.rtManager = null;
        this.operandManager = null;
        this.protocolManager = null;
        this.scenarioManager = null;
        this.commandManager = null;
        this.videoManager = null;
        this.environmentManager = null;
        this.peopleManager = null;
        this.categoryManager = null;
        this.departmentManager = null;
        this.accessProfileManager = null;
        this.badgesManager = null;
        this.rightManager = null;
        this.badgeTypeManager = null;
        this.chatManager = null;
        this.deviceManager = null;
        this.http = null;
        this.httpOData = null;
        this.lastState = null;
        this.keepAliveId = -1;
        this.error = new rxjs_1.Subject();
        this.connectionLost = new rxjs_1.Subject();
        this.newState = new rxjs_1.Subject();
    }
}
exports.AppServer = AppServer;
