import { DateTimeOffset } from "./dateTimeOffset";
export declare class OverlayItem {
    Date: DateTimeOffset;
    Id: string;
    Layers: string;
    Parameters: string;
    PositionString: string;
    Recipient: string;
    SizeString: string;
    Symbol: string;
    SymbolParameters: string;
    SymbolProperties: string;
    Synoptic: string;
    ZIndex: number;
    get date(): Date;
    get position(): number[];
    get size(): number[];
    constructor(obj: Partial<OverlayItem>);
}
