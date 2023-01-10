import { TreeRow } from "./treeRow";
import { GroupState } from "./groupState";
export declare class GroupRow extends TreeRow {
    State: GroupState;
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
    ColorInfo: string;
    FullSynoptic: string;
    FullParameters: string;
    FullSound: string;
    constructor(row: Partial<GroupRow>);
}
