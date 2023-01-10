"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.BadgeRow = void 0;
const helper_1 = require("../common/helper");
const personRow_1 = require("./personRow");
class BadgeRow {
    constructor(b) {
        this.Code = '';
        this.Format = '';
        this.Info = '';
        this.Status = null;
        this.BadgeTypeId = null;
        Object.assign(this, b);
        if (b.Person) {
            this.Person = new personRow_1.PersonRow(b.Person);
        }
    }
    /**
     * Set the status of the BadgeRow instance
     * @param status New status
     */
    setStatus(status) {
        var _a, _b;
        this.Status = status;
        if (status === 0) {
            // badge actif
            const personId = helper_1.Helper.parseInt((_b = (_a = this.Info) === null || _a === void 0 ? void 0 : _a.match(/\((\d+)\)/)) === null || _b === void 0 ? void 0 : _b[1], null); // "lastname firstname (id)"
            if (personId)
                this.PersonId = personId;
        }
        else {
            // retire la personne des badges volés, perdus...
            if (this.Person)
                this.Info = `${this.Person.LastName} ${this.Person.FirstName} (${this.Person.Id})`;
            this.PersonId = null;
        }
    }
}
exports.BadgeRow = BadgeRow;
