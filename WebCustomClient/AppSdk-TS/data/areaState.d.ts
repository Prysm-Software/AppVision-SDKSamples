export declare class AreaState {
    Id: number;
    Name: string;
    OrValue: number;
    AndValue: number;
    LockedCount: number;
    MaskedCount: number;
    AlarmCount: number;
    AlarmAcknowledgeWaitingCount: number;
    AlarmProcessingCount: number;
    AlarmProcessWaitingCount: number;
    AlarmClosedCount: number;
    MaxSeverity: number;
    WaitEndCount: number;
    WaitReportCount: number;
    constructor(areaState: Partial<AreaState>);
    get isAlarm(): boolean;
    get isAlarmAcknowledgeWaiting(): boolean;
    get isAlarmProcessing(): boolean;
    get isAlarmProcessWaiting(): boolean;
    get isAlarmClosed(): boolean;
}
