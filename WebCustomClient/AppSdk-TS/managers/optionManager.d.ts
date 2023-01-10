import { Subject } from "rxjs";
import { OptionRow } from "../data/optionRow";
import { HttpService } from "../services/httpService";
export declare class OptionManager {
    private http;
    private options;
    rowsUpdated: Subject<OptionRow[]>;
    constructor(http: HttpService);
    /**
     * Get option value
     * @param optionName Option name
     * @returns Option value as string
     */
    get(optionName: string): string;
    /**
     * Set option value
     * @param optionName Option name
     * @param value Option value
     */
    set(optionName: string, value: string): Promise<void>;
    private loadDefaultOptions;
}
