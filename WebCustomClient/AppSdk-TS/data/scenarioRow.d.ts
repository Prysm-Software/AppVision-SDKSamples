import { ScenarioState } from "./scenarioState";
export declare class ScenarioRow {
    constructor(state: Partial<ScenarioRow>);
    Id: number;
    Name: string;
    Unique: boolean;
    Activated: boolean;
    Instances: ScenarioState[];
}
