export declare class EnvironmentModel {
    isSideBarVisible: boolean;
    isHeaderVisible: boolean;
    layout: LayoutModel;
}
export declare class LayoutModel {
    containers?: ContainerModel[];
    mode: string;
    properties?: Map<string, LayoutPanelProperties>;
    constructor(mode?: string, containers?: ContainerModel[]);
    static createDefault(documents: string[]): LayoutModel;
}
export declare class LayoutPanelProperties {
    resizable: boolean;
    size: string;
}
export declare class ContainerModel {
    name: string;
    document?: DocumentModel;
    isSelectable: boolean;
    hasMenu: boolean;
    allowDragAndDrop: boolean;
    constructor(containerName: string, document?: DocumentModel);
}
export declare class DocumentModel {
    name: string;
    args?: Args[];
}
export declare class Args {
    name: string;
    value: string;
    static fromString(args: string, separator?: string): Args[];
    static getValue(args: Args[], name: string): string;
}
