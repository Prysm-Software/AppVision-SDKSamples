import { Subject } from "rxjs";
import { AreaRow } from "../data/areaRow";
import { AreaState } from "../data/areaState";
import { HttpService } from "../services/httpService";
export declare class AreaManager {
    private http;
    private areas;
    rowsUpdated: Subject<AreaRow[]>;
    statesChanged: Subject<AreaState[]>;
    get count(): number;
    constructor(http: HttpService);
    /**
     * Build area tree
     * @returns Array of TreeRow
     */
    getAreaTree(): AreaRow[];
    /**
     * Get an area by Id
     * @param id Area id
     * @returns AreaRow
     */
    getAreaById(id: number): AreaRow;
    /**
     * Get an area by name
     * @param name Area name (with or without the '$A.')
     * @returns AreaRow
     */
    getAreaByName(name: string): AreaRow;
}
