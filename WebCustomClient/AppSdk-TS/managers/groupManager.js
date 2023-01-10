"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.GroupManager = void 0;
const rxjs_1 = require("rxjs");
const groupRow_1 = require("../data/groupRow");
const groupState_1 = require("../data/groupState");
const treeRow_1 = require("../data/treeRow");
class GroupManager {
    constructor(http) {
        this.http = http;
        this.rowsUpdated = new rxjs_1.Subject();
        this.statesChanged = new rxjs_1.Subject();
    }
    get count() { return this.groups.length; }
    /**
     * Build GroupRow tree
     * @returns Tree of GroupRow
     */
    getGroupTree() {
        const copy = this.groups.map(a => new groupRow_1.GroupRow(a));
        return treeRow_1.TreeRow.init(copy);
    }
    /**
     * Get group by id
     * @param id Group Id
     * @returns GroupRow
     */
    getGroupById(id) {
        const group = this.groups.find(g => g.Id === id);
        if (group)
            return new groupRow_1.GroupRow(group);
        return null;
    }
    /**
     * Get group by name
     * @param name Groupe name
     * @returns GroupRow
     */
    getGroupByName(name) {
        if (name.startsWith('$G.'))
            name = name.substring(3);
        const group = this.groups.find(g => g.Name.toLowerCase() === name.toLowerCase());
        if (group)
            return new groupRow_1.GroupRow(group);
        return null;
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get('AppVisionService.svc/GetGroupStates');
            let json = yield resp.json();
            const states = json.map(s => new groupState_1.GroupState(s));
            resp = yield this.http.get('AppVisionService.svc/GetGroups');
            json = yield resp.json();
            this.groups = json.map(t => new groupRow_1.GroupRow(t));
            this.groups.forEach(g => {
                const state = states.find(s => s.Id === g.Id);
                if (state)
                    g.State = state;
            });
        });
    }
    /** @internal */
    updateRowsInternal(groups) {
        const updatedRows = [];
        groups.forEach(group => {
            const id = this.groups.findIndex(a => a.Id === group.Id);
            if (id >= 0) {
                this.groups[id] = group;
            }
            else {
                this.groups.push(group);
            }
            updatedRows.push(group);
            //if (TreeRow.insertOrUpdateItemInArray(this.groups, group))
            //    updatedRows.push(group);
            //else
            //    console.error("Failed to update group");
        });
        if (updatedRows.length > 0) {
            this.rowsUpdated.next(updatedRows);
        }
    }
    /** @internal */
    updateStatesInternal(states) {
        const updatedStates = [];
        states.forEach(state => {
            const group = this.groups.find(g => g.Id === state.Id);
            if (group) {
                group.State = state;
                updatedStates.push(state);
            }
        });
        if (updatedStates.length > 0) {
            this.statesChanged.next(updatedStates);
        }
    }
}
exports.GroupManager = GroupManager;
