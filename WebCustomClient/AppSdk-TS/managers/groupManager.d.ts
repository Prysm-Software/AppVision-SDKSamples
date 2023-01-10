import { Subject } from "rxjs";
import { GroupRow } from "../data/groupRow";
import { GroupState } from "../data/groupState";
import { HttpService } from "../services/httpService";
export declare class GroupManager {
    private http;
    private groups;
    rowsUpdated: Subject<GroupRow[]>;
    statesChanged: Subject<GroupState[]>;
    get count(): number;
    constructor(http: HttpService);
    /**
     * Build GroupRow tree
     * @returns Tree of GroupRow
     */
    getGroupTree(): GroupRow[];
    /**
     * Get group by id
     * @param id Group Id
     * @returns GroupRow
     */
    getGroupById(id: number): GroupRow;
    /**
     * Get group by name
     * @param name Groupe name
     * @returns GroupRow
     */
    getGroupByName(name: string): GroupRow;
}
