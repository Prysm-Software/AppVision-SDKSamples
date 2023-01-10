"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CustomColor = void 0;
const stringHelper_1 = require("../common/stringHelper");
const alarmRow_1 = require("./alarmRow");
const areaState_1 = require("./areaState");
const eventRow_1 = require("./eventRow");
const groupState_1 = require("./groupState");
const variableState_1 = require("./variableState");
const alarmsHelper_1 = require("../common/alarmsHelper");
class CustomColor {
    constructor(filter, sbackground, sforeground, alarmManager, areaManager) {
        this.alarmManager = alarmManager;
        this.areaManager = areaManager;
        this.description = "Custom color";
        //var color = ColorHelper. ColorConverter.ConvertFromString(sbackground);
        this.background = sbackground !== null && sbackground !== void 0 ? sbackground : "#FFFFFF";
        //color = ColorConverter.ConvertFromString(sforeground);
        this.foreground = sforeground !== null && sforeground !== void 0 ? sforeground : "#FFFFFF";
        this.filterSeverityMin = 0;
        this.filterSeverityMax = 100;
        this.filterArea = this.filterGroup = "";
        if (!stringHelper_1.StringHelper.isNullOrEmpty(filter)) {
            var ss = filter.split(',');
            ss.forEach((s) => {
                if (s.startsWith("$A."))
                    this.filterArea = s.substring(3);
                else if (s.startsWith("$G."))
                    this.filterGroup = s.substring(3);
                else if (s.startsWith("Type="))
                    this.filterType = s.substring(5);
                else if (s.startsWith("Description="))
                    this.description = s.substring(12);
                else if (s.startsWith("FilterSeverityMin=")) {
                    let n = s.substring(18);
                    if (n && !isNaN(Number(n)))
                        this.filterSeverityMin = Number(n);
                }
                else if (s.startsWith("FilterSeverityMax=")) {
                    let n = s.substring(18);
                    if (n && !isNaN(Number(n)))
                        this.filterSeverityMax = Number(n);
                }
                if (this.filterSeverityMax < this.filterSeverityMin)
                    this.filterSeverityMax = this.filterSeverityMin;
            });
        }
    }
    isMatch(o) {
        if (o instanceof eventRow_1.EventRow) {
            var e = o;
            if (e.Tag instanceof alarmRow_1.AlarmRow) {
                if (this.checkType(e.Tag))
                    o = e.Tag;
            }
            else if (e.Tag instanceof variableState_1.VariableState) {
                if (this.checkType(e.Tag))
                    o = e.Tag;
            }
        }
        else if (o instanceof variableState_1.VariableState) {
            var st = o;
            var al = this.alarmManager.getAlarms(1, (al) => al.SourceName == "$V." + st.Name);
            if (al != null && al.length > 0)
                if (this.checkType(al[0]))
                    o = al[0];
        }
        else if (o instanceof areaState_1.AreaState) {
            var area = o;
            var al = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.alarmManager.getAlarms(0, (al) => al.AreaId == area.Id));
            if (al != null)
                if (this.checkType(al[0]))
                    o = al;
        }
        else if (o instanceof groupState_1.GroupState) {
            var area = o;
            var al = alarmsHelper_1.AlarmsHelper.sortAlarmArray(this.alarmManager.getAlarms(0, (al) => al.GroupNames == area.Name));
            if (al != null)
                if (this.checkType(al[0]))
                    o = al;
        }
        if (!this.checkType(o))
            return false;
        if (o instanceof eventRow_1.EventRow) {
            var e = o;
            if (!this.checkArea(e.AreaId))
                return false;
            if (!this.checkGroup(e.GroupNames))
                return false;
            return true;
        }
        if (o instanceof variableState_1.VariableState)
            if (o.Row != null) {
                var s = o;
                if (!this.checkArea(s.Row.AreaId))
                    return false;
                if (!this.checkGroups(s.Row.FullGroupNames))
                    return false;
                return true;
            }
        if (o instanceof alarmRow_1.AlarmRow) {
            var a = o;
            if (!this.checkSeverity(a.Severity))
                return false;
            if (!this.checkArea(a.AreaId))
                return false;
            if (!this.checkGroup(a.GroupNames))
                return false;
            return true;
        }
        return false;
    }
    checkType(o) {
        if (stringHelper_1.StringHelper.isNullOrEmpty(this.filterType)) // pas de filtre?
            return false;
        if (o instanceof alarmRow_1.AlarmRow) {
            var a = o;
            return this.checkAlarmStatus(a.status, a.WaitEnd);
            //if (a.WaitAcknowledge)
            //	return FilterType.Contains(a.WaitEnd ? "Alarm.WaitAck" : "Alarm.WaitAEnded");
            //return FilterType.Contains(a.WaitEnd ? "Alarm.WaitEnd" : "Alarm.Closed");
        }
        if (o instanceof eventRow_1.EventRow) {
            return this.filterType.includes("Event");
        }
        if (o instanceof variableState_1.VariableState) {
            var v = o.Row;
            if (v != null)
                if (v.IsAlarm) {
                    var vast = this.alarmManager.getAlarms(1, (al) => al.SourceName == v.Name);
                    if (vast != null && vast.length > 0) {
                        return this.checkAlarmStatus(vast[0].status, vast[0].WaitEnd);
                        //if (vast.WaitAcknowledge)
                        //	return FilterType.Contains(vast.WaitEnd ? "Alarm.WaitAck" : "Alarm.WaitAEnded");
                        //return FilterType.Contains(vast.WaitEnd ? "Alarm.WaitEnd" : "Alarm.Closed");
                    }
                }
            return this.filterType.includes("VariableState");
        }
        return false;
    }
    checkAlarmStatus(ast, waitEnd) {
        switch (ast) {
            case alarmRow_1.AlarmStatus.AcknowledgeWaiting: return this.filterType.includes(waitEnd ? "Alarm.WaitAckActive" : "Alarm.WaitAckInactive");
            case alarmRow_1.AlarmStatus.Processing: return this.filterType.includes("Alarm.Processing");
            case alarmRow_1.AlarmStatus.ProcessWaiting: return this.filterType.includes("Alarm.Waiting");
        }
        return this.filterType.includes("Alarm.Closed");
        //return FilterType.Contains(waitEnd ? "Alarm.ActiveClosed": "Alarm.Closed");
    }
    checkSeverity(severity) {
        if ((this.filterSeverityMin == 0) && (this.filterSeverityMax == 0)) // pas de filtre
            return true;
        return (severity >= this.filterSeverityMin) && (severity <= this.filterSeverityMax);
    }
    checkArea(areaId) {
        if (stringHelper_1.StringHelper.isNullOrEmpty(this.filterArea)) // pas de filtre?
            return true;
        if (areaId == null)
            return false;
        var a = this.areaManager.getAreaById(areaId);
        if (a == null)
            return false;
        return (a.Name == this.filterArea) || a.Name.startsWith(this.filterArea + ".");
    }
    checkGroup(groupNames) {
        if (stringHelper_1.StringHelper.isNullOrEmpty(this.filterGroup)) // pas de filtre?
            return true;
        if (stringHelper_1.StringHelper.isNullOrEmpty(groupNames))
            return false;
        var names = groupNames.split(',');
        return this.checkGroups(names);
    }
    checkGroups(groupNames) {
        if (stringHelper_1.StringHelper.isNullOrEmpty(this.filterGroup)) // pas de filtre?
            return true;
        groupNames.forEach((value) => {
            if ((value == this.filterGroup) || value.startsWith(this.filterGroup + "."))
                return true;
        });
        return false;
    }
}
exports.CustomColor = CustomColor;
