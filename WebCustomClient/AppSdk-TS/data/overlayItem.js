"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.OverlayItem = void 0;
const dateHelper_1 = require("../common/dateHelper");
const helper_1 = require("../common/helper");
class OverlayItem {
    constructor(obj) {
        Object.assign(this, obj);
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
    get position() {
        try {
            const pos = this.PositionString.split(',');
            if (pos.length === 2) {
                return pos.map(helper_1.Helper.parseFloat);
            }
        }
        catch (_a) { }
        return [0, 0];
    }
    get size() {
        try {
            const s = this.SizeString.split(',');
            if (s.length === 2) {
                return s.map(helper_1.Helper.parseInt);
            }
        }
        catch (_a) { }
        return [0, 0];
    }
}
exports.OverlayItem = OverlayItem;
