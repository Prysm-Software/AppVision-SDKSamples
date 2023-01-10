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
exports.AreaManager = void 0;
const rxjs_1 = require("rxjs");
const areaRow_1 = require("../data/areaRow");
const areaState_1 = require("../data/areaState");
const treeRow_1 = require("../data/treeRow");
class AreaManager {
    constructor(http) {
        this.http = http;
        this.rowsUpdated = new rxjs_1.Subject();
        this.statesChanged = new rxjs_1.Subject();
    }
    get count() { return this.areas.length; }
    /**
     * Build area tree
     * @returns Array of TreeRow
     */
    getAreaTree() {
        const copy = this.areas.map(a => new areaRow_1.AreaRow(a));
        return treeRow_1.TreeRow.init(copy);
    }
    /**
     * Get an area by Id
     * @param id Area id
     * @returns AreaRow
     */
    getAreaById(id) {
        const area = this.areas.find(a => a.Id === id);
        if (area)
            return new areaRow_1.AreaRow(area);
        return null;
    }
    /**
     * Get an area by name
     * @param name Area name (with or without the '$A.')
     * @returns AreaRow
     */
    getAreaByName(name) {
        if (name.startsWith('$A.'))
            name = name.substring(3);
        const area = this.areas.find(a => a.Name.toLowerCase() === name.toLowerCase());
        if (area)
            return new areaRow_1.AreaRow(area);
        return null;
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get('AppVisionService.svc/GetAreaStates');
            let json = yield resp.json();
            const states = json.map(s => new areaState_1.AreaState(s));
            resp = yield this.http.get('AppVisionService.svc/GetAreas');
            json = yield resp.json();
            this.areas = json.map(t => new areaRow_1.AreaRow(t));
            this.areas.forEach(a => {
                const state = states.find(s => s.Id === a.Id);
                if (state)
                    a.State = state;
            });
        });
    }
    /** @internal */
    updateRowsInternal(areas) {
        const updatedRows = [];
        areas.forEach(area => {
            const id = this.areas.findIndex(a => a.Id === area.Id);
            if (id >= 0) {
                this.areas[id] = area;
            }
            else {
                this.areas.push(area);
            }
            updatedRows.push(area);
            //if (TreeRow.insertOrUpdateItemInArray(this.areas, area))
            //    updatedRows.push(area);
            //else
            //    console.error("Failed to update area");
        });
        if (updatedRows.length > 0) {
            this.rowsUpdated.next(updatedRows);
        }
    }
    /** @internal */
    updateStatesInternal(states) {
        const updatedStates = [];
        states.forEach(state => {
            const area = this.areas.find(a => a.Id === state.Id);
            //const area = TreeRow.getItemById(this.areas, state.Id);
            if (area) {
                area.State = state;
                updatedStates.push(state);
            }
        });
        if (updatedStates.length > 0) {
            this.statesChanged.next(updatedStates);
        }
    }
}
exports.AreaManager = AreaManager;
