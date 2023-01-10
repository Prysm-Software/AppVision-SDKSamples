import { AlarmRow } from "./alarmRow";
export declare enum AlarmEvent {
    Start = 0,
    End = 1,
    Update = 2,
    Acknowledge = 3,
    Report = 4,
    Resume = 5,
    Delete = 6,
    Transfer = 7,
    None = 8
}
export declare class AlarmInfo {
    Alarm: AlarmRow;
    Event: number;
    constructor(alarmInfo: Partial<AlarmInfo>);
}
