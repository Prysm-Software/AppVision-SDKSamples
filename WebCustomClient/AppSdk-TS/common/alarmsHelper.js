"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AlarmsHelper = void 0;
const alarmInfo_1 = require("../data/alarmInfo");
const alarmRow_1 = require("../data/alarmRow");
class AlarmsHelper {
    /**
     * Tells if an alarm belong to a group
     * @param alarm Alarm
     * @param groupName Group name (with or without the $G.)
     * @returns True of false
     */
    static isAlarmFromGroup(alarm, groupName) {
        if (groupName.startsWith('$G.')) {
            groupName = groupName.substring(3);
        }
        if (alarm.groups.length === 0) {
            return false;
        }
        if (alarm.groups.indexOf(groupName) >= 0) {
            return true;
        }
        for (const group of alarm.groups) {
            if (group.startsWith(groupName + '.')) {
                return true;
            }
        }
        return false;
    }
    /**
     * Update an AlarmRow array with an AlarmInfo array.
     * @param alarms Existing array
     * @param update Update array
     * @returns The alarms array updated
     */
    static updateAlarmsList(alarms, update) {
        const newAlarms = [...alarms];
        update.forEach(alarmInfo => {
            switch (alarmInfo.Event) {
                case alarmInfo_1.AlarmEvent.Start:
                case alarmInfo_1.AlarmEvent.Resume:
                case alarmInfo_1.AlarmEvent.Update:
                case alarmInfo_1.AlarmEvent.Report:
                case alarmInfo_1.AlarmEvent.End:
                case alarmInfo_1.AlarmEvent.Acknowledge: {
                    const index = newAlarms.findIndex(v => v.Id === alarmInfo.Alarm.Id);
                    if (index >= 0) {
                        newAlarms[index] = alarmInfo.Alarm;
                    }
                    else {
                        newAlarms.unshift(alarmInfo.Alarm);
                    }
                    break;
                }
                case alarmInfo_1.AlarmEvent.Delete: {
                    const index = newAlarms.findIndex(a => a.Id === alarmInfo.Alarm.Id);
                    if (index >= 0) {
                        newAlarms.splice(index, 1);
                    }
                    break;
                }
                default:
                    break;
            }
        });
        return newAlarms;
    }
    /**
     * Return the language key corresponding to the alarm status
     * @param status Alarm status
     * @returns RT string key
     */
    static getStatusRtKey(status) {
        switch (status) {
            case alarmRow_1.AlarmStatus.AcknowledgeWaiting: return 'AlarmStatus.AcknowledgeWaiting';
            case alarmRow_1.AlarmStatus.Closed: return 'AlarmStatus.Closed';
            case alarmRow_1.AlarmStatus.Processing: return 'AlarmStatus.Processing';
            case alarmRow_1.AlarmStatus.ProcessWaiting: return 'AlarmStatus.ProcessWaiting';
            default: return 'None';
        }
    }
    static compare(a, b) {
        if (a.WaitAcknowledge && !b.WaitAcknowledge)
            return -1;
        if (!a.WaitAcknowledge && b.WaitAcknowledge)
            return 1;
        if (a.Severity > b.Severity)
            return -1;
        if (a.Severity < b.Severity)
            return 1;
        if (a.dateStart > b.dateStart)
            return -1;
        if (a.dateStart < b.dateStart)
            return 1;
        return 0;
    }
    static sortAlarmArray(alarms) {
        return alarms.sort((a, b) => {
            return (((a.WaitAcknowledge === b.WaitAcknowledge) ? 0 : a.WaitAcknowledge ? -1 : 1) ||
                (a.Severity < b.Severity ? 1 : -1) ||
                (a.dateStart < b.dateStart ? 1 : -1));
        });
    }
}
exports.AlarmsHelper = AlarmsHelper;
