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
exports.HolidayManager = void 0;
const holidayRow_1 = require("../data/holidayRow");
class HolidayManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all Hollidays
     * @returns Array of HollidayRow
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get("AppVisionService.svc/GetHolidays");
            const json = yield resp.json();
            return json.map(h => new holidayRow_1.HolidayRow(h));
        });
    }
    /**
     * Add new holiday
     * @param row HolidayRow to add
     * @returns Id of the holidayRow created
     */
    add(row) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.post("AppVisionService.svc/AddHoliday", JSON.stringify(row), 'POST');
            const json = yield resp.text();
            return parseInt(json);
        });
    }
    /**
     * Delete holiday
     * @param row Holiday to delete
     */
    delete(row) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.post("AppVisionService.svc/DeleteHoliday", JSON.stringify(row));
        });
    }
}
exports.HolidayManager = HolidayManager;
