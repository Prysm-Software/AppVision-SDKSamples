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
exports.AccessProfileManager = void 0;
const helper_1 = require("../common/helper");
const accessProfileRightRow_1 = require("../data/accessProfileRightRow");
const accessProfileRow_1 = require("../data/accessProfileRow");
class PersonProfileRow {
    constructor(p) {
        Object.assign(this, p);
        if (p.AccessProfile) {
            this.AccessProfile = new accessProfileRow_1.AccessProfileRow(p.AccessProfile);
        }
    }
}
class AccessProfileManager {
    constructor(http, rightManager) {
        this.http = http;
        this.rightManager = rightManager;
    }
    /**
     * Get all access profile from the server
     * @returns Array of accessProfile
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/AccessProfiles?$expand=AccessProfileRights,Categories,Departments');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(a => new accessProfileRow_1.AccessProfileRow(a));
        });
    }
    /**
     * Add new access profile with linked prop AccessProfileRights
     * @param profile Profile to add
     * @returns Array of AccessProfile
     */
    add(profile) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(profile);
            odataRow["odata.type"] = "Prysm.AppControl.Data.AccessProfile";
            const resp = yield this.http.post('AppControlDataService.svc/AccessProfiles', JSON.stringify(odataRow));
            const json = yield resp.json();
            const profileCreated = new accessProfileRow_1.AccessProfileRow(json);
            profileCreated.AccessProfileRights = [];
            if (((_a = profile.AccessProfileRights) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                for (const r of profile.AccessProfileRights) {
                    r.AccessProfileId = profileCreated.Id;
                    const rightCreated = yield this.rightManager.addAccessProfileRight(r);
                    profileCreated.AccessProfileRights.push(rightCreated);
                }
            }
            return profileCreated;
        });
    }
    /**
     * Update access profile
     * @param profile Profile to update
     * @param updateRights If true, delete and update the linked rights. Default is False
     */
    update(profile, updateRights = false) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(profile);
            odataRow["odata.type"] = "Prysm.AppControl.Data.AccessProfile";
            if (updateRights) {
                const resp = yield this.http.get('AppControlDataService.svc/AccessProfileRights?$filter=AccessProfileId eq ' + profile.Id);
                if (resp.status !== 204) {
                    const json = yield resp.json();
                    const oldRights = json.value.map(a => new accessProfileRightRow_1.AccessProfileRightRow(a));
                    for (const r of oldRights) {
                        yield this.rightManager.deleteAccessProfileRight(r);
                    }
                }
            }
            yield this.http.post('AppControlDataService.svc/AccessProfiles(' + profile.Id + ')', JSON.stringify(odataRow), 'MERGE');
            if (updateRights && ((_a = profile.AccessProfileRights) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                for (const r of profile.AccessProfileRights) {
                    yield this.rightManager.addAccessProfileRight(r);
                }
            }
        });
    }
    /**
     * Delete access profile
     * @param profileId Access profile id
     */
    delete(profileId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/AccessProfiles(' + profileId + ')', null, 'DELETE');
        });
    }
    /**
     * Get all profile from a person
     * @param personId Person id
     * @returns Array of access profile
     */
    getPersonProfiles(personId) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People(' + personId + ')/PersonProfiles?$expand=AccessProfile');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => p.AccessProfile);
        });
    }
    /**
     * Get category and department profile of a person
     * @param person Person (the method use the DepartmentId and CategoryId)
     * @returns Array of access profile
     */
    getInheritedPersonProfiles(person) {
        return __awaiter(this, void 0, void 0, function* () {
            let profiles = [];
            if (person.DepartmentId != null) {
                const resp = yield this.http.get('AppControlDataService.svc/Departments(' + person.DepartmentId + ')/AccessProfile?$expand=AccessProfileRights');
                if (resp.status !== 204) {
                    const json = yield resp.json();
                    profiles.push(new accessProfileRow_1.AccessProfileRow(json));
                }
            }
            if (person.CategoryId != null) {
                const resp = yield this.http.get('AppControlDataService.svc/Categories(' + person.CategoryId + ')/AccessProfile?$expand=AccessProfileRights');
                if (resp.status !== 204) {
                    const json = yield resp.json();
                    profiles.push(new accessProfileRow_1.AccessProfileRow(json));
                }
            }
            return profiles;
        });
    }
    /**
     * Add access profile to a person
     * @param personId Id of the person
     * @param profileId Id of the profile
     * @returns PersonProfileRow created
     */
    addPersonProfile(personId, profileId) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = {
                AccessProfileId: profileId,
                PersonId: personId
            };
            odataRow["odata.type"] = "Prysm.AppControl.Data.PersonProfile";
            const resp = yield this.http.post('AppControlDataService.svc/People(' + personId + ')/PersonProfiles', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new PersonProfileRow(json);
        });
    }
    /**
     * Delete profile from a person
     * @param personId Person id
     * @param profileId Profile id
     */
    deletePersonProfile(personId, profileId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/PersonProfiles(AccessProfileId=' + profileId + ',PersonId=' + personId + ')', null, 'DELETE');
        });
    }
}
exports.AccessProfileManager = AccessProfileManager;
