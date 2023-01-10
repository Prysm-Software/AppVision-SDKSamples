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
exports.BadgeManager = void 0;
const helper_1 = require("../common/helper");
const badgeRow_1 = require("../data/badgeRow");
class BadgeManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all badge
     * @returns Array of BadgeRow
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Badges?$expand=Person');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new badgeRow_1.BadgeRow(p));
        });
    }
    /**
     * Add a badge to a person
     * @param personId Person Id
     * @param badge BadgeRow to add
     * @returns BadgeRow created
     */
    addToPerson(personId, badge) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(badge);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Badge";
            const resp = yield this.http.post('AppControlDataService.svc/People(' + personId + ')/Badges', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new badgeRow_1.BadgeRow(json);
        });
    }
    /**
     * Get all badge from a person
     * @param personId Person Id
     * @returns Array of BadgeRow
     */
    getFromPerson(personId) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/People(' + personId + ')/Badges');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.map(b => new badgeRow_1.BadgeRow(b));
        });
    }
    /**
     * Delete a badge
     * @param badge BadgeRow to delete
     *
     * @remark
     * Id is not sufficient so we need to send the whole object
     */
    delete(badge) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/Badges(BadgeTypeId=' + badge.BadgeTypeId + ',Code=\'' + badge.Code + '\',Format=\'' + badge.Format + '\')', null, 'DELETE');
        });
    }
    /**
     * Update a badge
     * @param badge BadgeRow to update
     */
    update(badge) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(badge);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Badge";
            yield this.http.post('AppControlDataService.svc/Badges(BadgeTypeId=' + badge.BadgeTypeId + ',Code=\'' + badge.Code + '\',Format=\'' + badge.Format + '\')', JSON.stringify(odataRow), 'PUT');
        });
    }
    /**
     * Get all badge from a badge type
     * @param id BadgeType id
     * @returns Array of BadgeRow
     */
    getFromBadgeType(id) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Badges?$expand=Person&$filter=BadgeTypeId eq ' + id);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new badgeRow_1.BadgeRow(p));
        });
    }
}
exports.BadgeManager = BadgeManager;
