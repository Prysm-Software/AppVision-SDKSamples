import { DepartmentRow } from '../data/departmentRow';
import { HttpService } from '../services/httpService';
export declare class DepartmentManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all departments
     * @returns Array of DepartmentRow
     */
    getAll(): Promise<DepartmentRow[]>;
    /**
     * Add departmentRow
     * @param department DepartmentRow to add
     * @returns DepartmentRow created
     */
    add(department: DepartmentRow): Promise<DepartmentRow>;
    /**
     * Delete departmentRow
     * @param id DepartmentRow Id
     */
    delete(id: number): Promise<void>;
    /**
     * Update departmentRow
     * @param department DepartmentRow to update
     */
    update(department: DepartmentRow): Promise<void>;
}
