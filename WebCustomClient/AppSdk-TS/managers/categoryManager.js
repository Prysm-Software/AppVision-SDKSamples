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
exports.CategoryManager = void 0;
const helper_1 = require("../common/helper");
const categoryRow_1 = require("../data/categoryRow");
class CategoryManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all categories
     * @returns Array of CategoryRow
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Categories');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(c => new categoryRow_1.CategoryRow(c));
        });
    }
    /**
     * Add new categoryRow
     * @param category
     * @returns CategoryRow created
     */
    add(category) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(category);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Category";
            const resp = yield this.http.post('AppControlDataService.svc/Categories', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new categoryRow_1.CategoryRow(json);
        });
    }
    /**
     * Delete categoryRow
     * @param id CategoryRow Id
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/Categories(' + id + ')', null, 'DELETE');
        });
    }
    /**
     * Update CategoryRow
     * @param category CategoryRow to update
     */
    update(category) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(category);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Category";
            yield this.http.post('AppControlDataService.svc/Categories(' + category.Id + ')', JSON.stringify(odataRow), 'MERGE');
        });
    }
}
exports.CategoryManager = CategoryManager;
