import { PersonZone } from "../data/personZone";
import { HttpService } from "../services/httpService";
declare type PersonZoneExpand = 'Zone' | 'Person' | 'Person/Category' | 'Person/Department';
interface GetPersonZoneOptionsRequest {
    expands?: PersonZoneExpand[];
    filterZone?: number[];
    filterCategories?: number[];
    filterDepartments?: number[];
}
export declare class PersonZoneManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all person zone
     * @param options Request options. Allow to set expand parameters and return linked property.
     * @returns Array of PersonZone
     *
     * @example
     * Request person with person and zone:
     * server.personZoneManager.getAll({expands: ['Zone', 'Person']})
     */
    getAll(options?: GetPersonZoneOptionsRequest): Promise<PersonZone[]>;
    /**
     * Add new personZone
     * @param personZone PersonZone to add
     * @returns PersonZone created
     */
    add(personZone: PersonZone): Promise<PersonZone>;
    /**
     * Delete personZone
     * @param personZone PersonZone to delete
     */
    delete(personZone: PersonZone): Promise<void>;
}
export {};
