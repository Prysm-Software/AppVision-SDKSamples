"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ScenarioTriggerOrigin = exports.ScenarioState = exports.ScenarioStatus = void 0;
var ScenarioStatus;
(function (ScenarioStatus) {
    ScenarioStatus[ScenarioStatus["Stopped"] = 0] = "Stopped";
    ScenarioStatus[ScenarioStatus["Running"] = 1] = "Running";
})(ScenarioStatus = exports.ScenarioStatus || (exports.ScenarioStatus = {}));
class ScenarioState {
    constructor(state) {
        Object.assign(this, state);
    }
}
exports.ScenarioState = ScenarioState;
class ScenarioTriggerOrigin {
}
exports.ScenarioTriggerOrigin = ScenarioTriggerOrigin;
