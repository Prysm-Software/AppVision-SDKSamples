import { TreeRow } from "./treeRow";
import { AreaState } from "./areaState";
export declare class AreaRow extends TreeRow {
    State: AreaState;
    Icon: string;
    Parameters: string;
    Extension: string;
    Url: string;
    CustomExtension: string;
    CustomField1: string;
    CustomField2: string;
    CustomField3: string;
    CustomField4: string;
    CustomField5: string;
    Synoptic: string;
    Sound: string;
    SiteInfo: string;
    ColorInfo: string;
    FullSynoptic: string;
    FullParameters: string;
    FullSound: string;
    constructor(row: Partial<AreaRow>);
}
