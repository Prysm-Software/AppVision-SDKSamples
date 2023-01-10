import { Subject } from "rxjs";
import { ScenarioState } from "../data/scenarioState";
import { ScenarioRow } from "../data/scenarioRow";
import { HttpService } from "../services/httpService";
export declare class ScenarioManager {
    private http;
    private rows;
    statesChanged: Subject<ScenarioState[]>;
    constructor(http: HttpService);
    /**
     * Get all scenarioRow
     */
    get scenarios(): ScenarioRow[];
    /**
     * Start scenario by id
     * @param scenarioId
     */
    startScenarioById(scenarioId: number): Promise<void>;
    /**
     * Stop scenario instance by id
     * @param scenarioId Scenario Id
     * @param instanceId Scenario instance Id
     */
    stopScenarioInstanceById(scenarioId: number, instanceId: number): Promise<void>;
}
