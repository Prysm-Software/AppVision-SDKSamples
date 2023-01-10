import { DateTimeOffset } from "../data/dateTimeOffset";
export declare abstract class DateHelper {
    static toJSDate(date: DateTimeOffset): Date;
    static strToJSDate(date: string): Date;
    static stringToDate(dateStr: string): Date;
    static toDateTimeString(date: Date): string;
    static toDateString(date: Date): string;
    static today(): Date;
}
