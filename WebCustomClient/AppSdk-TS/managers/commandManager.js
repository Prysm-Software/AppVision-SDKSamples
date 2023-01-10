"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CommandManager = void 0;
const rxjs_1 = require("rxjs");
const commandParser_1 = require("../data/commands/commandParser");
class CommandManager {
    constructor() {
        this.commandReceived = new rxjs_1.Subject();
    }
    updateCommand(command) {
        const commandStr = command.replace("CommandClient;", "");
        const cmd = commandParser_1.CommandParser.parse(commandStr);
        if (command != null) {
            this.commandReceived.next(cmd);
        }
    }
}
exports.CommandManager = CommandManager;
