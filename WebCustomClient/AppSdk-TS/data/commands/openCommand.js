"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.OpenCommand = void 0;
const helper_1 = require("../../common/helper");
const environmentModels_1 = require("../environmentModels");
class OpenCommand {
    constructor(args) {
        const parts = args.split(',');
        this._viewName = parts[0];
        for (let i = 1; i < parts.length; i++) {
            if (parts[i].startsWith('windowtitle')) {
                this._title = helper_1.Helper.getParameter('windowtitle', parts[i]);
            }
        }
        this._args = environmentModels_1.Args.fromString(args, ',');
    }
    /**
     * Window title
     */
    get title() { return this._title; }
    /**
     * Document name ('@Alarmslist', '@EventsList' ...)
     */
    get viewName() { return this._viewName; }
    /**
     * Document args
     */
    get args() { return this._args; }
}
exports.OpenCommand = OpenCommand;
