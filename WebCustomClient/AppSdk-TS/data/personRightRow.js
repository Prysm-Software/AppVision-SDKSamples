"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.PersonRightRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
class PersonRightRow {
    constructor(p) {
        this.ValidityStart = '';
        this.ValidityEnd = '';
        this.ValidityInfo = '';
        Object.assign(this, p);
    }
    get validityStart() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityStart);
    }
    get validityEnd() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityEnd);
    }
}
exports.PersonRightRow = PersonRightRow;
