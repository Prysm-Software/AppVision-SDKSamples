"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TreeRow = void 0;
const stringHelper_1 = require("../common/stringHelper");
class TreeRow {
    constructor(row) {
        Object.assign(this, row);
        this.items = [];
        this.parent = null;
    }
    isChild(id) {
        if (id === this.Id)
            return true;
        for (const child of this.items) {
            if (child.isChild(id)) {
                return true;
            }
        }
        return false;
    }
    // construit l'arborescence
    static init(rows, separator = '.') {
        const map = new Map();
        rows.forEach(row => map.set(row.Name, row));
        rows.filter(row => row.Name.includes(separator))
            .forEach(row => {
            const parentName = stringHelper_1.StringHelper.getParentName(row.Name, separator);
            const parent = map.get(parentName);
            if (parent) {
                row.parent = parent;
                if (!parent.items)
                    parent.items = [];
                parent.items.push(row);
            }
        });
        return rows.filter(a => a.parent == null);
    }
    // Return a deep clone of a TreeRow array (Clone parent and items too)
    static deepCloneArray(rows, ctor) {
        const tmp = [];
        rows.forEach((a) => {
            const tmpArea = new ctor(a);
            tmp.push(tmpArea);
            if (a.parent) {
                tmpArea.parent = new ctor(a);
            }
            if (a.items && a.items.length > 0) {
                tmpArea.items = this.deepCloneArray(a.items, ctor);
            }
        });
        return tmp;
    }
    // Insert or update a treerow inside an array
    static insertOrUpdateItemInArray(rows, newRow, currentLevel = 1) {
        const level = newRow.Name.split('.').length;
        if (currentLevel === level) {
            const i = rows.findIndex(r => r.Id === newRow.Id);
            if (i >= 0)
                rows[i] = newRow;
            else
                rows.push(newRow);
            return true;
        }
        else {
            const parent = rows.find(r => newRow.Name.startsWith(r.Name));
            if (parent)
                return TreeRow.insertOrUpdateItemInArray(parent.items, newRow, parent.Name.split('.').length + 1);
        }
        return false;
    }
    // Return an item by its id searching inside the tree of rows
    static getItemById(rows, id) {
        let row = rows.find(r => r.Id === id);
        if (row) {
            return row;
        }
        else if (row == null) {
            for (let i = 0; i < rows.length; i++) {
                if (rows[i].items && rows[i].items.length > 0) {
                    row = TreeRow.getItemById(rows[i].items, id);
                    if (row) {
                        return row;
                    }
                }
            }
        }
        else {
            return null;
        }
    }
    static getItemByName(rows, name) {
        var _a;
        let row = rows.find(r => r.Name === name);
        if (row)
            return row;
        for (const r of rows) {
            if (((_a = r.items) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                row = TreeRow.getItemByName(r.items, name);
                if (row)
                    return row;
            }
        }
        return null;
    }
}
exports.TreeRow = TreeRow;
