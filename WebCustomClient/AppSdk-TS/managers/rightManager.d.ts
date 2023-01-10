import { AccessProfileRightRow } from '../data/accessProfileRightRow';
import { PersonRightRow } from '../data/personRightRow';
import { PersonRow } from '../data/personRow';
import { HttpService } from '../services/httpService';
export interface PersonRightMap {
    person: PersonRow;
    accessProfileId?: number;
    ttId: number;
}
export declare class RightManager {
    private http;
    constructor(http: HttpService);
    /**
     * Add right to an access profile
     * @param right Right object
     * @returns Right object created
     */
    addAccessProfileRight(right: AccessProfileRightRow): Promise<AccessProfileRightRow>;
    /**
     * Delete rigt from an access profile
     * @param right Right object to delete
     */
    deleteAccessProfileRight(right: AccessProfileRightRow): Promise<void>;
    /**
     * Get all righs of an access profile
     * @param accessProfileId
     * @returns Array of rights
     */
    getAccessProfileRights(accessProfileId: number): Promise<AccessProfileRightRow[]>;
    /**
     * Get array of PersonRightMap from a reader
     * @param readerId Reader id
     * @returns Array of PersonRightMap
     */
    getPeopleRightMapByReader(readerId: number): Promise<PersonRightMap[]>;
    /**
     * Get access exception from a person
     * @param personId Person Id
     * @returns Array of rights
     */
    getPersonAccessExceptions(personId: number): Promise<PersonRightRow[]>;
    /**
     * Delete access exception from a person
     * @param right Access exception to delete
     */
    deletePersonAccessException(right: PersonRightRow): Promise<void>;
    /**
     * Add new access exception to a person
     * @param right
     */
    addPersonAccessException(right: PersonRightRow): Promise<void>;
}
