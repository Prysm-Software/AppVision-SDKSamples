"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.LoadCommand = void 0;
const environmentModels_1 = require("../environmentModels");
class LoadCommand {
    constructor(args) {
        const parts = args.split(',');
        const windowNb = parseInt(parts[0]);
        if (windowNb !== 0 && windowNb !== 1)
            throw Error('Invalid argument windowNb = ' + windowNb);
        this._containerName = parts[1];
        this._viewName = parts[2];
        this._args = environmentModels_1.Args.fromString(parts.slice(3).join(','), ',');
    }
    /**
     * Container Name: 'c1', 'c2', 'main'...
     */
    get containerName() { return this._containerName; }
    /**
     * Document name: '@AlarmsList' ...
     */
    get viewName() { return this._viewName; }
    /**
     * Document arguments
     */
    get args() { return [...this._args]; }
}
exports.LoadCommand = LoadCommand;
