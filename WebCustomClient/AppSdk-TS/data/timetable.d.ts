export declare enum TimetableType {
    Daily = 0,
    Weekly = 1,
    WeeklyWithHolidays = 2
}
export declare class Timetable {
    Type: TimetableType;
    Items: TimetableItem[];
    DefaultItems: TimetableItem[];
    ItemsWithPeriod: TimetableWithPeriod[];
    constructor(tt: Partial<Timetable>);
}
export declare class TimetableItem {
    TotalMinutes: number;
    Value: number;
    Day: number;
    get getDay(): number;
    Hour: number;
    get getHour(): number;
    Minute: number;
    get getMinute(): number;
    IsValid: boolean;
    isValid(timeTableType: TimetableType): boolean;
    equals(item: TimetableItem): boolean;
    IsHoliday: boolean;
    get isHoliday(): boolean;
    constructor(tt?: Partial<Timetable>);
}
export declare class TimetableWithPeriod {
    Start: string;
    get start(): Date;
    End: string;
    get end(): Date;
    Items: TimetableItem[];
    match(date: number): boolean;
    constructor(tt?: Partial<TimetableWithPeriod>);
}
