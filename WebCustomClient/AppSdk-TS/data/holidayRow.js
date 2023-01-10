"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.HolidayRow = void 0;
class HolidayRow {
    constructor(state) {
        this.Id = 0;
        this.Culture = '';
        this.Extension = '';
        this.CustomExtension = '';
        this.Url = '';
        Object.assign(this, state);
    }
    get date() {
        return new Date(parseInt(this.Date.replace(/[^0-9 +]/g, '')));
    }
}
exports.HolidayRow = HolidayRow;
