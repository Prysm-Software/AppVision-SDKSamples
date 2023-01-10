"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VariablesHelper = void 0;
const variableRow_1 = require("../data/variableRow");
class VariablesHelper {
    static getValuesDescription(row) {
        let values;
        let type = "NotSettable";
        switch (row.Type) {
            case variableRow_1.VariableType.Bool:
            case variableRow_1.VariableType.Timetable:
                values = this.initListValues(row);
                type = "Enum";
                break;
            case variableRow_1.VariableType.Int:
            case variableRow_1.VariableType.Double:
            case variableRow_1.VariableType.String:
            case variableRow_1.VariableType.Reader:
                type = "String";
                break;
            case variableRow_1.VariableType.Enum:
                if (/b\d+/.test(row.StateDescs)) {
                    type = "EnumBit";
                    values = this.initBitValues(row);
                }
                else {
                    type = "Enum";
                    values = this.initListValues(row);
                }
                break;
            default:
                break;
        }
        return {
            type: type,
            possibleValues: values
        };
    }
    static initListValues(variable) {
        let values = variable.StateDescs
            .split(/\r|\n/)
            .filter(i => i !== "")
            .map(s => {
            if (s.indexOf(':') !== -1) {
                return {
                    name: s.split(':')[1],
                    value: s.split(':')[0]
                };
            }
            else {
                return {
                    name: s,
                    value: s
                };
            }
        });
        if ((values === null || values.length === 0) && variable.Type === 1)
            values = [
                { name: "false", value: "0" },
                { name: "true", value: "1" }
            ];
        return values;
    }
    static initBitValues(variable) {
        const values = [];
        const varValue = parseInt(variable.State.Value);
        variable.StateDescs
            .split(/\r|\n/)
            .filter(i => i !== "" && i.startsWith("b"))
            .forEach(item => {
            const bit = varValue & (1 << 1);
            values.push({ name: item.replace(/b\d+:/, ''), value: String(bit !== 0) });
        });
        return values;
    }
}
exports.VariablesHelper = VariablesHelper;
