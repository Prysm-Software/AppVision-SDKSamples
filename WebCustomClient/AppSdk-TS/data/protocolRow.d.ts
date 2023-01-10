import { ProtocolState } from "./protocolState";
export declare class ProtocolRow {
    constructor(protocol: Partial<ProtocolRow>);
    State: ProtocolState;
    Address: string;
    AutoStart: boolean;
    CustomExtension: string;
    Description: string;
    Executable: string;
    Extension: string;
    Icon: string;
    Id: number;
    IsExclude: boolean;
    LogFilename: string;
    LogLevel: number;
    MaxRetry: number;
    Name: string;
    Parameters: string;
    Timeout: number;
    Url: string;
}
