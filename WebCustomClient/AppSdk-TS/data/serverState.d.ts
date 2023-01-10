export declare class ServerState {
    CpuCounter: number;
    ServerName: string;
    RamAvailable: number;
    RamInstalled: number;
    SiteId: number;
    SiteInfo: string;
    State: number;
    SystemStateError: string;
    Version: string;
    constructor(obj: Partial<ServerState>);
    get cpuPercent(): number;
}
