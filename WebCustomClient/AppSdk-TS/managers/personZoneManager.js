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
exports.PersonZoneManager = void 0;
const helper_1 = require("../common/helper");
const personZone_1 = require("../data/personZone");
class PersonZoneManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all person zone
     * @param options Request options. Allow to set expand parameters and return linked property.
     * @returns Array of PersonZone
     *
     * @example
     * Request person with person and zone:
     * server.personZoneManager.getAll({expands: ['Zone', 'Person']})
     */
    getAll(options = {}) {
        var _a, _b, _c;
        return __awaiter(this, void 0, void 0, function* () {
            const expands = (options === null || options === void 0 ? void 0 : options.expands.join(',')) || '';
            let filter = [];
            if (((_a = options === null || options === void 0 ? void 0 : options.filterZone) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                filter = [...filter, ...options.filterZone.map(id => 'ZoneId eq ' + id)];
            }
            if (((_b = options === null || options === void 0 ? void 0 : options.filterCategories) === null || _b === void 0 ? void 0 : _b.length) > 0) {
                filter = [...filter, ...options.filterCategories.map(id => 'Person/CategoryId eq ' + id)];
            }
            if (((_c = options === null || options === void 0 ? void 0 : options.filterDepartments) === null || _c === void 0 ? void 0 : _c.length) > 0) {
                filter = [...filter, ...options.filterDepartments.map(id => 'Person/DepartmentId eq ' + id)];
            }
            const resp = yield this.http.get('AppControlDataService.svc/PersonZones()?$orderby=Person/LastName&$expand=' + expands + '&$filter=' + filter.join(' and '));
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(pz => new personZone_1.PersonZone(pz));
        });
    }
    /**
     * Add new personZone
     * @param personZone PersonZone to add
     * @returns PersonZone created
     */
    add(personZone) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(personZone);
            odataRow["odata.type"] = "Prysm.AppControl.Data.PersonZone";
            const resp = yield this.http.post('AppControlDataService.svc/PersonZones', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new personZone_1.PersonZone(json);
        });
    }
    /**
     * Delete personZone
     * @param personZone PersonZone to delete
     */
    delete(personZone) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/PersonZones(PersonId=' + personZone.PersonId + ',ZoneId=' + personZone.ZoneId + ')', null, 'DELETE');
        });
    }
}
exports.PersonZoneManager = PersonZoneManager;
