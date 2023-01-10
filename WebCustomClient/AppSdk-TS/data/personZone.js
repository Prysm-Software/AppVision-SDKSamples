"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.PersonZone = void 0;
const dateHelper_1 = require("../common/dateHelper");
const personRow_1 = require("./personRow");
const zone_1 = require("./zone");
class PersonZone {
    constructor(obj) {
        Object.assign(this, obj);
        if (obj.Zone) {
            this.Zone = new zone_1.Zone(obj.Zone);
        }
        if (obj.Person) {
            this.Person = new personRow_1.PersonRow(obj.Person);
        }
    }
    get dateInput() {
        return dateHelper_1.DateHelper.stringToDate(this.DateInput);
    }
}
exports.PersonZone = PersonZone;
