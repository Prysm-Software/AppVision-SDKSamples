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
exports.VisitorManager = void 0;
const dateHelper_1 = require("../common/dateHelper");
const helper_1 = require("../common/helper");
const badgeRow_1 = require("../data/badgeRow");
const visitorRow_1 = require("../data/visitorRow");
class VisitorManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all visitors
     * @param options Option request. Allow to request linked property on visitors like Person, Category ...
     * @returns Array of visitors
     */
    getAll(options = {}) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const expandsParams = ((_a = options === null || options === void 0 ? void 0 : options.expands) === null || _a === void 0 ? void 0 : _a.join(',')) || '';
            const resp = yield this.http.get('AppControlDataService.svc/Visitors?$expand=' + expandsParams);
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(p => new visitorRow_1.VisitorRow(p));
        });
    }
    /**
     * Get a single visitor
     * @param id Id of the visitor
     * @param options Option request. Allow to request linked property on visitors like Person, Category ...
     * @returns VisitorRow
     */
    getById(id, options = {}) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const expandsParams = ((_a = options === null || options === void 0 ? void 0 : options.expands) === null || _a === void 0 ? void 0 : _a.join(',')) || '';
            const resp = yield this.http.get('AppControlDataService.svc/Visitors(' + id + ')?$expand=' + expandsParams);
            const json = yield resp.json();
            return new visitorRow_1.VisitorRow(json);
        });
    }
    /**
     * Count visitors
     * @returns Number of visitors
     */
    count() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Visitors/$count', { 'Accept': '*/*' });
            const result = yield resp.text();
            return parseInt(result);
        });
    }
    /**
     * Add a visitor (automatically create the person if needed)
     * @param visitor VisitorRow to add
     * @param badge BadgeRow (optionnal). If BadgeRow is not null, the personRow will automatically be created
     */
    add(visitor, badge = null) {
        return __awaiter(this, void 0, void 0, function* () {
            const rg = new RegExp('"', 'g');
            if (!badge)
                badge = new badgeRow_1.BadgeRow({});
            if (!badge.Status)
                badge.Status = 0;
            visitor.DateCreation = dateHelper_1.DateHelper.toDateTimeString(new Date());
            visitor.DateModification = dateHelper_1.DateHelper.toDateTimeString(new Date());
            visitor.DateValidation = dateHelper_1.DateHelper.toDateTimeString(new Date());
            const body = `{
			"Visitor": "${JSON.stringify(helper_1.Helper.lightCloneObject(visitor)).replace(rg, "'")}",
			"Badge": "${JSON.stringify(helper_1.Helper.lightCloneObject(badge)).replace(rg, "'")}"
		}`;
            const resp = yield this.http.post('AppControlDataService.svc/AddVisitor', body);
            const json = yield resp.json();
            const created = yield this.getById(parseInt(json.value), { expands: ['Person', 'Person/Badges'] });
            return created;
        });
    }
    /**
     * Update an existing visitor
     * @param visitor Visitor to update
     * @param badge Badge of the visitor if any (optional)
     * @returns Visitor updated
     */
    update(visitor, badge = null) {
        return __awaiter(this, void 0, void 0, function* () {
            const rg = new RegExp('"', 'g');
            if (!badge)
                badge = new badgeRow_1.BadgeRow({});
            if (!badge.Status)
                badge.Status = 0;
            const body = `{
			"Visitor": "${JSON.stringify(helper_1.Helper.lightCloneObject(visitor)).replace(rg, "'")}",
			"Badge": "${JSON.stringify(helper_1.Helper.lightCloneObject(badge)).replace(rg, "'")}"
		}`;
            yield this.http.post('AppControlDataService.svc/UpdateVisitor', body);
            const created = yield this.getById(visitor.Id, { expands: ['Person', 'Person/Badges'] });
            return created;
        });
    }
    /**
     * Delete a visitor
     * @param id Id of the visitor
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/RemoveVisitor?id=' + id, null);
        });
    }
}
exports.VisitorManager = VisitorManager;
