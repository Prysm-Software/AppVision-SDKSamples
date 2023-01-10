import { AccessProfileRow } from '../data/accessProfileRow';
import { PersonRow } from '../data/personRow';
import { HttpService } from '../services/httpService';
import { RightManager } from './rightManager';
declare class PersonProfileRow {
    PersonId: number;
    AccessProfileId: number;
    AccessProfile: AccessProfileRow;
    constructor(p: Partial<PersonProfileRow>);
}
export declare class AccessProfileManager {
    private http;
    private rightManager;
    constructor(http: HttpService, rightManager: RightManager);
    /**
     * Get all access profile from the server
     * @returns Array of accessProfile
     */
    getAll(): Promise<AccessProfileRow[]>;
    /**
     * Add new access profile with linked prop AccessProfileRights
     * @param profile Profile to add
     * @returns Array of AccessProfile
     */
    add(profile: AccessProfileRow): Promise<AccessProfileRow>;
    /**
     * Update access profile
     * @param profile Profile to update
     * @param updateRights If true, delete and update the linked rights. Default is False
     */
    update(profile: AccessProfileRow, updateRights?: boolean): Promise<void>;
    /**
     * Delete access profile
     * @param profileId Access profile id
     */
    delete(profileId: number): Promise<void>;
    /**
     * Get all profile from a person
     * @param personId Person id
     * @returns Array of access profile
     */
    getPersonProfiles(personId: number): Promise<AccessProfileRow[]>;
    /**
     * Get category and department profile of a person
     * @param person Person (the method use the DepartmentId and CategoryId)
     * @returns Array of access profile
     */
    getInheritedPersonProfiles(person: PersonRow): Promise<AccessProfileRow[]>;
    /**
     * Add access profile to a person
     * @param personId Id of the person
     * @param profileId Id of the profile
     * @returns PersonProfileRow created
     */
    addPersonProfile(personId: number, profileId: number): Promise<PersonProfileRow>;
    /**
     * Delete profile from a person
     * @param personId Person id
     * @param profileId Profile id
     */
    deletePersonProfile(personId: number, profileId: number): Promise<void>;
}
export {};
