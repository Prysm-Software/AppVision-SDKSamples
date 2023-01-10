import { DateTimeOffset } from "./dateTimeOffset";
export declare class ValueRow {
    Id: number;
    VariableId: number;
    Value: number;
    SiteId: number;
    SiteValueId: number;
    Date: DateTimeOffset;
    get date(): Date;
    constructor(value: Partial<ValueRow>);
}
