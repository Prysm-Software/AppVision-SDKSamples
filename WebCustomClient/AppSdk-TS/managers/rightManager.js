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
exports.RightManager = void 0;
const helper_1 = require("../common/helper");
const accessProfileRightRow_1 = require("../data/accessProfileRightRow");
const personRightRow_1 = require("../data/personRightRow");
const personRow_1 = require("../data/personRow");
class RightManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Add right to an access profile
     * @param right Right object
     * @returns Right object created
     */
    addAccessProfileRight(right) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(right);
            odataRow["odata.type"] = "Prysm.AppControl.Data.AccessProfileRight";
            const resp = yield this.http.post('AppControlDataService.svc/AccessProfiles(' + right.AccessProfileId + ')/AccessProfileRights', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new accessProfileRightRow_1.AccessProfileRightRow(json);
        });
    }
    /**
     * Delete rigt from an access profile
     * @param right Right object to delete
     */
    deleteAccessProfileRight(right) {
        return __awaiter(this, void 0, void 0, function* () {
            const obj = 'AccessProfileId=' + right.AccessProfileId + ',CrisisLevel=' + right.CrisisLevel + ',ReaderId=' + right.ReaderId;
            yield this.http.post('AppControlDataService.svc/AccessProfileRights(' + obj + ')', null, 'DELETE');
        });
    }
    /**
     * Get all righs of an access profile
     * @param accessProfileId
     * @returns Array of rights
     */
    getAccessProfileRights(accessProfileId) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/AccessProfiles(' + accessProfileId + ')/AccessProfileRights');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(r => new accessProfileRightRow_1.AccessProfileRightRow(r));
        });
    }
    /**
     * Get array of PersonRightMap from a reader
     * @param readerId Reader id
     * @returns Array of PersonRightMap
     */
    getPeopleRightMapByReader(readerId) {
        return __awaiter(this, void 0, void 0, function* () {
            const rights = [];
            let resp = yield this.http.get('AppControlDataService.svc/AccessProfileRights?$expand=AccessProfile,AccessProfile/People,AccessProfile/People/Person,AccessProfile/Categories,AccessProfile/Categories/People,AccessProfile/Departments,AccessProfile/Departments/People&$filter=ReaderId eq ' + readerId);
            if (resp.status !== 204) {
                const json = yield resp.json();
                const access = json.value;
                for (const a of access) {
                    for (const p of a.AccessProfile.People) {
                        rights.push({
                            accessProfileId: a.AccessProfile.Id,
                            person: new personRow_1.PersonRow(p.Person),
                            ttId: a.TimetableId
                        });
                    }
                    for (const d of a.AccessProfile.Departments) {
                        for (const p of d.People) {
                            rights.push({
                                accessProfileId: a.AccessProfile.Id,
                                person: new personRow_1.PersonRow(p),
                                ttId: a.TimetableId
                            });
                        }
                    }
                    for (const c of a.AccessProfile.Categories) {
                        for (const p of c.People) {
                            rights.push({
                                accessProfileId: a.AccessProfile.Id,
                                person: new personRow_1.PersonRow(p),
                                ttId: a.TimetableId
                            });
                        }
                    }
                }
            }
            resp = yield this.http.get('AppControlDataService.svc/PersonRights?$expand=Person&$filter=ReaderId eq ' + readerId);
            if (resp.status !== 204) {
                const json = yield resp.json();
                const access = json.value;
                for (const o of access) {
                    rights.push({
                        person: new personRow_1.PersonRow(o.Person),
                        ttId: o.TimetableId
                    });
                }
            }
            return rights;
            // return [
            //     ...rights,
            //     ...(json.value as []).map((o: any) => ({
            //         person: new PersonRow(o.Person),
            //         ttId: o.TimetableId
            //     }))
            // ];
        });
    }
    /**
     * Get access exception from a person
     * @param personId Person Id
     * @returns Array of rights
     */
    getPersonAccessExceptions(personId) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People(' + personId + ')/PersonRights');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(r => new personRightRow_1.PersonRightRow(r));
        });
    }
    /**
     * Delete access exception from a person
     * @param right Access exception to delete
     */
    deletePersonAccessException(right) {
        return __awaiter(this, void 0, void 0, function* () {
            const obj = 'CrisisLevel=' + right.CrisisLevel + ',PersonId=' + right.PersonId + ',ReaderId=' + right.ReaderId;
            yield this.http.post('AppControlDataService.svc/PersonRights(' + obj + ')', null, 'DELETE');
        });
    }
    /**
     * Add new access exception to a person
     * @param right
     */
    addPersonAccessException(right) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(right);
            odataRow["odata.type"] = "Prysm.AppControl.Data.PersonRight";
            yield this.http.post('AppControlDataService.svc/People(' + right.PersonId + ')/PersonRights', JSON.stringify(odataRow));
        });
    }
}
exports.RightManager = RightManager;
