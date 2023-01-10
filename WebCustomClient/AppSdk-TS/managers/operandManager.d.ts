import { AlarmManager } from "./alarmManager";
import { AreaManager } from "./areaManager";
import { GroupManager } from "./groupManager";
import { ProtocolManager } from "./protocolManager";
import { UserManager } from "./userManager";
import { VariableManager } from "./variableManager";
export declare class OperandManager {
    private variableManager;
    private alarmManager;
    private areaManager;
    private groupManager;
    private userManager;
    private protocolManager;
    constructor(variableManager: VariableManager, areaManager: AreaManager, alarmManager: AlarmManager, groupManager: GroupManager, userManager: UserManager, protocolManager: ProtocolManager);
    write(operand: string, value: any): void;
    read(operand: string): any;
    op: this;
    readVariable(operand: string): any;
    readArea(operand: string): any;
    readGroup(operand: string): any;
    readUser(operand: string): any;
}
