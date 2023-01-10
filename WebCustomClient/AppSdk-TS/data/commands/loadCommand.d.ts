import { Args } from "../environmentModels";
import { Command } from "./command";
export declare class LoadCommand implements Command {
    private _containerName;
    private _viewName;
    private _args;
    /**
     * Container Name: 'c1', 'c2', 'main'...
     */
    get containerName(): string;
    /**
     * Document name: '@AlarmsList' ...
     */
    get viewName(): string;
    /**
     * Document arguments
     */
    get args(): Args[];
    constructor(args: string);
}
