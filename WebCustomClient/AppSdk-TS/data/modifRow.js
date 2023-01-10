"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ModifRow = exports.ModifType = void 0;
const dateHelper_1 = require("../common/dateHelper");
var ModifType;
(function (ModifType) {
    ModifType[ModifType["None"] = 0] = "None";
    ModifType[ModifType["Add"] = 1] = "Add";
    ModifType[ModifType["Change"] = 2] = "Change";
    ModifType[ModifType["Delete"] = 4] = "Delete";
})(ModifType = exports.ModifType || (exports.ModifType = {}));
class ModifRow {
    constructor(modif) {
        Object.assign(this, modif);
        //temp tronque le champ Info
        if (this.Info !== null)
            this.Info = this.Info.substring(0, 50);
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
    get type() {
        return ModifType[this.Type];
    }
    ;
}
exports.ModifRow = ModifRow;
