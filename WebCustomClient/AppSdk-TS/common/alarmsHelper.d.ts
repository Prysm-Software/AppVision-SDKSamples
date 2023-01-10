import { AlarmInfo } from "../data/alarmInfo";
import { AlarmRow, AlarmStatus } from "../data/alarmRow";
export declare abstract class AlarmsHelper {
    /**
     * Tells if an alarm belong to a group
     * @param alarm Alarm
     * @param groupName Group name (with or without the $G.)
     * @returns True of false
     */
    static isAlarmFromGroup(alarm: AlarmRow, groupName: string): boolean;
    /**
     * Update an AlarmRow array with an AlarmInfo array.
     * @param alarms Existing array
     * @param update Update array
     * @returns The alarms array updated
     */
    static updateAlarmsList(alarms: AlarmRow[], update: AlarmInfo[]): AlarmRow[];
    /**
     * Return the language key corresponding to the alarm status
     * @param status Alarm status
     * @returns RT string key
     */
    static getStatusRtKey(status: AlarmStatus): string;
    static compare(a: AlarmRow, b: AlarmRow): number;
    static sortAlarmArray(alarms: AlarmRow[]): AlarmRow[];
}
