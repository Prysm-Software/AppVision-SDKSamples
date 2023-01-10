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
exports.BadgeTypeManager = void 0;
const helper_1 = require("../common/helper");
const badgeType_1 = require("../data/badgeType");
class BadgeTypeManager {
    constructor(http, rightManager) {
        this.http = http;
        this.rightManager = rightManager;
    }
    /**
     * Get all badge types
     * @returns Array of BadgeType
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/BadgeTypes');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(t => new badgeType_1.BadgeType(t));
        });
    }
    /**
     * Add new badgeType
     * @param badgeType BadgeType object to add
     * @returns BadgeType created
     */
    add(badgeType) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(badgeType);
            odataRow["odata.type"] = "Prysm.AppControl.Data.BadgeType";
            const resp = yield this.http.post('AppControlDataService.svc/BadgeTypes', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new badgeType_1.BadgeType(json);
        });
    }
    /**
     * Delete a badgeType
     * @param id BadgeType Id
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/BadgeTypes(' + id + ')', null, 'DELETE');
        });
    }
    /**
     * Update badgeType
     * @param badgeType BadgeType to update
     */
    update(badgeType) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(badgeType);
            odataRow["odata.type"] = "Prysm.AppControl.Data.BadgeType";
            yield this.http.post('AppControlDataService.svc/BadgeTypes(' + badgeType.Id + ')', JSON.stringify(odataRow), 'MERGE');
        });
    }
}
exports.BadgeTypeManager = BadgeTypeManager;
