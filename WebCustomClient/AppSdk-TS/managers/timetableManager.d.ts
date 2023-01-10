import { Timetable } from "../data/timetable";
import { HttpService } from "../services/httpService";
import { VariableManager } from "./variableManager";
export declare class TimetableManager {
    private http;
    private variableManager;
    constructor(http: HttpService, variableManager: VariableManager);
    fromXML(xml: string): Timetable;
}
