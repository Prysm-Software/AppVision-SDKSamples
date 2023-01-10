import { DateTimeOffset } from "./dateTimeOffset";
export declare class ClientInfo {
    ProductDescription: string;
    ProductCompany: string;
    ProductName: string;
    ProcessName: string;
    ProcessVersion: string;
    ClientId: string;
    Id: number;
    Name: string;
    FullName: string;
    HostName: string;
    DateStart: DateTimeOffset;
    IsProtocol: boolean;
    constructor(clientInfo: Partial<ClientInfo>);
}
