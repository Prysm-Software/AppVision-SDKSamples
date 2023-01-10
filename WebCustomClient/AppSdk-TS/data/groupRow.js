"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GroupRow = void 0;
const treeRow_1 = require("./treeRow");
const groupState_1 = require("./groupState");
class GroupRow extends treeRow_1.TreeRow {
    constructor(row) {
        super(row);
        if (this.State == null)
            this.State = new groupState_1.GroupState({ Id: this.Id });
    }
}
exports.GroupRow = GroupRow;
