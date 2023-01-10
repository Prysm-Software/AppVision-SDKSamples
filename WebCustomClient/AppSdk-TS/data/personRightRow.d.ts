export declare class PersonRightRow {
    PersonId: number;
    ReaderId: number;
    CrisisLevel: number;
    TimetableId: number;
    ValidityStart: string;
    ValidityEnd: string;
    ValidityInfo: string;
    get validityStart(): Date;
    get validityEnd(): Date;
    constructor(p: Partial<PersonRightRow>);
}
