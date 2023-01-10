export declare class AccessProfileRightRow {
    AccessProfileId: number;
    ReaderId: number;
    TimetableId: number;
    CrisisLevel: number;
    ValidityStart: string;
    get validityStart(): Date;
    ValidityEnd: string;
    get validityEnd(): Date;
    constructor(r: Partial<AccessProfileRightRow>);
}
