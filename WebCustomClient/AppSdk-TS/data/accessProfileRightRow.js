"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AccessProfileRightRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
class AccessProfileRightRow {
    constructor(r) {
        Object.assign(this, r);
    }
    get validityStart() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityStart);
    }
    get validityEnd() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityEnd);
    }
}
exports.AccessProfileRightRow = AccessProfileRightRow;
