import { DateTimeOffset } from "./dateTimeOffset";
export declare enum UserMessageStatus {
    InProgress = 1,
    WaitAck = 5,
    Completed = 10
}
export declare class UserMessageRow {
    Id: number;
    DateCreate: DateTimeOffset;
    DateClose: DateTimeOffset;
    AlarmId?: number;
    AreaId?: number;
    AreaDesc: string;
    Description: string;
    GroupNames: string;
    GroupDesc: string;
    Attachments: string;
    Photo: string;
    UserIdCreate: number;
    UserIdClose?: number;
    UserNameCreate: string;
    UserNameClose: string;
    Severity: number;
    ContentCategory: string;
    ContentReference: string;
    Content: string;
    Status: UserMessageStatus;
    To: string;
    Category: string;
    IsIncident: boolean;
    Extension: string;
    CustomExtension: string;
    ServerName: string;
    DateWrite: string;
    constructor(obj: Partial<UserMessageRow>);
    get dateCreate(): Date;
}
