"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AlarmRow = exports.AlarmStatus = void 0;
const dateHelper_1 = require("../common/dateHelper");
var AlarmStatus;
(function (AlarmStatus) {
    AlarmStatus[AlarmStatus["None"] = 0] = "None";
    AlarmStatus[AlarmStatus["AcknowledgeWaiting"] = 10] = "AcknowledgeWaiting";
    AlarmStatus[AlarmStatus["Processing"] = 20] = "Processing";
    AlarmStatus[AlarmStatus["ProcessWaiting"] = 30] = "ProcessWaiting";
    AlarmStatus[AlarmStatus["Closed"] = 100] = "Closed";
})(AlarmStatus = exports.AlarmStatus || (exports.AlarmStatus = {}));
class AlarmRow {
    constructor(event) {
        Object.assign(this, event);
    }
    get dateAck() {
        return dateHelper_1.DateHelper.toJSDate(this.DateAck);
    }
    get dateStart() {
        return dateHelper_1.DateHelper.toJSDate(this.DateStart);
    }
    get dateReceive() {
        return dateHelper_1.DateHelper.toJSDate(this.DateReceive);
    }
    get dateReport() {
        return dateHelper_1.DateHelper.toJSDate(this.DateReport);
    }
    get dateEnd() {
        return dateHelper_1.DateHelper.toJSDate(this.DateEnd);
    }
    get status() {
        if (this.WaitAcknowledge)
            return AlarmStatus.AcknowledgeWaiting;
        if (this.WaitProcess)
            return AlarmStatus.ProcessWaiting;
        if (this.WaitReport)
            return AlarmStatus.Processing;
        return AlarmStatus.Closed;
    }
    get groups() {
        return this.GroupNames !== '' && this.GroupNames ? this.GroupNames.split(',') : [];
    }
    /**
     * Return attachment as an array
     * No need to use it as a setter, this returns a cloned array. Use addAttachment
     */
    get attachments() {
        return this.Attachments !== '' && this.Attachments ? this.Attachments.split('\n') : [];
    }
    /**
     * Add a file path to the attachments
     * @param filePath Filepath without the Config/
     */
    addAttachment(filePath) {
        const attachments = this.attachments;
        attachments.push(filePath);
        this.Attachments = attachments.join("\n");
    }
}
exports.AlarmRow = AlarmRow;
