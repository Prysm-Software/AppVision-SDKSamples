import { TreeRow } from "./treeRow";
import { VariableState } from "./variableState";
export declare enum VariableType {
    Node = 0,
    Bool = 1,
    Int = 2,
    String = 3,
    Enum = 4,
    Double = 5,
    Timetable = 6,
    Buffer = 7,
    Calendar = 8,
    Reader = 10,
    Video = 11,
    Audio = 12
}
export declare enum VariableBufferType {
    Byte = 0,
    SByte = 1,
    UInt16 = 2,
    Int16 = 3,
    UInt32 = 4,
    Int32 = 5,
    UInt64 = 6,
    Int64 = 7,
    Double = 9,
    Bit = 10
}
export declare class VariableRow extends TreeRow {
    FullAddress: string;
    FullParameters: string;
    FullVideoLink: string;
    Type: number;
    State: VariableState;
    StateDescs: string;
    StateType: number;
    IsExclude: boolean;
    AreaId: number;
    AreaDescription: string;
    GroupNames: string[];
    FullGroupDescriptions: string;
    FullGroupNames: string[];
    FullGroupIds: number[];
    Expression: string;
    CustomField1: string;
    CustomField2: string;
    CustomField3: string;
    CustomField4: string;
    CustomField5: string;
    Sound: string;
    FullSound: string;
    FullSynoptic: string;
    IsAlarm: boolean;
    StoreValue: boolean;
    StoreEvent: boolean;
    StoreTempo: number;
    StoreDeltaMin: number;
    StoreDeltaMax: number;
    Parameters: string;
    Address: string;
    AlarmInstructions: string;
    IsAlarmMultiple: boolean;
    AlarmNeedAcknowledge: boolean;
    AlarmResetOnAcknowledge: boolean;
    AlarmNeedReport: boolean;
    AlarmSeverity: number;
    AlarmSeverity2: number;
    AlarmTempo: number;
    AlarmMinValue: number;
    AlarmMaxValue: number;
    AlarmMinValue2: number;
    AlarmMaxValue2: number;
    Synoptic: string;
    GraphMin: number;
    GraphMax: number;
    GraphScale: number;
    VideoLink: string;
    Symbol: string;
    Icon: string;
    NoVisible: string;
    Script: string;
    Position: string;
    Notes: string;
    BufferLength: string;
    BufferTypeInt: number;
    BufferType: number;
    Extension: string;
    Url: string;
    CustomExtension: string;
    constructor(row: Partial<VariableRow>);
    static getTreeByType(items: VariableRow[], type: number): VariableRow[];
    private hasLeaf;
}
