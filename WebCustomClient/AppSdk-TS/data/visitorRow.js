"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VisitorRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
const personRow_1 = require("./personRow");
class VisitorRow {
    constructor(row) {
        this.Id = 0;
        this.FirstName = '';
        this.LastName = '';
        this.Info1 = '';
        this.Info2 = '';
        this.Info3 = '';
        this.Info4 = '';
        this.Info5 = '';
        this.Info6 = '';
        this.Info7 = '';
        this.Info8 = '';
        this.Info9 = '';
        Object.assign(this, row);
        if (row.Person) {
            this.Person = new personRow_1.PersonRow(row.Person);
        }
    }
    get dateModification() {
        return dateHelper_1.DateHelper.stringToDate(this.DateModification);
    }
    get dateCreation() {
        return dateHelper_1.DateHelper.stringToDate(this.DateCreation);
    }
    get dateValidation() {
        return dateHelper_1.DateHelper.stringToDate(this.DateValidation);
    }
    get validityStart() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityStart);
    }
    get validityEnd() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityEnd);
    }
}
exports.VisitorRow = VisitorRow;
