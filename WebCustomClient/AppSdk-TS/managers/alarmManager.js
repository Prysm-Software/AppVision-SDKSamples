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
exports.AlarmManager = void 0;
const rxjs_1 = require("rxjs");
const alarmRow_1 = require("../data/alarmRow");
const alarmInfo_1 = require("../data/alarmInfo");
const eventRow_1 = require("../data/eventRow");
const alarmsHelper_1 = require("../common/alarmsHelper");
class AlarmManager {
    constructor(http) {
        this.http = http;
        this.alarmsChanged = new rxjs_1.Subject();
    }
    /**
     * Get current alarms
     * @param maxCount Max number of alarms wanted (default: 0 => return all)
     * @param filter Function to filter the alarms (default: null => no filter)
     * @returns Array of AlarmRow
     */
    getAlarms(maxCount = 0, filter = null) {
        if (filter == null)
            filter = () => true;
        if (maxCount > 0)
            return this.alarms.filter(filter).slice(0, maxCount).map(a => new alarmRow_1.AlarmRow(a));
        return this.alarms.filter(filter).map(a => new alarmRow_1.AlarmRow(a));
    }
    /**
     * Get an alarm by its Id
     * @param id Alarm id
     * @returns Alarm Row
     */
    getById(id) {
        return this.alarms.find(a => a.Id === id);
    }
    /**
     * Get current alarms by area
     * @param area Area filter
     * @returns Array of AlarmRow
     */
    getAlarmsByArea(area) {
        const arr = [];
        for (const al of this.alarms) {
            if (al.AreaId && area.isChild(al.AreaId)) {
                arr.push(al);
            }
        }
        return arr.map(a => new alarmRow_1.AlarmRow(a));
    }
    /**
     * Get current alarms by group
     * @param groupName Group filter
     * @returns Array of AlarmRow
     */
    getAlarmsByGroupName(groupName) {
        return this.alarms.filter(a => alarmsHelper_1.AlarmsHelper.isAlarmFromGroup(a, groupName)).map(a => new alarmRow_1.AlarmRow(a));
    }
    /**
     * Count alarms
     * @param filter Function to filter the alarms
     * @returns Number of alarms
     */
    count(filter = null) {
        if (filter == null)
            filter = () => true;
        return this.alarms.filter(filter).length;
    }
    /**
     * Acknowledge an alarm by operand name
     * @param operand Operand name
     */
    acknowledgeByName(operand) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppVisionService.svc/AcknowledgeAlarmByName?opName=' + operand, null);
        });
    }
    /**
     * Acknowledge an alarm by id
     * @param alarmId Alarm id
     */
    acknowledgeById(alarmId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/AcknowledgeAlarmById?Id=' + alarmId);
        });
    }
    /**
     * Mask or unmask an alarm by operand name
     * @param opName Operand name
     * @param isMasked True for masking, False for unmasking
     * @param tempo 0 for permanent. Value in milliseconds
     */
    mask(opName, isMasked, tempo = 60000) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get(`AppVisionService.svc/MaskAlarm?opName=${opName}&isMasked=${isMasked}&tempo=${tempo}`);
        });
    }
    /**
     * Add report to an alarm
     * @param alarmId Alarm id
     * @param report Report
     * @param alarmWaiting
     * @returns True if success false otherwise
     */
    report(alarmId, report, alarmWaiting) {
        return __awaiter(this, void 0, void 0, function* () {
            const response = yield this.http.post('AppVisionService.svc/ReportAlarm?id=' + alarmId + '&alarmWaiting=' + alarmWaiting, JSON.stringify([{ Name: "@Report", Value: report }]));
            const json = yield response.json();
            return json;
        });
    }
    /**
     * Transfer an alarm
     * @param alarmId Alarm id
     * @param userId User id to transfer the alarm
     * @returns void
     */
    transfer(alarmId, userId) {
        return __awaiter(this, void 0, void 0, function* () {
            const response = yield this.http.get('AppVisionService.svc/TransferAlarmById?id=' + alarmId + '&transferUserId=' + userId);
            const json = yield response.json();
            return json;
        });
    }
    /**
     * Get events history of an alarm
     * @param alarmId Alarm id
     * @returns Array of EventRow
     */
    getAlarmEvents(alarmId) {
        return __awaiter(this, void 0, void 0, function* () {
            const response = yield this.http.get('AppVisionService.svc/GetAlarmEvents?alarmId=' + alarmId);
            const json = yield response.json();
            return json.map(e => new eventRow_1.EventRow(e));
        });
    }
    /**
     * Add new alarm
     * @param severity
     * @param state
     * @param description
     * @param info
     * @param areaName
     * @param groupNames
     * @param videolink
     * @param attachments
     * @param waitReport
     * @returns Id of the alarm created
     */
    addAlarm(severity, state, description, info = null, areaName = null, groupNames = null, videolink = null, attachments = null, waitReport = false) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get(`AppVisionService.svc/AddAlarm?severity=${severity}&state=${state}&description=${description}&info=${info}&areaName=${areaName}&groupNames=${groupNames}&videolink=${videolink}&attachments=${attachments}&waitReport=${waitReport}`);
            return yield resp.json();
        });
    }
    /**
     * Add new Alarm
     * @param alarm AlarmRow object
     * @returns Id of the alarm created
     */
    addAlarmRow(alarm) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.post('AppVisionService.svc/AddAlarmRow', JSON.stringify(alarm));
            return yield resp.json();
        });
    }
    /**
     * Update an alarm
     * @param alarm Alarm to update
     * @returns void
     */
    updateAlarm(alarm) {
        return __awaiter(this, void 0, void 0, function* () {
            const response = yield this.http.post('AppVisionService.svc/UpdateAlarm', JSON.stringify(alarm));
            const json = yield response.json();
            return json;
        });
    }
    /**
     * Get history of alarms
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param filters Operand filter ($G.xxxx ...)
     * @param usersIds User Id filter
     * @param minSeverity Min severity filter
     * @param maxSeverity Max severity filter
     * @param maxCount default 2500
     * @returns Array of AlarmRow
     */
    getHistoAlarms(dateStart, dateEnd, filters = null, usersIds = null, minSeverity = 0, maxSeverity = 100, maxCount = 2500) {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = `AppVisionService.svc/GetHistoAlarms?` +
                `minId=0` +
                `&maxCount=${maxCount}` +
                `&dateStart=${dateStart.toISOString()}` +
                `&dateEnd=${dateEnd.toISOString()}` +
                `&filters=Severity>=${minSeverity},Severity<=${maxSeverity}`;
            if (filters && filters !== '')
                uri += ',' + filters;
            if (usersIds && usersIds.length > 0)
                uri += '&userIds=' + usersIds.join(',');
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            return json.map(a => new alarmRow_1.AlarmRow(a));
        });
    }
    /**
     * Get an alarm from histo
     * @param id Id of the alarm
     * @returns AlarmRow
     */
    getHistoAlarmById(id) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get("AppVisionService.svc/GetHistoAlarmById?alarmId=" + id);
            const json = yield resp.json();
            return new alarmRow_1.AlarmRow(json);
        });
    }
    /** @internal */
    updateAlarmsInternal(alarmsInfo) {
        alarmsInfo.forEach(alarmInfo => {
            switch (alarmInfo.Event) {
                case alarmInfo_1.AlarmEvent.Start:
                case alarmInfo_1.AlarmEvent.Resume:
                case alarmInfo_1.AlarmEvent.Acknowledge:
                case alarmInfo_1.AlarmEvent.Report:
                case alarmInfo_1.AlarmEvent.End:
                case alarmInfo_1.AlarmEvent.Update: {
                    const index = this.alarms.findIndex(a => a.Id === alarmInfo.Alarm.Id);
                    if (index >= 0) {
                        const copy = [...this.alarms];
                        copy[index] = alarmInfo.Alarm;
                        this.alarms = copy;
                    }
                    else {
                        this.alarms = [...this.alarms, alarmInfo.Alarm];
                    }
                    break;
                }
                case alarmInfo_1.AlarmEvent.Delete: {
                    const index = this.alarms.findIndex(a => a.Id === alarmInfo.Alarm.Id);
                    if (index >= 0) {
                        const copy = [...this.alarms];
                        copy.splice(index, 1);
                        this.alarms = copy;
                    }
                    break;
                }
                default:
                    break;
            }
        });
        this.alarms = this.alarms.sort(alarmsHelper_1.AlarmsHelper.compare);
        this.alarmsChanged.next(alarmsInfo);
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/GetCurrentAlarms');
            const json = yield resp.json();
            this.alarms = json.map(a => new alarmRow_1.AlarmRow(a));
        });
    }
}
exports.AlarmManager = AlarmManager;
