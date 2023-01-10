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
exports.ScenarioManager = void 0;
const rxjs_1 = require("rxjs");
const scenarioState_1 = require("../data/scenarioState");
const scenarioRow_1 = require("../data/scenarioRow");
class ScenarioManager {
    constructor(http) {
        this.http = http;
        this.statesChanged = new rxjs_1.Subject();
    }
    /**
     * Get all scenarioRow
     */
    get scenarios() {
        return [...this.rows.values()];
    }
    /**
     * Start scenario by id
     * @param scenarioId
     */
    startScenarioById(scenarioId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/ScenarioManager/StartById?id=' + scenarioId);
        });
    }
    /**
     * Stop scenario instance by id
     * @param scenarioId Scenario Id
     * @param instanceId Scenario instance Id
     */
    stopScenarioInstanceById(scenarioId, instanceId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/ScenarioManager/StopById?id=' + scenarioId + '&instanceId=' + instanceId);
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/ScenarioManager/GetActivatedScenarios');
            const json = yield resp.json();
            this.rows = new Map();
            for (let r of json) {
                const row = new scenarioRow_1.ScenarioRow(r);
                if (row.Instances != null) {
                    row.Instances.forEach(i => i.Row = row);
                }
                this.rows.set(row.Id, row);
            }
        });
    }
    /** @internal */
    updateStatesInternal(states) {
        const updatedStates = [];
        states.forEach(state => {
            const row = this.rows.get(state.ScenarioId);
            if (row != null) {
                state.Row = row;
                const instanceIndex = row.Instances.findIndex(i => i.Id === state.Id);
                if (instanceIndex >= 0) {
                    if (state.Status === scenarioState_1.ScenarioStatus.Stopped) {
                        row.Instances.splice(instanceIndex, 1);
                    }
                    else {
                        row.Instances[instanceIndex] = state;
                    }
                }
                else {
                    row.Instances.push(state);
                }
                //this.rows.set(state.Id, row);
                updatedStates.push(state);
            }
        });
        if (updatedStates.length > 0) {
            this.statesChanged.next(updatedStates);
        }
    }
}
exports.ScenarioManager = ScenarioManager;
