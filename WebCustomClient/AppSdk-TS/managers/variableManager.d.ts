import { Subject } from "rxjs";
import { VariableRow, VariableType } from "../data/variableRow";
import { VariableState } from "../data/variableState";
import { HttpService } from "../services/httpService";
import { ValueRow } from "../data/valueRow";
import { NameValue } from "../data/nameValue";
export declare class VariableManager {
    private http;
    private rowsByName;
    private rowsById;
    private states;
    rowsUpdated: Subject<VariableRow[]>;
    rowsAdded: Subject<VariableRow[]>;
    rowsDeleted: Subject<VariableRow[]>;
    statesChanged: Subject<VariableState[]>;
    constructor(http: HttpService);
    /**
     * Set variable
     * @param varName Variable name
     * @param varValue Variable value
     * @param info Info field
     */
    set(varName: string, varValue: string, info?: string, params?: NameValue[]): Promise<void>;
    /**
     * Lock or unlock variable
     * @param varName VariableName
     * @param isLocked Lock or unlock
     * @param params NameValue parameters
     */
    lock(varName: string, isLocked: boolean, params: NameValue[]): Promise<void>;
    /**
     * Updates a VariableRow
     * @param row VariableRow to be updated.
     * @returns true if the variable has been successfully updated.
     */
    updateVariable(row: VariableRow): Promise<boolean>;
    /**
     * Add new VariableRow
     * @param row VariableRow to add
     * @returns VariableRow created
     */
    add(row: VariableRow): Promise<VariableRow>;
    /**
     * Add an array of rows
     * @param rows
     * @returns true ou false
     */
    adds(rows: VariableRow[]): Promise<boolean>;
    /**
     * Delete a row
     * @param row
     * @returns true ou false
     */
    delete(row: VariableRow): Promise<boolean>;
    /**
     * Get all variable rows
     * @returns
     */
    getRows(): VariableRow[];
    /**
     * Get all variable states
     * @returns
     */
    getStates(): VariableState[];
    /**
     * Get row by id
     * @param id Variable id
     * @returns Variable row
     *
     * @remark
     * Get value from cache
     */
    getRowById(id: number): VariableRow;
    /**
     * Get row by name
     * @param varName Variable name
     * @returns Variable row
     *
     * @remark
     * Get value from cache
     */
    getRowByName(varName: string): VariableRow;
    /**
     * Get variable state by variable name
     * @param varName Variable name
     * @returns Variable state
     *
     * @remark
     * Get value from cache
     */
    getStateByName(varName: string): VariableState;
    /**
     * Get variable state by variable id
     * @param varId Variable id
     * @returns Variable state
     *
     * @remark
     * Get value from cache
     */
    getStateById(varId: number): VariableState;
    /**
     * Get rows by type
     * @param rowType Variable row type
     * @returns Array of variables
     *
     * @remark
     * Get value from cache
     */
    getRowsByType(rowType: VariableType): VariableRow[];
    /**
     * Get variable states history
     * @param dateStart Filter date start
     * @param dateEnd  Filter date end
     * @param variableNames Filter variable names
     * @param maxCount Max count result. Default is 2500
     * @returns Array of variable rows
     */
    getHistoValues(dateStart: Date, dateEnd: Date, variableNames: string[], maxCount?: number): Promise<ValueRow[]>;
    private getVariableState;
}
