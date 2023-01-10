export declare class GroupState {
    Id: number;
    AlarmAcknowledgeWaitingCount: number;
    AlarmClosedCount: number;
    AlarmCount: number;
    AlarmProcessWaitingCount: number;
    AlarmProcessingCount: number;
    AndValue: number;
    LockedCount: number;
    MaskedCount: number;
    MaxSeverity: number;
    Name: string;
    OrValue: number;
    WaitEndCount: number;
    WaitReportCount: number;
    constructor(state: Partial<GroupState>);
    get isAlarm(): boolean;
    get isAlarmAcknowledgeWaiting(): boolean;
    get isAlarmProcessing(): boolean;
    get isAlarmProcessWaiting(): boolean;
    get isAlarmClosed(): boolean;
}
