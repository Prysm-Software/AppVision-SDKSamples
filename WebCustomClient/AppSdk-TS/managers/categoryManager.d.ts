import { CategoryRow } from '../data/categoryRow';
import { HttpService } from '../services/httpService';
export declare class CategoryManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all categories
     * @returns Array of CategoryRow
     */
    getAll(): Promise<CategoryRow[]>;
    /**
     * Add new categoryRow
     * @param category
     * @returns CategoryRow created
     */
    add(category: CategoryRow): Promise<CategoryRow>;
    /**
     * Delete categoryRow
     * @param id CategoryRow Id
     */
    delete(id: number): Promise<void>;
    /**
     * Update CategoryRow
     * @param category CategoryRow to update
     */
    update(category: CategoryRow): Promise<void>;
}
