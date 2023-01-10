"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VariableRow = exports.VariableBufferType = exports.VariableType = void 0;
const stringHelper_1 = require("../common/stringHelper");
const treeRow_1 = require("./treeRow");
var VariableType;
(function (VariableType) {
    VariableType[VariableType["Node"] = 0] = "Node";
    VariableType[VariableType["Bool"] = 1] = "Bool";
    VariableType[VariableType["Int"] = 2] = "Int";
    VariableType[VariableType["String"] = 3] = "String";
    VariableType[VariableType["Enum"] = 4] = "Enum";
    VariableType[VariableType["Double"] = 5] = "Double";
    VariableType[VariableType["Timetable"] = 6] = "Timetable";
    VariableType[VariableType["Buffer"] = 7] = "Buffer";
    VariableType[VariableType["Calendar"] = 8] = "Calendar";
    VariableType[VariableType["Reader"] = 10] = "Reader";
    VariableType[VariableType["Video"] = 11] = "Video";
    VariableType[VariableType["Audio"] = 12] = "Audio";
})(VariableType = exports.VariableType || (exports.VariableType = {}));
var VariableBufferType;
(function (VariableBufferType) {
    VariableBufferType[VariableBufferType["Byte"] = 0] = "Byte";
    VariableBufferType[VariableBufferType["SByte"] = 1] = "SByte";
    VariableBufferType[VariableBufferType["UInt16"] = 2] = "UInt16";
    VariableBufferType[VariableBufferType["Int16"] = 3] = "Int16";
    VariableBufferType[VariableBufferType["UInt32"] = 4] = "UInt32";
    VariableBufferType[VariableBufferType["Int32"] = 5] = "Int32";
    VariableBufferType[VariableBufferType["UInt64"] = 6] = "UInt64";
    VariableBufferType[VariableBufferType["Int64"] = 7] = "Int64";
    VariableBufferType[VariableBufferType["Double"] = 9] = "Double";
    VariableBufferType[VariableBufferType["Bit"] = 10] = "Bit";
})(VariableBufferType = exports.VariableBufferType || (exports.VariableBufferType = {}));
class VariableRow extends treeRow_1.TreeRow {
    constructor(row) {
        super(row);
        Object.assign(this, row);
    }
    // construit l'arborescence
    static getTreeByType(items, type) {
        // clone les variables qui nous intéresse
        const vars = items
            .filter(i => i.Type === 0 || i.Type === type)
            .map(i => new VariableRow(i));
        // construit l'arborescence
        const map = new Map();
        vars.forEach(i => map.set(i.Name, i));
        vars.filter(i => i.Name.includes('.'))
            .forEach(i => {
            const parentName = stringHelper_1.StringHelper.getParentName(i.Name, '.');
            const parent = map.get(parentName);
            if (parent) {
                i.parent = parent;
                if (!parent.items)
                    parent.items = [];
                parent.items.push(i);
            }
        });
        //todo retire les dossiers vides
        // renvoie le 1er niveau
        return vars.filter(i => i.parent == null);
    }
    hasLeaf() {
        if (this.Type === 0)
            return this.items.some(i => i.hasLeaf());
        return true;
    }
}
exports.VariableRow = VariableRow;
