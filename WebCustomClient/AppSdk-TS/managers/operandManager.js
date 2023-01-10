"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.OperandManager = void 0;
class OperandManager {
    constructor(variableManager, areaManager, alarmManager, groupManager, userManager, protocolManager) {
        // pour utiliser la syntaxe $X.prop en lecture/écriture
        this.op = new Proxy(this, {
            get(target, property) { return target.read(property); },
            set(target, property, value) { target.write(property, value); return true; }
        });
        this.variableManager = variableManager;
        this.alarmManager = alarmManager;
        this.areaManager = areaManager;
        this.groupManager = groupManager;
        this.userManager = userManager;
        this.protocolManager = protocolManager;
    }
    write(operand, value) {
        if (!operand.startsWith('$V.'))
            throw new Error('Invalid operand');
        //todo on ne prend pas (encore) en compte l'affectation binaire .bn
        this.variableManager.set(operand.substring(3), value.toString());
    }
    read(operand) {
        if (operand.startsWith('$V.'))
            return this.readVariable(operand.substring(3));
        else if (operand.startsWith('$A.'))
            return this.readArea(operand.substring(3));
        else if (operand.startsWith('$G.'))
            return this.readArea(operand.substring(3));
        else if (operand.startsWith('$U#'))
            return this.readUser(operand.substring(3));
        return operand;
    }
    readVariable(operand) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j;
        let prop;
        let bit = -1;
        // $V.var.b0
        const m = operand.match(/\.b[0-9]+$/);
        if (m) {
            operand = operand.slice(0, m.index);
            bit = parseInt(m[0].substring(2));
        }
        // $V.var#Prop
        const op = operand.split('#');
        if (op.length > 1) {
            operand = op[0];
            prop = op[1];
        }
        const row = this.variableManager.getRowByName(operand);
        if (!row)
            return null;
        switch (prop) {
            case 'Name':
                return row.Name;
            case 'Desc':
                return row.FullDescription;
            case 'WaitAck':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return false;
                    return alarm.WaitAcknowledge;
                }
            case 'Alarm':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return false;
                    return true;
                }
            case 'AreaDesc':
                {
                    const area = this.areaManager.getAreaById(row.AreaId);
                    if (!area)
                        return "";
                    return area.Description;
                }
            case 'AreaName':
                {
                    const area = this.areaManager.getAreaById(row.AreaId);
                    if (!area)
                        return "";
                    return area.Name;
                }
            case 'Date':
                return (_a = row.State) === null || _a === void 0 ? void 0 : _a.Date;
            case 'Id':
                return row.Id;
            case 'Info':
                return (_b = row.State) === null || _b === void 0 ? void 0 : _b.Info;
            case 'Instructions':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return null;
                    return alarm.Instructions;
                }
            case 'IsAlarmMasked':
                return (_c = row.State) === null || _c === void 0 ? void 0 : _c.IsAlarmMasked;
            case 'IsLocked':
                return (_d = row.State) === null || _d === void 0 ? void 0 : _d.IsLocked;
            case 'MaxSeverity':
                {
                    //#TODO
                    //THIS IS TEMPORARY
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return undefined;
                    return alarm.Severity;
                }
            case 'PreviousDate':
                return (_e = row.State) === null || _e === void 0 ? void 0 : _e.PreviousDate;
            case 'PreviousValue':
                return (_f = row.State) === null || _f === void 0 ? void 0 : _f.PreviousValue;
            case 'Quality':
                return (_g = row.State) === null || _g === void 0 ? void 0 : _g.Quality;
            case 'WaitEnd':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return false;
                    return alarm.WaitEnd;
                }
            case 'WaitReport':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return false;
                    return alarm.WaitReport;
                }
            case 'WaitProcess':
                {
                    const alarm = this.alarmManager.getAlarms().find(a => a.SourceId === row.Id);
                    if (!alarm)
                        return false;
                    return alarm.WaitProcess;
                }
            case 'Sound':
                //#TODO Where is row.Sound ?
                //THIS IS TEMPORARY
                return undefined;
            case 'State':
                return (_h = row.State) === null || _h === void 0 ? void 0 : _h.valueDesc;
            case 'Parameters':
                //Not sure about this
                return row.FullParameters;
            case 'SParameters':
                return (_j = row.State) === null || _j === void 0 ? void 0 : _j.Parameters;
            case 'Symbol':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.Symbol property yet
            case 'Synoptic':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.Synoptic property yet
            case 'VideoLink':
                return row.FullVideoLink;
            case 'CustomField1':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.CustomPropertyX property yet
            case 'CustomField2':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.CustomPropertyX property yet
            case 'CustomField3':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.CustomPropertyX property yet
            case 'CustomField4':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.CustomPropertyX property yet
            case 'CustomField5':
                //#TODO
                //THIS IS TEMPORARY;
                return ""; //There is no row.CustomPropertyX property yet
            default: {
                const value = row === null || row === void 0 ? void 0 : row.State.Value;
                // si enum par bit
                if (bit > -1)
                    return (parseInt(value) >> bit) & 1;
                return value;
            }
        }
    }
    readArea(operand) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p, _q;
        let prop;
        // $A.var#Prop
        const op = operand.split('#');
        if (op.length > 1) {
            operand = op[0];
            prop = op[1];
        }
        const area = this.areaManager.getAreaByName(operand);
        if (!area)
            return null;
        switch (prop) {
            case 'Name':
                return area.Name;
            case 'Desc':
                return area.FullDescription;
            case 'WaitAck':
                return (_a = area.State) === null || _a === void 0 ? void 0 : _a.isAlarmAcknowledgeWaiting;
            case 'Alarm':
                return (_b = area.State) === null || _b === void 0 ? void 0 : _b.isAlarm;
            case 'Id':
                return area.Id;
            case 'AlarmProcessing':
                return (_c = area.State) === null || _c === void 0 ? void 0 : _c.isAlarmProcessing;
            case 'AlarmWaiting':
                return (_d = area.State) === null || _d === void 0 ? void 0 : _d.isAlarmProcessWaiting;
            case 'AlarmClosed':
                return (_e = area.State) === null || _e === void 0 ? void 0 : _e.isAlarmClosed;
            case 'AlarmCount':
                return (_f = area.State) === null || _f === void 0 ? void 0 : _f.AlarmCount;
            case 'WaitAckCount':
                return (_g = area.State) === null || _g === void 0 ? void 0 : _g.AlarmAcknowledgeWaitingCount;
            case 'AlarmProcessingCount':
                return (_h = area.State) === null || _h === void 0 ? void 0 : _h.AlarmProcessingCount;
            case 'AlarmWaitingCount':
                return (_j = area.State) === null || _j === void 0 ? void 0 : _j.AlarmProcessWaitingCount;
            case 'AlarmClosedCount':
                return (_k = area.State) === null || _k === void 0 ? void 0 : _k.AlarmClosedCount;
            case 'LockedCount':
                return (_l = area.State) === null || _l === void 0 ? void 0 : _l.LockedCount;
            case 'MaskedCount':
                return (_m = area.State) === null || _m === void 0 ? void 0 : _m.MaskedCount;
            case 'MaxSeverity':
                return (_o = area.State) === null || _o === void 0 ? void 0 : _o.MaxSeverity;
            case 'OrValue':
                return (_p = area.State) === null || _p === void 0 ? void 0 : _p.OrValue;
            case 'AndValue':
                return (_q = area.State) === null || _q === void 0 ? void 0 : _q.AndValue;
            default: {
                return area.State;
            }
        }
    }
    readGroup(operand) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p;
        let prop;
        // $A.var#Prop
        const op = operand.split('#');
        if (op.length > 1) {
            operand = op[0];
            prop = op[1];
        }
        const group = this.groupManager.getGroupByName(operand);
        if (!group)
            return null;
        switch (prop) {
            case 'Name':
                return group.Name;
            case 'Desc':
                return group.FullDescription;
            case 'WaitAck':
                return group.State.isAlarmAcknowledgeWaiting;
            case 'Alarm':
                return (_a = group.State) === null || _a === void 0 ? void 0 : _a.isAlarm;
            case 'Id':
                return group.Id;
            case 'AlarmProcessing':
                return (_b = group.State) === null || _b === void 0 ? void 0 : _b.isAlarmProcessing;
            case 'AlarmWaiting':
                return (_c = group.State) === null || _c === void 0 ? void 0 : _c.isAlarmProcessWaiting;
            case 'AlarmClosed':
                return (_d = group.State) === null || _d === void 0 ? void 0 : _d.isAlarmClosed;
            case 'AlarmCount':
                return (_e = group.State) === null || _e === void 0 ? void 0 : _e.AlarmCount;
            case 'WaitAckCount':
                return (_f = group.State) === null || _f === void 0 ? void 0 : _f.AlarmAcknowledgeWaitingCount;
            case 'AlarmProcessingCount':
                return (_g = group.State) === null || _g === void 0 ? void 0 : _g.AlarmProcessingCount;
            case 'AlarmWaitingCount':
                return (_h = group.State) === null || _h === void 0 ? void 0 : _h.AlarmProcessWaitingCount;
            case 'AlarmClosedCount':
                return (_j = group.State) === null || _j === void 0 ? void 0 : _j.AlarmClosedCount;
            case 'LockedCount':
                return (_k = group.State) === null || _k === void 0 ? void 0 : _k.LockedCount;
            case 'MaskedCount':
                return (_l = group.State) === null || _l === void 0 ? void 0 : _l.MaskedCount;
            case 'MaxSeverity':
                return (_m = group.State) === null || _m === void 0 ? void 0 : _m.MaxSeverity;
            case 'OrValue':
                return (_o = group.State) === null || _o === void 0 ? void 0 : _o.OrValue;
            case 'AndValue':
                return (_p = group.State) === null || _p === void 0 ? void 0 : _p.AndValue;
            default: {
                const value = group.State;
                return value;
            }
        }
    }
    readUser(operand) {
        const user = this.userManager.currentUser;
        if (!user)
            return null;
        if (operand.startsWith('Profile.')) {
            operand = operand.substring(8);
            if (user.profile.hasOwnProperty(operand))
                return user.profile[operand];
            console.warn(`$U#Profile.${operand} is not supported`);
            return null;
        }
        if (user.hasOwnProperty(operand))
            return user[operand];
        switch (operand) {
            case 'Fullname':
                return user.fullName;
            case 'Options':
                return user.options;
            default:
                console.warn(`$U#${operand} is not supported`);
                return null;
        }
    }
}
exports.OperandManager = OperandManager;
