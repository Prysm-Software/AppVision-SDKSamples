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
exports.DepartmentManager = void 0;
const helper_1 = require("../common/helper");
const departmentRow_1 = require("../data/departmentRow");
class DepartmentManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all departments
     * @returns Array of DepartmentRow
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Departments');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(d => new departmentRow_1.DepartmentRow(d));
        });
    }
    /**
     * Add departmentRow
     * @param department DepartmentRow to add
     * @returns DepartmentRow created
     */
    add(department) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(department);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Department";
            const resp = yield this.http.post('AppControlDataService.svc/Departments', JSON.stringify(odataRow));
            const json = yield resp.json();
            return new departmentRow_1.DepartmentRow(json);
        });
    }
    /**
     * Delete departmentRow
     * @param id DepartmentRow Id
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/Departments(' + id + ')', null, 'DELETE');
        });
    }
    /**
     * Update departmentRow
     * @param department DepartmentRow to update
     */
    update(department) {
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(department);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Department";
            yield this.http.post('AppControlDataService.svc/Departments(' + department.Id + ')', JSON.stringify(odataRow), 'MERGE');
        });
    }
}
exports.DepartmentManager = DepartmentManager;
