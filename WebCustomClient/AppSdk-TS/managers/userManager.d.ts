import { UserRow } from "../data/userRow";
import { ClientInfo } from "../data/clientInfo";
import { ProfileRow } from "../data/profileRow";
import { HttpService } from "../services/httpService";
import { UserState } from "../data/userState";
import { Subject } from "rxjs";
export declare class UserManager {
    private http;
    private rows;
    currentUser: UserRow;
    stateChanged: Subject<UserState>;
    rowsUpdated: Subject<UserRow[]>;
    rowsAdded: Subject<UserRow[]>;
    rowsDeleted: Subject<UserRow[]>;
    constructor(http: HttpService);
    /**
     * Get users from the server
     * @returns Array of UserRow
     */
    getUsers(): UserRow[];
    /**
     * Get connected users
     * @returns Array of userRow
     */
    getConnectedUsers(): Promise<ClientInfo[]>;
    /**
     * Get profiles
     * @returns Array of profiles
     */
    getProfiles(): Promise<ProfileRow[]>;
    /**
     * Get all user from a profile
     * @param profileName Profile name
     * @returns Array of UserRow
     */
    getUsersByProfileName(profileName: string): Promise<UserRow[]>;
    /**
     * Get user by id
     * @param userId User id
     * @returns UserRow
     */
    getUserById(userId: number): UserRow;
    /**
     * Delete user
     * @param user User to delete
     */
    deleteUser(user: UserRow): Promise<void>;
    /**
     * Add new user
     * @param user User to add
     * @returns Id of the new user
     */
    addUser(user: UserRow): Promise<number>;
    /**
     * Update profile
     * @param profile ProfileRow to update
     */
    updateProfile(profile: ProfileRow): Promise<void>;
    /**
     * Check rights of the current connected user
     * @param right Right name
     * @returns True or false
     */
    checkRight(right: string): boolean;
    /**
     * Change pasword of an user
     * @param username Username
     * @param oldPassword Old password
     * @param newPassword New password
     */
    changePassword(username: string, oldPassword: string, newPassword: string): Promise<void>;
}
