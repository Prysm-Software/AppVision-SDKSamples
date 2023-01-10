import { Zone } from "../data/zone";
import { HttpService } from "../services/httpService";
export declare class ZoneManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all zones from server
     * @returns Array of zone
     */
    getAll(): Promise<Zone[]>;
    /**
     * Count zones
     * @returns Zones number
     */
    count(): Promise<number>;
    /**
     * Add new zone
     * @param zone Zone object
     * @returns Zone created
     */
    add(zone: Zone): Promise<Zone>;
    /**
     * Delete zone by id
     * @param id Id of the zone
     */
    delete(id: number): Promise<void>;
    /**
     * Update zone
     * @param zone Zone object
     * @param updateReaders True to update the readers props of the zone
     */
    update(zone: Zone, updateReaders: boolean): Promise<void>;
}
