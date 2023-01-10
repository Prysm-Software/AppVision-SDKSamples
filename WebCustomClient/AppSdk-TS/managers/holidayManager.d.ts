import { HolidayRow } from "../data/holidayRow";
import { HttpService } from "../services/httpService";
export declare class HolidayManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all Hollidays
     * @returns Array of HollidayRow
     */
    getAll(): Promise<HolidayRow[]>;
    /**
     * Add new holiday
     * @param row HolidayRow to add
     * @returns Id of the holidayRow created
     */
    add(row: HolidayRow): Promise<number>;
    /**
     * Delete holiday
     * @param row Holiday to delete
     */
    delete(row: HolidayRow): Promise<void>;
}
