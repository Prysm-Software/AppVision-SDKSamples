import { Args } from "../environmentModels";
import { Command } from "./command";
export declare class OpenCommand implements Command {
    private _viewName;
    private _title;
    private _args;
    /**
     * Window title
     */
    get title(): string;
    /**
     * Document name ('@Alarmslist', '@EventsList' ...)
     */
    get viewName(): string;
    /**
     * Document args
     */
    get args(): Args[];
    constructor(args: string);
}
