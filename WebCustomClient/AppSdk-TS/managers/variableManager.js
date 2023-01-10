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
exports.VariableManager = void 0;
const rxjs_1 = require("rxjs");
const variableRow_1 = require("../data/variableRow");
const variableState_1 = require("../data/variableState");
const valueRow_1 = require("../data/valueRow");
const treeRow_1 = require("../data/treeRow");
const helper_1 = require("../common/helper");
class VariableManager {
    constructor(http) {
        this.http = http;
        this.rowsAdded = new rxjs_1.Subject();
        this.rowsUpdated = new rxjs_1.Subject();
        this.rowsDeleted = new rxjs_1.Subject();
        this.statesChanged = new rxjs_1.Subject();
    }
    /**
     * Set variable
     * @param varName Variable name
     * @param varValue Variable value
     * @param info Info field
     */
    set(varName, varValue, info = null, params = null) {
        return __awaiter(this, void 0, void 0, function* () {
            if (!params)
                params = [];
            params.push({ Name: "@name", Value: varName });
            params.push({ Name: "@value", Value: varValue });
            if (info !== null)
                params.push({ Name: "@info", Value: info });
            yield this.http.post('AppVisionService.svc/SetVariableExt', JSON.stringify(params));
        });
    }
    // /**
    //  * Set variable ext
    //  * @param params Array of param as namevalue array
    //  */
    // public async setExt(params: NameValue[]) {
    // 	let uri = 'AppVisionService.svc/SetVariableExt';
    // 	await this.http.post(uri, JSON.stringify(params));
    // }
    /**
     * Lock or unlock variable
     * @param varName VariableName
     * @param isLocked Lock or unlock
     * @param params NameValue parameters
     */
    lock(varName, isLocked, params) {
        return __awaiter(this, void 0, void 0, function* () {
            const uri = `AppVisionService.svc/LockVariableExt?name=${varName}&isLocked=${isLocked}`;
            yield this.http.post(encodeURI(uri), JSON.stringify(params));
        });
    }
    /**
     * Updates a VariableRow
     * @param row VariableRow to be updated.
     * @returns true if the variable has been successfully updated.
     */
    updateVariable(row) {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = 'AppVisionService.svc/UpdateVariable';
            const clone = helper_1.Helper.lightCloneObject(row);
            const response = yield this.http.post(encodeURI(uri), JSON.stringify(clone));
            const json = yield response.json();
            return json;
        });
    }
    /**
     * Add new VariableRow
     * @param row VariableRow to add
     * @returns VariableRow created
     */
    add(row) {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.post("AppVisionService.svc/AddVariable2", JSON.stringify(row), 'POST');
            const json = yield resp.json();
            return new variableRow_1.VariableRow(json);
        });
    }
    /**
     * Add an array of rows
     * @param rows
     * @returns true ou false
     */
    adds(rows) {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.post("AppVisionService.svc/AddVariables", JSON.stringify(rows), 'POST');
            const json = yield resp.json();
            return json;
        });
    }
    /**
     * Delete a row
     * @param row
     * @returns true ou false
     */
    delete(row) {
        return __awaiter(this, void 0, void 0, function* () {
            const clone = helper_1.Helper.lightCloneObject(row);
            let resp = yield this.http.post("AppVisionService.svc/DeleteVariable", JSON.stringify(clone), 'POST');
            const json = yield resp.json();
            return json;
        });
    }
    /**
     * Get all variable rows
     * @returns
     */
    getRows() {
        return [...this.rowsById.values()];
    }
    /**
     * Get all variable states
     * @returns
     */
    getStates() {
        return [...this.states.values()];
    }
    /**
     * Get row by id
     * @param id Variable id
     * @returns Variable row
     *
     * @remark
     * Get value from cache
     */
    getRowById(id) {
        return this.rowsById.get(id);
    }
    /**
     * Get row by name
     * @param varName Variable name
     * @returns Variable row
     *
     * @remark
     * Get value from cache
     */
    getRowByName(varName) {
        if (varName.startsWith('$V.') || varName.startsWith('$v.')) {
            varName = varName.substring(3);
        }
        return this.rowsByName.get(varName);
    }
    /**
     * Get variable state by variable name
     * @param varName Variable name
     * @returns Variable state
     *
     * @remark
     * Get value from cache
     */
    getStateByName(varName) {
        if (varName.startsWith('$V.') || varName.startsWith('$v.')) {
            varName = varName.substring(3);
        }
        return this.states.get(varName);
    }
    /**
     * Get variable state by variable id
     * @param varId Variable id
     * @returns Variable state
     *
     * @remark
     * Get value from cache
     */
    getStateById(varId) {
        const row = this.rowsById.get(varId);
        if (row == null)
            return null;
        return this.getStateByName(row.Name);
    }
    /**
     * Get rows by type
     * @param rowType Variable row type
     * @returns Array of variables
     *
     * @remark
     * Get value from cache
     */
    getRowsByType(rowType) {
        return this.getRows().filter(v => v.Type === rowType).map(v => new variableRow_1.VariableRow(v));
    }
    /**
     * Get variable states history
     * @param dateStart Filter date start
     * @param dateEnd  Filter date end
     * @param variableNames Filter variable names
     * @param maxCount Max count result. Default is 2500
     * @returns Array of variable rows
     */
    getHistoValues(dateStart, dateEnd, variableNames, maxCount = 2500) {
        return __awaiter(this, void 0, void 0, function* () {
            const uri = `AppVisionService.svc/GetHistoValues?` +
                `index=0` +
                `&count=${maxCount}` +
                `&dateStart=${dateStart.toISOString()}` +
                `&dateEnd=${dateEnd.toISOString()}` +
                `&names=${variableNames.join(',')}`;
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            return json.map(a => new valueRow_1.ValueRow(a));
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get('AppVisionService.svc/GetVariableStates');
            let json = yield resp.json();
            this.states = new Map();
            for (const s of json)
                this.states.set(s.Name, new variableState_1.VariableState(s));
            resp = yield this.http.get('AppVisionService.svc/GetVariables');
            json = yield resp.json();
            this.rowsByName = new Map();
            this.rowsById = new Map();
            for (const r of json) {
                const row = new variableRow_1.VariableRow(r);
                const state = this.states.get(r.Name);
                if (state) { // liaison row <-> state
                    state.Row = row;
                    row.State = state;
                }
                this.rowsByName.set(r.Name, row);
                this.rowsById.set(r.Id, row);
            }
            // liaison .parent .items
            treeRow_1.TreeRow.init([...this.rowsByName.values()]);
        });
    }
    /** @internal */
    updateRowsInternal(rows) {
        return __awaiter(this, void 0, void 0, function* () {
            const added = [];
            const updated = [];
            let deleted = [];
            for (let i = 0; i < rows.length; i++) {
                const row = rows[i];
                const originalRow = this.getRowById(row.Id);
                // Name changed ?
                if (this.getStateByName(row.Name) == null) {
                    const rowToDelete = this.getRows().find(v => v.Id === row.Id);
                    if (rowToDelete)
                        this.rowsByName.delete(rowToDelete.Name);
                }
                // New variable
                if (originalRow == null) {
                    if (row.Type !== variableRow_1.VariableType.Node) {
                        const state = yield this.getVariableState(row.Id);
                        if (state) {
                            row.State = state;
                            state.Row = row;
                            this.states.set(state.Name, state);
                        }
                    }
                    this.rowsByName.set(row.Name, row);
                    this.rowsById.set(row.Id, row);
                    //todo set parent & items
                    added.push(row);
                }
                // Deleted variable
                else if (row.IsExclude === true) {
                    const notifsRow = [row];
                    this.rowsByName.delete(row.Name);
                    this.rowsById.delete(row.Id);
                    this.states.delete(row.Name);
                    //todo retire parent.items
                    if (row.Type === variableRow_1.VariableType.Node) {
                        const rowsByNameToDelete = [...this.rowsByName.keys()].filter(k => k.startsWith(row.Name + '.'));
                        rowsByNameToDelete.forEach(key => this.rowsByName.delete(key));
                        const statesByNameToDelete = [...this.states.keys()].filter(k => k.startsWith(row.Name + '.'));
                        statesByNameToDelete.forEach(key => this.states.delete(key));
                        const rowsByIdToDelete = [...this.rowsById].filter(entry => entry[1].Name.startsWith(row.Name + '.'));
                        rowsByIdToDelete.forEach(entry => {
                            notifsRow.push(entry[1]);
                            this.rowsById.delete(entry[0]);
                        });
                    }
                    deleted = [...deleted, ...notifsRow];
                }
                // Updated variable
                else {
                    // WARNING : FullDescription is empty inside the notif object
                    const state = originalRow.State;
                    if (state) {
                        row.State = state;
                        state.Row = row;
                    }
                    this.rowsByName.set(row.Name, row);
                    this.rowsById.set(row.Id, row);
                    //todo set parent & items
                    updated.push(row);
                }
            }
            if (added.length > 0) {
                this.rowsAdded.next(added);
            }
            if (deleted.length > 0) {
                this.rowsDeleted.next(deleted);
            }
            if (updated.length > 0) {
                this.rowsUpdated.next(updated);
            }
        });
    }
    /** @internal */
    updateStatesInternal(states) {
        const statesChanged = [];
        states.forEach(state => {
            const row = this.rowsByName.get(state.Name);
            if (row) {
                row.State = state;
                state.Row = row;
                this.states.set(state.Name, state);
                statesChanged.push(state);
            }
        });
        if (statesChanged.length > 0) {
            this.statesChanged.next(statesChanged);
        }
    }
    getVariableState(id) {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                const resp = yield this.http.get('AppVisionService.svc/GetVariableStateById?id=' + id);
                const json = yield resp.json();
                return new variableState_1.VariableState(json);
            }
            catch (e) {
                return null;
            }
        });
    }
}
exports.VariableManager = VariableManager;
