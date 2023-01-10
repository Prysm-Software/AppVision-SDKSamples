import { CustomColor } from "../data/customColor";
import { AlarmManager } from "./alarmManager";
import { AreaManager } from "./areaManager";
import { OptionManager } from "./optionManager";
export declare class RowCustomColorManager {
    customColors: CustomColor[];
    optionManager: OptionManager;
    areaManager: AreaManager;
    alarmManager: AlarmManager;
    constructor(optionManager: OptionManager, areaManager: AreaManager, alarmManager: AlarmManager);
    getCustomColors(): CustomColor[];
    /**
     * Get the custom color of an object
     * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
     * @returns Css color or undefined
     */
    getCustomColor(obj: any): CustomColor | undefined;
    private loadInternal;
    private getColor;
}
