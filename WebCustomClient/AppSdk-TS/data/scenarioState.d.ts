import { ScenarioRow } from "./scenarioRow";
export declare enum ScenarioStatus {
    Stopped = 0,
    Running = 1
}
export declare class ScenarioState {
    constructor(state: Partial<ScenarioState>);
    Id: number;
    ScenarioId: number;
    Row: ScenarioRow;
    Trigger: ScenarioTriggerOrigin;
    Status: ScenarioStatus;
}
export declare class ScenarioTriggerOrigin {
    Description: string;
    Origin: object;
}
