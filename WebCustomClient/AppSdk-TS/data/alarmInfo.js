"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AlarmInfo = exports.AlarmEvent = void 0;
const alarmRow_1 = require("./alarmRow");
var AlarmEvent;
(function (AlarmEvent) {
    AlarmEvent[AlarmEvent["Start"] = 0] = "Start";
    AlarmEvent[AlarmEvent["End"] = 1] = "End";
    AlarmEvent[AlarmEvent["Update"] = 2] = "Update";
    AlarmEvent[AlarmEvent["Acknowledge"] = 3] = "Acknowledge";
    AlarmEvent[AlarmEvent["Report"] = 4] = "Report";
    AlarmEvent[AlarmEvent["Resume"] = 5] = "Resume";
    AlarmEvent[AlarmEvent["Delete"] = 6] = "Delete";
    AlarmEvent[AlarmEvent["Transfer"] = 7] = "Transfer";
    AlarmEvent[AlarmEvent["None"] = 8] = "None";
})(AlarmEvent = exports.AlarmEvent || (exports.AlarmEvent = {}));
class AlarmInfo {
    constructor(alarmInfo) {
        Object.assign(this, alarmInfo);
        this.Alarm = new alarmRow_1.AlarmRow(alarmInfo.Alarm);
    }
}
exports.AlarmInfo = AlarmInfo;
