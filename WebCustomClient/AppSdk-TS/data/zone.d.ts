export declare class Zone {
    Id: number;
    Name: string;
    Description: string;
    Extension: string;
    CustomExtension: string;
    Parameters: string;
    CheckAntipassbackInput: boolean;
    CheckAntipassbackOutput: boolean;
    ZoneReaderIns: ZoneReaderIn[];
    ZoneReaderOuts: ZoneReaderOut[];
    constructor(obj: Partial<Zone>);
}
export declare class ZoneReaderIn {
    ZoneId: number;
    ReaderId: number;
    constructor(obj: Partial<ZoneReaderIn>);
}
export declare class ZoneReaderOut {
    ZoneId: number;
    ReaderId: number;
    constructor(obj: Partial<ZoneReaderOut>);
}
