"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AccessProfileRow = void 0;
const accessProfileRightRow_1 = require("./accessProfileRightRow");
class AccessProfileRow {
    constructor(p) {
        var _a;
        this.Id = 0;
        this.Name = '';
        this.Description = '';
        Object.assign(this, p);
        if (((_a = p.AccessProfileRights) === null || _a === void 0 ? void 0 : _a.length) > 0) {
            this.AccessProfileRights = p.AccessProfileRights.map(apr => new accessProfileRightRow_1.AccessProfileRightRow(apr));
        }
    }
}
exports.AccessProfileRow = AccessProfileRow;
