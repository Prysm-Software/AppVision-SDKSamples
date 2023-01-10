export declare class HolidayRow {
    Id: number;
    Date: string;
    Type: number;
    IsAnnual: boolean;
    Culture: string;
    Extension: string;
    CustomExtension: string;
    Url: string;
    get date(): Date;
    constructor(state: Partial<HolidayRow>);
}
