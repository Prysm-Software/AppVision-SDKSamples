"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserManager = void 0;
const userRow_1 = require("../data/userRow");
const clientInfo_1 = require("../data/clientInfo");
const profileRow_1 = require("../data/profileRow");
const rxjs_1 = require("rxjs");
class UserManager {
    constructor(http) {
        this.http = http;
        this.rows = new Map();
        this.stateChanged = new rxjs_1.Subject();
        this.rowsUpdated = new rxjs_1.Subject();
        this.rowsAdded = new rxjs_1.Subject();
        this.rowsDeleted = new rxjs_1.Subject();
    }
    /**
     * Get users from the server
     * @returns Array of UserRow
     */
    getUsers() {
        // const resp = await this.http.get('AppVisionService.svc/GetUsers');
        // const userResp = await resp.json();
        // return (userResp as UserRow[]).map(u => new UserRow(u));
        return [...this.rows.values()];
    }
    /**
     * Get connected users
     * @returns Array of userRow
     */
    getConnectedUsers() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/GetUsersConnected');
            const clientInfoResp = yield resp.json();
            return clientInfoResp.map(u => new clientInfo_1.ClientInfo(u));
        });
    }
    /**
     * Get profiles
     * @returns Array of profiles
     */
    getProfiles() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/GetProfiles');
            const profileResp = yield resp.json();
            return profileResp.map(p => new profileRow_1.ProfileRow(p));
        });
    }
    /**
     * Get all user from a profile
     * @param profileName Profile name
     * @returns Array of UserRow
     */
    getUsersByProfileName(profileName) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/GetUsersByProfile?name=' + profileName);
            const userResp = yield resp.json();
            return userResp.map(u => new userRow_1.UserRow(u));
        });
    }
    /**
     * Get user by id
     * @param userId User id
     * @returns UserRow
     */
    getUserById(userId) {
        // const resp = await this.http.get('AppVisionService.svc/GetUserById?id=' + userId);
        // const userResp = await resp.json();
        // return new UserRow(userResp);
        return this.rows.get(userId);
    }
    /**
     * Delete user
     * @param user User to delete
     */
    deleteUser(user) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppVisionService.svc/DeleteUser', JSON.stringify(user));
        });
    }
    /**
     * Add new user
     * @param user User to add
     * @returns Id of the new user
     */
    addUser(user) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.post('AppVisionService.svc/AddUser', JSON.stringify(user));
            const userIdResp = yield resp.text();
            return parseInt(userIdResp);
        });
    }
    /**
     * Update profile
     * @param profile ProfileRow to update
     */
    updateProfile(profile) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.post('AppVisionService.svc/UpdateProfile', JSON.stringify(profile));
            const json = yield resp.json();
            if (!json)
                throw new Error("Server returned false");
        });
    }
    /**
     * Check rights of the current connected user
     * @param right Right name
     * @returns True or false
     */
    checkRight(right) {
        if (!this.currentUser)
            return false;
        // cas particulier (copier sur le client lourd)
        switch (right) {
            case 'Display.Left':
                return this.currentUser.checkRight("Display.Left.Synoptic") ||
                    this.currentUser.checkRight("Display.Left.Area") ||
                    this.currentUser.checkRight("Display.Left.Group") ||
                    this.currentUser.checkRight("Display.Left.Layout") ||
                    this.currentUser.checkRight("Display.Left.Standard") ||
                    this.currentUser.checkRight("Display.Left.Cycle") ||
                    this.currentUser.checkRight("Display.Left.Video");
            case "Display.Right":
                return this.currentUser.checkRight("Display.Right.Video") || this.currentUser.checkRight("Display.Right.Audio");
            case "Display.Left.Synoptic":
                return this.currentUser.checkRight(right) && this.currentUser.checkRight("Display.Synoptic");
            case "Display.Left.Layout":
                return this.currentUser.checkRight(right) && this.currentUser.checkRight("Client.ChangeLayout");
            case "Display.Left.Cycle":
                return this.currentUser.checkRight(right) && this.currentUser.checkRight("Client.LoadCycle");
            case "Display.Left.Video":
                return this.currentUser.checkRight(right) && this.currentUser.checkRight("Video.Live");
            case "Video.PlaybackOnAlarm":
                return this.currentUser.checkRight("Video.PlaybackOnAlarm") || this.currentUser.checkRight("Video.Playback");
            case "Video.Ptz":
                return this.currentUser.checkRight("Video.PtzMove") || this.currentUser.checkRight("Video.PtzGotoPreset") || this.currentUser.checkRight("Video.PtzRunPattern");
            case "MenuAdministration":
                return this.currentUser.checkRight("Configure.User") || this.currentUser.checkRight("Server.Maintenance");
            default:
        }
        return this.currentUser.checkRight(right);
    }
    /**
     * Change pasword of an user
     * @param username Username
     * @param oldPassword Old password
     * @param newPassword New password
     */
    changePassword(username, oldPassword, newPassword) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppVisionService.svc/ChangePassword', JSON.stringify([
                { Name: "username", Value: username },
                { Name: "oldpassword", Value: oldPassword },
                { Name: "newpassword", Value: newPassword },
            ]), "POST", null);
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            // Load current user
            let resp = yield this.http.get('AppVisionService.svc/GetCurrentUser');
            const userJson = yield resp.json();
            this.currentUser = new userRow_1.UserRow(userJson);
            // Load current user profile
            resp = yield this.http.get('AppVisionService.svc/GetProfileById?id=' + this.currentUser.ProfileId);
            const profileJson = yield resp.json();
            this.currentUser.profile = new profileRow_1.ProfileRow(profileJson);
            // Load all users
            resp = yield this.http.get('AppVisionService.svc/GetUsers');
            const usersJson = yield resp.json();
            const users = usersJson.map(u => new userRow_1.UserRow(u));
            for (const u of users)
                this.rows.set(u.Id, u);
        });
    }
    /** @internal */
    updateRowsInternal(rows) {
        const addedUsers = [];
        const deletedUsers = [];
        const updatedUsers = [];
        for (const user of rows) {
            if (user.Id < 0) {
                this.rows.delete(Math.abs(user.Id));
                deletedUsers.push(user);
            }
            else {
                if (this.rows.has(user.Id)) {
                    updatedUsers.push(user);
                }
                else {
                    addedUsers.push(user);
                }
                this.rows.set(user.Id, user);
            }
        }
        if (addedUsers.length > 0)
            this.rowsAdded.next(addedUsers);
        if (deletedUsers.length > 0)
            this.rowsDeleted.next(deletedUsers);
        if (updatedUsers.length > 0)
            this.rowsUpdated.next(updatedUsers);
    }
    /** @internal */
    updateStateInternal(state) {
        this.currentUser.State = state;
        this.stateChanged.next(state);
    }
}
exports.UserManager = UserManager;
