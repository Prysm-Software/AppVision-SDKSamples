export declare class TreeRow {
    Id: number;
    Name: string;
    Description: string;
    FullDescription: string;
    parent: TreeRow;
    items: TreeRow[];
    constructor(row: Partial<TreeRow>);
    isChild(id: number): boolean;
    static init<T extends TreeRow>(rows: T[], separator?: string): T[];
    static deepCloneArray<T extends TreeRow>(rows: T[], ctor: (new (c: T) => T)): T[];
    static insertOrUpdateItemInArray(rows: TreeRow[], newRow: TreeRow, currentLevel?: number): boolean;
    static getItemById<T extends TreeRow>(rows: T[], id: number): T;
    static getItemByName<T extends TreeRow>(rows: T[], name: string): T;
}
