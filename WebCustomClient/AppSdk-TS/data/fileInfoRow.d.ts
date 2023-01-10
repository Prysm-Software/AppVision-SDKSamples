import { TreeRow } from "./treeRow";
export declare enum FileInfoType {
    Undefined = 0,
    Cs = 1,
    Reference = 2,
    Synoptic = 3,
    Symbol = 4,
    Folder = 5,
    Header = 6,
    Footer = 7,
    Pcs = 8,
    Layout = 9,
    Workflow = 10,
    Js = 11
}
export declare class FileInfoRow extends TreeRow {
    TypeInt: number;
    ContextInt: number;
    get type(): string;
}
