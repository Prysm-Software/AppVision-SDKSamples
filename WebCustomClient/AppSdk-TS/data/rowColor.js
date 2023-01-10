"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.RowColor = void 0;
const colorHelper_1 = require("../common/colorHelper");
const alarmInfo_1 = require("./alarmInfo");
const alarmRow_1 = require("./alarmRow");
const areaState_1 = require("./areaState");
const eventRow_1 = require("./eventRow");
const groupState_1 = require("./groupState");
const variableRow_1 = require("./variableRow");
const variableState_1 = require("./variableState");
const stringHelper_1 = require("../common/stringHelper");
const alarmsHelper_1 = require("../common/alarmsHelper");
/**
 * Helper class for getting foreground and background color of an Alarm, Event, State ...
 */
class RowColor {
    constructor(server) {
        this.server = server;
    }
    /**
     * Get the background color of an object
     * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
     * @returns Css color
     */
    getBackgroundColor(obj) {
        var cc = this.server.customColorManager.getCustomColor(obj);
        if (cc && !stringHelper_1.StringHelper.isNullOrEmpty(cc.background)) {
            return cc.background;
        }
        let alarm;
        if (obj instanceof alarmRow_1.AlarmRow) {
            alarm = obj;
        }
        let variable;
        if (obj instanceof variableState_1.VariableState) {
            variable = obj;
        }
        let evt;
        if (obj instanceof eventRow_1.EventRow) {
            evt = obj;
        }
        if ((evt === null || evt === void 0 ? void 0 : evt.TypeInt) === eventRow_1.EventType.Variable && (evt === null || evt === void 0 ? void 0 : evt.SourceId)) {
            const v = this.server.variableManager.getRowById(evt.SourceId);
            if (v) {
                variable = this.server.variableManager.getStateByName(v.Name);
            }
        }
        if (variable) {
            alarm = this.server.alarmManager.getAlarms().filter(a => a.SourceId === variable.Id).pop();
        }
        if (obj instanceof areaState_1.AreaState) {
            const area = obj;
            const areaRow = this.server.areaManager.getAreaByName(area.Name);
            alarm = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.server.alarmManager.getAlarmsByArea(areaRow))[0];
        }
        if (obj instanceof groupState_1.GroupState) {
            const group = obj;
            alarm = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.server.alarmManager.getAlarmsByGroupName(group.Name))[0];
        }
        if (alarm) {
            switch (alarm.status) {
                case alarmRow_1.AlarmStatus.AcknowledgeWaiting: return this.getColor(alarm.WaitEnd ? "App.Color.Alarm.WaitAck.Background" : "App.Color.Alarm.WaitAckEnded.Background");
                case alarmRow_1.AlarmStatus.Processing: return this.getColor("App.Color.Alarm.Processing.Background");
                case alarmRow_1.AlarmStatus.ProcessWaiting: return this.getColor("App.Color.Alarm.Waiting.Background");
            }
            return this.getColor("App.Color.Alarm.Background");
        }
        if (evt) {
            if (evt.TypeInt === eventRow_1.EventType.User) {
                if (evt.SubTypeInt === eventRow_1.EventSubType.UserMessageAlarmCreate)
                    return this.getColor("App.Color.Alarm.WaitAck.Background");
                return this.getColor("App.Color.Event.User.Background");
            }
            if (evt.TypeInt === eventRow_1.EventType.Server)
                return this.getColor("App.Color.Event.Server.Background");
            if (evt.TypeInt === eventRow_1.EventType.Protocol) {
                if (evt.SubTypeInt === eventRow_1.EventSubType.ProtocolStartByUser || evt.SubTypeInt === eventRow_1.EventSubType.ProtocolStopByUser)
                    return this.getColor("App.Color.Event.User.Background");
                return this.getColor("App.Color.Event.Protocol.Background");
            }
            if (evt.TypeInt === eventRow_1.EventType.VariableReaderGranted)
                return this.getColor("App.Color.Event.Reader.Granted.Background");
            if (evt.TypeInt === eventRow_1.EventType.VariableReaderDenied)
                return this.getColor("App.Color.Event.Reader.Denied.Background");
            if (evt.TypeInt === eventRow_1.EventType.AreaOrGroupChanged)
                return this.getColor("App.Color.State.Groups.Background");
            if (evt.TypeInt === eventRow_1.EventType.AlarmChanged) {
                if (evt.SubTypeInt === alarmInfo_1.AlarmEvent.Start || evt.SubTypeInt === alarmInfo_1.AlarmEvent.Update)
                    return this.getColor("App.Color.Alarm.WaitAck.Background");
            }
            if (evt.TypeInt === eventRow_1.EventType.Variable) {
                if (evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateAlarm || evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateAlarmByUser)
                    return this.getColor("App.Color.Alarm.Background");
                if (evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateByUser)
                    return this.getColor("App.Color.Event.User.Background");
            }
        }
        if (variable && variable.Row) {
            if (variable.OriginId > 0)
                return this.getColor("App.Color.Event.User.Background");
            switch (variable.Row.Type) {
                case variableRow_1.VariableType.Bool:
                case variableRow_1.VariableType.Enum:
                case variableRow_1.VariableType.Timetable:
                    return this.getColor("App.Color.State.Enum.Background");
                case variableRow_1.VariableType.String:
                    return this.getColor("App.Color.State.Text.Background");
                case variableRow_1.VariableType.Int:
                case variableRow_1.VariableType.Double:
                    return this.getColor("App.Color.State.Numeric.Background");
            }
        }
        return this.getColor("App.Color.Default.Background");
    }
    /**
    * Get the text color of an object
    * @param obj AlarmRow, AreaState, GroupState, VariableState or EventRow
    * @returns Css color
    */
    getForegroundColor(obj) {
        var cc = this.server.customColorManager.getCustomColor(obj);
        if (cc && !stringHelper_1.StringHelper.isNullOrEmpty(cc.foreground)) {
            return cc.foreground;
        }
        let alarm;
        if (obj instanceof alarmRow_1.AlarmRow)
            alarm = obj;
        let variable;
        if (obj instanceof variableState_1.VariableState)
            variable = obj;
        let evt;
        if (obj instanceof eventRow_1.EventRow)
            evt = obj;
        if ((evt === null || evt === void 0 ? void 0 : evt.TypeInt) === eventRow_1.EventType.Variable && (evt === null || evt === void 0 ? void 0 : evt.SourceId)) {
            const v = this.server.variableManager.getRowById(evt.SourceId);
            if (v) {
                variable = this.server.variableManager.getStateByName(v.Name);
            }
        }
        if (variable) {
            alarm = this.server.alarmManager.getAlarms().filter(a => a.SourceId === variable.Id).pop();
        }
        if (obj instanceof areaState_1.AreaState) {
            const area = obj;
            const a = this.server.areaManager.getAreaByName(area.Name);
            alarm = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.server.alarmManager.getAlarmsByArea(a))[0];
        }
        if (obj instanceof groupState_1.GroupState === true) {
            const group = obj;
            alarm = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.server.alarmManager.getAlarmsByGroupName(group.Name))[0];
        }
        if (alarm) {
            switch (alarm.status) {
                case alarmRow_1.AlarmStatus.AcknowledgeWaiting: return this.getColor(alarm.WaitEnd ? "App.Color.Alarm.WaitAck.Foreground" : "App.Color.Alarm.WaitAckEnded.Foreground");
                case alarmRow_1.AlarmStatus.Processing: return this.getColor("App.Color.Alarm.Processing.Foreground");
                case alarmRow_1.AlarmStatus.ProcessWaiting: return this.getColor("App.Color.Alarm.Waiting.Foreground");
            }
            return this.getColor("App.Color.Alarm.Foreground");
        }
        if (evt) {
            switch (evt.TypeInt) {
                case eventRow_1.EventType.User:
                    return this.getColor("App.Color.Event.User.Foreground");
                case eventRow_1.EventType.Server:
                    return this.getColor("App.Color.Event.Server.Foreground");
                case eventRow_1.EventType.Protocol:
                    if (evt.SubTypeInt === eventRow_1.EventSubType.ProtocolStartByUser || evt.SubTypeInt === eventRow_1.EventSubType.ProtocolStopByUser)
                        return this.getColor("App.Color.Event.User.Foreground");
                    return this.getColor("App.Color.Event.Protocol.Foreground");
                case eventRow_1.EventType.VariableReaderGranted:
                    return this.getColor("App.Color.Event.Reader.Granted.Foreground");
                case eventRow_1.EventType.VariableReaderDenied:
                    return this.getColor("App.Color.Event.Reader.Denied.Foreground");
                case eventRow_1.EventType.AreaOrGroupChanged:
                    return this.getColor("App.Color.State.Groups.Foreground");
                case eventRow_1.EventType.AlarmChanged:
                    if (evt.SubTypeInt === alarmInfo_1.AlarmEvent.Start || evt.SubTypeInt === alarmInfo_1.AlarmEvent.Update)
                        return this.getColor("App.Color.Alarm.WaitAck.Foreground");
                    break;
                case eventRow_1.EventType.Variable:
                    if (evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateAlarm || evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateAlarmByUser)
                        return this.getColor("App.Color.Alarm.Foreground");
                    if (evt.SubTypeInt === eventRow_1.EventSubType.VarUpdateByUser)
                        return this.getColor("App.Color.Event.User.Foreground");
                    break;
            }
        }
        if (variable && variable.Row) {
            if (variable.OriginId > 0)
                return this.getColor("App.Color.Event.User.Foreground");
            switch (variable.Row.Type) {
                case variableRow_1.VariableType.Bool:
                case variableRow_1.VariableType.Enum:
                case variableRow_1.VariableType.Timetable:
                    return this.getColor("App.Color.State.Enum.Foreground");
                case variableRow_1.VariableType.String:
                    return this.getColor("App.Color.State.Text.Foreground");
                case variableRow_1.VariableType.Int:
                case variableRow_1.VariableType.Double:
                    return this.getColor("App.Color.State.Numeric.Foreground");
            }
        }
        return this.getColor("App.Color.Default.Foreground");
    }
    getColor(key) {
        return colorHelper_1.ColorHelper.xamlToCssColor(this.server.optionManager.get(key));
    }
}
exports.RowColor = RowColor;
