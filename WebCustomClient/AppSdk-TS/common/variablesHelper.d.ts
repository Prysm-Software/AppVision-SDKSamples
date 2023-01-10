import { VariableRow } from "../data/variableRow";
export interface VariableValue {
    name: string;
    value: string;
}
export interface VariableValuesDescription {
    possibleValues: VariableValue[];
    type: VariableValueType;
}
declare type VariableValueType = "Enum" | "String" | "EnumBit" | "NotSettable";
export declare abstract class VariablesHelper {
    static getValuesDescription(row: VariableRow): VariableValuesDescription;
    private static initListValues;
    private static initBitValues;
}
export {};
