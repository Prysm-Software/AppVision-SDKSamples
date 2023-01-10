export declare enum DeviceType {
    Mobile = 0,
    PC = 1,
    Other = 100
}
export declare class DeviceInfos {
    DeviceId: string;
    RowName: string;
    RowId: number;
    Type: DeviceType;
    constructor(obj: Partial<DeviceInfos>);
}
