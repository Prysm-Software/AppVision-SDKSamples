"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ScenarioRow = void 0;
const scenarioState_1 = require("./scenarioState");
class ScenarioRow {
    constructor(state) {
        var _a;
        Object.assign(this, state);
        if (((_a = state.Instances) === null || _a === void 0 ? void 0 : _a.length) > 0) {
            this.Instances = state.Instances.map(i => new scenarioState_1.ScenarioState(i));
        }
        else {
            this.Instances = [];
        }
    }
}
exports.ScenarioRow = ScenarioRow;
