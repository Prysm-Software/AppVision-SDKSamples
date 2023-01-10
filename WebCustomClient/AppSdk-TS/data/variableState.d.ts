import { DateTimeOffset } from "./dateTimeOffset";
import { VariableRow } from "./variableRow";
import { NameValue } from "./nameValue";
export declare class VariableState {
    constructor(varState: Partial<VariableState>);
    Date: DateTimeOffset;
    Id: number;
    Info: string;
    IsAlarmMasked: boolean;
    IsLocked: boolean;
    Name: string;
    Operation: string;
    OriginDesc: string;
    OriginId: number;
    Parameters: NameValue[];
    PreviousDate: DateTimeOffset;
    PreviousValue: string;
    Quality: number;
    Severity: number;
    Value: string;
    ValueInternal: string;
    Row: VariableRow;
    get photo(): string;
    get valueDesc(): string;
    get date(): Date;
    private formatValueDesc;
}
