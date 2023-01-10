import { BadgeType } from '../data/badgeType';
import { HttpService } from '../services/httpService';
import { RightManager } from './rightManager';
export declare class BadgeTypeManager {
    private http;
    private rightManager;
    constructor(http: HttpService, rightManager: RightManager);
    /**
     * Get all badge types
     * @returns Array of BadgeType
     */
    getAll(): Promise<BadgeType[]>;
    /**
     * Add new badgeType
     * @param badgeType BadgeType object to add
     * @returns BadgeType created
     */
    add(badgeType: BadgeType): Promise<BadgeType>;
    /**
     * Delete a badgeType
     * @param id BadgeType Id
     */
    delete(id: number): Promise<void>;
    /**
     * Update badgeType
     * @param badgeType BadgeType to update
     */
    update(badgeType: BadgeType): Promise<void>;
}
