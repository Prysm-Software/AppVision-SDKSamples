import { Timetable, TimetableItem } from "../data/timetable";
export declare abstract class TimetableHelper {
    static fromXML(xml: string): Timetable;
    static toXML(timetable: Timetable): string;
    static compareItems(items1: TimetableItem[], items2: TimetableItem[]): boolean;
}
