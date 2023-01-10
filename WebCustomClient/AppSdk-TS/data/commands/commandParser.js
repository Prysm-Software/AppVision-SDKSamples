"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CommandParser = void 0;
const openCommand_1 = require("./openCommand");
const loadCommand_1 = require("./loadCommand");
class CommandParser {
    static parse(cmd) {
        const parts = cmd.split(';');
        if (parts.length < 2)
            return null;
        const command = parts[0];
        const args = parts[1];
        try {
            switch (command) {
                case '@Open':
                    return new openCommand_1.OpenCommand(args);
                case '@Load':
                    return new loadCommand_1.LoadCommand(args);
                default:
                    return null;
            }
        }
        catch (e) {
            console.error("Cannot parse command : " + e);
        }
    }
}
exports.CommandParser = CommandParser;
