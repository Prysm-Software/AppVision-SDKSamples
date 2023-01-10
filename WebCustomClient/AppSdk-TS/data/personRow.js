"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.PersonRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
const badgeRow_1 = require("./badgeRow");
const categoryRow_1 = require("./categoryRow");
const personRightRow_1 = require("./personRightRow");
class PersonRow {
    constructor(p) {
        var _a, _b;
        this.FirstName = '';
        this.LastName = '';
        this.CompanyId = '';
        this.Address = '';
        this.Phone = '';
        this.Email = '';
        this.Photo = '';
        this.Notes = '';
        this.PinCode = '';
        this.Info1 = '';
        this.Info2 = '';
        this.Info3 = '';
        this.Info4 = '';
        this.Info5 = '';
        this.Info6 = '';
        this.Info7 = '';
        this.Info8 = '';
        this.Info9 = '';
        this.Id = 0;
        this.CategoryId = null;
        this.DepartmentId = null;
        this.IsActive = this.IsAntipassback = true;
        this.Badges = [];
        Object.assign(this, p);
        if (((_a = p.Badges) === null || _a === void 0 ? void 0 : _a.length) > 0) {
            this.Badges = p.Badges.map(b => new badgeRow_1.BadgeRow(b));
        }
        if (p.Category) {
            this.Category = new categoryRow_1.CategoryRow(p.Category);
        }
        if (p.Department) {
            this.Department = new categoryRow_1.CategoryRow(p.Department);
        }
        if (((_b = p.PersonRights) === null || _b === void 0 ? void 0 : _b.length) > 0) {
            this.PersonRights = p.PersonRights.map(r => new personRightRow_1.PersonRightRow(r));
        }
    }
    get validityStart() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityStart);
    }
    get validityEnd() {
        return dateHelper_1.DateHelper.stringToDate(this.ValidityEnd);
    }
    get FullName() {
        return (this.LastName != null ? this.LastName : "") + " " + (this.FirstName != null ? this.FirstName : "");
    }
}
exports.PersonRow = PersonRow;
