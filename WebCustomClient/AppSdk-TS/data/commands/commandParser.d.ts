import { Command } from "./command";
export declare abstract class CommandParser {
    static parse(cmd: string): Command;
}
