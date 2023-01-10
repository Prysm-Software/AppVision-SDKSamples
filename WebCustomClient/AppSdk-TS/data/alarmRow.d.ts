import { DateTimeOffset } from "./dateTimeOffset";
export declare enum AlarmStatus {
    None = 0,
    AcknowledgeWaiting = 10,
    Processing = 20,
    ProcessWaiting = 30,
    Closed = 100
}
export declare class AlarmRow {
    Id: number;
    AreaId: number;
    AreaDesc: string;
    Attachments: string;
    Count: number;
    Description: string;
    Severity: number;
    SourceName: string;
    SourceId: number;
    SourceId2: number;
    SourceId3: number;
    WaitAcknowledge: boolean;
    WaitEnd: boolean;
    WaitProcess: boolean;
    WaitReport: boolean;
    Info: string;
    Instructions: string;
    Type: number;
    StateDesc: string;
    GroupDesc: string;
    GroupNames: string;
    Video: string;
    Photo: string;
    Report: string;
    UserIdAck: number;
    UserIdReport: number;
    UserNameAck: string;
    UserNameReport: string;
    DateAck: DateTimeOffset;
    get dateAck(): Date;
    DateStart: DateTimeOffset;
    get dateStart(): Date;
    DateReceive: DateTimeOffset;
    get dateReceive(): Date;
    DateReport: DateTimeOffset;
    get dateReport(): Date;
    DateEnd: DateTimeOffset;
    get dateEnd(): Date;
    get status(): AlarmStatus;
    get groups(): string[];
    /**
     * Return attachment as an array
     * No need to use it as a setter, this returns a cloned array. Use addAttachment
     */
    get attachments(): string[];
    constructor(event: Partial<AlarmRow>);
    /**
     * Add a file path to the attachments
     * @param filePath Filepath without the Config/
     */
    addAttachment(filePath: string): void;
}
