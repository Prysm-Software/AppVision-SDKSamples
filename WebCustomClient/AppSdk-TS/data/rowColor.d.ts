import { AppServer } from "../appServer";
/**
 * Helper class for getting foreground and background color of an Alarm, Event, State ...
 */
export declare class RowColor {
    private server;
    constructor(server: AppServer);
    /**
     * Get the background color of an object
     * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
     * @returns Css color
     */
    getBackgroundColor(obj: any): string;
    /**
    * Get the text color of an object
    * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
    * @returns Css color
    */
    getForegroundColor(obj: any): string;
    private getColor;
}
