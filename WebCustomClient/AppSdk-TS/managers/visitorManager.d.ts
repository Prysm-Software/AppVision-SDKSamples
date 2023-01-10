import { BadgeRow } from '../data/badgeRow';
import { VisitorRow } from '../data/visitorRow';
import { HttpService } from '../services/httpService';
declare type Expand = 'Person' | 'Category' | 'Department' | 'Person/Badges';
interface GetVisitorsOptionsRequest {
    expands?: Expand[];
}
export declare class VisitorManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all visitors
     * @param options Option request. Allow to request linked property on visitors like Person, Category ...
     * @returns Array of visitors
     */
    getAll(options?: GetVisitorsOptionsRequest): Promise<VisitorRow[]>;
    /**
     * Get a single visitor
     * @param id Id of the visitor
     * @param options Option request. Allow to request linked property on visitors like Person, Category ...
     * @returns VisitorRow
     */
    getById(id: number, options?: GetVisitorsOptionsRequest): Promise<VisitorRow>;
    /**
     * Count visitors
     * @returns Number of visitors
     */
    count(): Promise<number>;
    /**
     * Add a visitor (automatically create the person if needed)
     * @param visitor VisitorRow to add
     * @param badge BadgeRow (optionnal). If BadgeRow is not null, the personRow will automatically be created
     */
    add(visitor: VisitorRow, badge?: BadgeRow): Promise<VisitorRow>;
    /**
     * Update an existing visitor
     * @param visitor Visitor to update
     * @param badge Badge of the visitor if any (optional)
     * @returns Visitor updated
     */
    update(visitor: VisitorRow, badge?: BadgeRow): Promise<VisitorRow>;
    /**
     * Delete a visitor
     * @param id Id of the visitor
     */
    delete(id: number): Promise<void>;
}
export {};
