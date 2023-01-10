import { AreaManager } from "../managers/areaManager";
import { AlarmManager } from "../managers/alarmManager";
export declare class CustomColor {
    private areaManager;
    private alarmManager;
    background: string;
    foreground: string;
    description: string;
    filterSeverityMin: number;
    filterSeverityMax: number;
    filterArea: string;
    filterGroup: string;
    filterType: string;
    constructor(filter: string, sbackground: string, sforeground: string, alarmManager: AlarmManager, areaManager: AreaManager);
    isMatch(o: any): boolean;
    private checkType;
    private checkAlarmStatus;
    private checkSeverity;
    private checkArea;
    private checkGroup;
    private checkGroups;
}
