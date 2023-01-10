"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AreaRow = void 0;
const treeRow_1 = require("./treeRow");
const areaState_1 = require("./areaState");
class AreaRow extends treeRow_1.TreeRow {
    constructor(row) {
        super(row);
        if (this.State == null)
            this.State = new areaState_1.AreaState({ Id: this.Id });
    }
}
exports.AreaRow = AreaRow;
