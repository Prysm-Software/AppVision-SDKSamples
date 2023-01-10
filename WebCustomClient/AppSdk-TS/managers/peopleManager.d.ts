import { PersonRow } from '../data/personRow';
import { HttpService } from '../services/httpService';
declare type PeopleExpand = 'Badges' | 'Category' | 'Department' | 'PersonRights';
interface GetPeopleOptionsRequest {
    expands?: PeopleExpand[];
}
export declare class PeopleManager {
    private http;
    constructor(http: HttpService);
    /**
     * Get all people
     * @param options Request options. Allow to set expand parameters and return linked property.
     * @returns Array of person row
     *
     * @example
     * Request people with badges :
     * server.peopleManager.getAll({expands: ['Badges']})
     */
    getAll(options?: GetPeopleOptionsRequest): Promise<PersonRow[]>;
    /**
     * Count people
     * @returns People number
     */
    count(): Promise<number>;
    /**
     * Add new person
     * @param person PersonRow to add
     * @returns PersonRow added
     */
    add(person: PersonRow): Promise<PersonRow>;
    /**
     * Delete person
     * @param personId Person Id
     */
    delete(personId: number): Promise<void>;
    /**
     * Update person
     * @param person PersonRow to update
     */
    update(person: PersonRow): Promise<void>;
    /**
     * Get all people from a department
     * @param id Department id
     * @returns Array of PersonRow
     */
    getFromDepartment(id: number): Promise<PersonRow[]>;
    /**
     * Get all people from a category
     * @param id Category id
     * @returns Array of PersonRow
     */
    getFromCategory(id: number): Promise<PersonRow[]>;
    /**
     * Get all people from an access profile
     * @param id Access profile id
     * @returns Array of PersonRow
     */
    getFromAccessProfile(id: number): Promise<PersonRow[]>;
}
export {};
