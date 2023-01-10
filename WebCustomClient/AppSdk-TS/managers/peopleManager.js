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
exports.PeopleManager = void 0;
const helper_1 = require("../common/helper");
const personRow_1 = require("../data/personRow");
class PeopleManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all people
     * @param options Request options. Allow to set expand parameters and return linked property.
     * @returns Array of person row
     *
     * @example
     * Request people with badges :
     * server.peopleManager.getAll({expands: ['Badges']})
     */
    getAll(options = {}) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const expandsParams = ((_a = options === null || options === void 0 ? void 0 : options.expands) === null || _a === void 0 ? void 0 : _a.join(',')) || '';
            const resp = yield this.http.get('AppControlDataService.svc/People?$expand=' + expandsParams);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new personRow_1.PersonRow(p));
        });
    }
    /**
     * Count people
     * @returns People number
     */
    count() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People/$count', { 'Accept': '*/*' });
            const result = yield resp.text();
            return parseInt(result);
        });
    }
    /**
     * Add new person
     * @param person PersonRow to add
     * @returns PersonRow added
     */
    add(person) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(person);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Person";
            const resp = yield this.http.post('AppControlDataService.svc/People', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new personRow_1.PersonRow(json);
        });
    }
    /**
     * Delete person
     * @param personId Person Id
     */
    delete(personId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/People(' + personId + ')', null, 'DELETE');
        });
    }
    /**
     * Update person
     * @param person PersonRow to update
     */
    update(person) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(person);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Person";
            yield this.http.post('AppControlDataService.svc/People(' + person.Id + ')', JSON.stringify(odataRow), 'MERGE');
        });
    }
    /**
     * Get all people from a department
     * @param id Department id
     * @returns Array of PersonRow
     */
    getFromDepartment(id) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People?$filter=DepartmentId eq ' + id);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new personRow_1.PersonRow(p));
        });
    }
    /**
     * Get all people from a category
     * @param id Category id
     * @returns Array of PersonRow
     */
    getFromCategory(id) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People?$filter=CategoryId eq ' + id);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new personRow_1.PersonRow(p));
        });
    }
    /**
     * Get all people from an access profile
     * @param id Access profile id
     * @returns Array of PersonRow
     */
    getFromAccessProfile(id) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/PersonProfiles?$expand=Person&$filter=AccessProfileId eq ' + id);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map((o) => new personRow_1.PersonRow(o.Person));
        });
    }
}
exports.PeopleManager = PeopleManager;
