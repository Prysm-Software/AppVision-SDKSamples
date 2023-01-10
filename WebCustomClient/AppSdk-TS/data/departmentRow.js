"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DepartmentRow = void 0;
class DepartmentRow {
    constructor(p = null) {
        this.Id = 0;
        this.Name = '';
        this.Description = '';
        this.AccessProfileId = null;
        this.Parameters = '';
        Object.assign(this, p);
    }
}
exports.DepartmentRow = DepartmentRow;
