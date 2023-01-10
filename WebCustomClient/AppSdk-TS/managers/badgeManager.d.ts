import { BadgeRow } from '../data/badgeRow';
import { HttpService } from '../services/httpService';
export declare class BadgeManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all badge
     * @returns Array of BadgeRow
     */
    getAll(): Promise<BadgeRow[]>;
    /**
     * Add a badge to a person
     * @param personId Person Id
     * @param badge BadgeRow to add
     * @returns BadgeRow created
     */
    addToPerson(personId: number, badge: BadgeRow): Promise<BadgeRow>;
    /**
     * Get all badge from a person
     * @param personId Person Id
     * @returns Array of BadgeRow
     */
    getFromPerson(personId: number): Promise<BadgeRow[]>;
    /**
     * Delete a badge
     * @param badge BadgeRow to delete
     *
     * @remark
     * Id is not sufficient so we need to send the whole object
     */
    delete(badge: BadgeRow): Promise<void>;
    /**
     * Update a badge
     * @param badge BadgeRow to update
     */
    update(badge: BadgeRow): Promise<void>;
    /**
     * Get all badge from a badge type
     * @param id BadgeType id
     * @returns Array of BadgeRow
     */
    getFromBadgeType(id: number): Promise<BadgeRow[]>;
}
