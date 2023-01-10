import { Subject } from "rxjs";
import { AlarmRow } from "../data/alarmRow";
import { AlarmInfo } from "../data/alarmInfo";
import { EventRow } from "../data/eventRow";
import { HttpService } from "../services/httpService";
import { AreaRow } from "../data/areaRow";
import { AppEventReturnHandler } from "../common/eventsHelper";
export declare class AlarmManager {
    private http;
    private alarms;
    alarmsChanged: Subject<AlarmInfo[]>;
    constructor(http: HttpService);
    /**
     * Get current alarms
     * @param maxCount Max number of alarms wanted (default: 0 => return all)
     * @param filter Function to filter the alarms (default: null => no filter)
     * @returns Array of AlarmRow
     */
    getAlarms(maxCount?: number, filter?: AppEventReturnHandler<AlarmRow, boolean>): AlarmRow[];
    /**
     * Get an alarm by its Id
     * @param id Alarm id
     * @returns Alarm Row
     */
    getById(id: number): AlarmRow;
    /**
     * Get current alarms by area
     * @param area Area filter
     * @returns Array of AlarmRow
     */
    getAlarmsByArea(area: AreaRow): AlarmRow[];
    /**
     * Get current alarms by group
     * @param groupName Group filter
     * @returns Array of AlarmRow
     */
    getAlarmsByGroupName(groupName: string): AlarmRow[];
    /**
     * Count alarms
     * @param filter Function to filter the alarms
     * @returns Number of alarms
     */
    count(filter?: AppEventReturnHandler<AlarmRow, boolean>): number;
    /**
     * Acknowledge an alarm by operand name
     * @param operand Operand name
     */
    acknowledgeByName(operand: string): Promise<void>;
    /**
     * Acknowledge an alarm by id
     * @param alarmId Alarm id
     */
    acknowledgeById(alarmId: number): Promise<void>;
    /**
     * Mask or unmask an alarm by operand name
     * @param opName Operand name
     * @param isMasked True for masking, False for unmasking
     * @param tempo 0 for permanent. Value in milliseconds
     */
    mask(opName: string, isMasked: boolean, tempo?: number): Promise<void>;
    /**
     * Add report to an alarm
     * @param alarmId Alarm id
     * @param report Report
     * @param alarmWaiting
     * @returns True if success false otherwise
     */
    report(alarmId: number, report: string, alarmWaiting: boolean): Promise<boolean>;
    /**
     * Transfer an alarm
     * @param alarmId Alarm id
     * @param userId User id to transfer the alarm
     * @returns void
     */
    transfer(alarmId: number, userId: number): Promise<boolean>;
    /**
     * Get events history of an alarm
     * @param alarmId Alarm id
     * @returns Array of EventRow
     */
    getAlarmEvents(alarmId: number): Promise<EventRow[]>;
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
    addAlarm(severity: number, state: string, description: string, info?: string, areaName?: string, groupNames?: string, videolink?: string, attachments?: string, waitReport?: boolean): Promise<number>;
    /**
     * Add new Alarm
     * @param alarm AlarmRow object
     * @returns Id of the alarm created
     */
    addAlarmRow(alarm: AlarmRow): Promise<number>;
    /**
     * Update an alarm
     * @param alarm Alarm to update
     * @returns void
     */
    updateAlarm(alarm: AlarmRow): Promise<boolean>;
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
    getHistoAlarms(dateStart: Date, dateEnd: Date, filters?: string, usersIds?: number[], minSeverity?: number, maxSeverity?: number, maxCount?: number): Promise<AlarmRow[]>;
    /**
     * Get an alarm from histo
     * @param id Id of the alarm
     * @returns AlarmRow
     */
    getHistoAlarmById(id: number): Promise<AlarmRow>;
}
