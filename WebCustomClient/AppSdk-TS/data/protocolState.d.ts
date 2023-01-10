import { DateTimeOffset } from "./dateTimeOffset";
import { ProtocolRow } from "./protocolRow";
export declare enum ProtocolStatus {
    Started = 0,
    Synchronized = 1,
    Downloading = 2,
    Stopped = 3,
    Fault = 4
}
export declare class ProtocolState {
    Id: number;
    Name: string;
    ProcessName: string;
    ProcessVersion: string;
    ProductCompany: string;
    ProductDescription: string;
    ProductName: string;
    Status: number;
    Date: DateTimeOffset;
    DateStart: DateTimeOffset;
    Row: ProtocolRow;
    constructor(state: Partial<ProtocolState>);
    get date(): Date;
    get dateStart(): Date;
    get status(): string;
}
