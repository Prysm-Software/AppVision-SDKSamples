"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ValueRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
class ValueRow {
    constructor(value) {
        Object.assign(this, value);
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
}
exports.ValueRow = ValueRow;
