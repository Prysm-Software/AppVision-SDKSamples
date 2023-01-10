import { Subject } from 'rxjs';
import { Command } from "../data/commands/command";
export declare class CommandManager {
    commandReceived: Subject<Command>;
    constructor();
    updateCommand(command: string): void;
}
