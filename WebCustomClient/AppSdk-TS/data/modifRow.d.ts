import { DateTimeOffset } from "./dateTimeOffset";
export declare enum ModifType {
    None = 0,
    Add = 1,
    Change = 2,
    Delete = 4
}
export declare class ModifRow {
    constructor(modif: Partial<ModifRow>);
    Id: number;
    Info: string;
    Description: string;
    Table: string;
    Source: string;
    UserId: number;
    SourceId: number;
    SourceId2: number;
    SiteId: number;
    SiteModifId: number;
    Extention: string;
    CustomExtension: string;
    Date: DateTimeOffset;
    get date(): Date;
    Type: number;
    get type(): string;
}
