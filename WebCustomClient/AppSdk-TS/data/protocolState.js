"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ProtocolState = exports.ProtocolStatus = void 0;
const dateHelper_1 = require("../common/dateHelper");
var ProtocolStatus;
(function (ProtocolStatus) {
    ProtocolStatus[ProtocolStatus["Started"] = 0] = "Started";
    ProtocolStatus[ProtocolStatus["Synchronized"] = 1] = "Synchronized";
    ProtocolStatus[ProtocolStatus["Downloading"] = 2] = "Downloading";
    ProtocolStatus[ProtocolStatus["Stopped"] = 3] = "Stopped";
    ProtocolStatus[ProtocolStatus["Fault"] = 4] = "Fault";
})(ProtocolStatus = exports.ProtocolStatus || (exports.ProtocolStatus = {}));
class ProtocolState {
    constructor(state) {
        Object.assign(this, state);
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
    get dateStart() {
        return dateHelper_1.DateHelper.toJSDate(this.DateStart);
    }
    get status() {
        return ProtocolStatus[this.Status];
    }
}
exports.ProtocolState = ProtocolState;
