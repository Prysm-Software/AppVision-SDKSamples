"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AreaState = void 0;
class AreaState {
    constructor(areaState) {
        Object.assign(this, areaState);
    }
    get isAlarm() {
        return this.AlarmCount > 0;
    }
    get isAlarmAcknowledgeWaiting() {
        return this.AlarmAcknowledgeWaitingCount > 0;
    }
    get isAlarmProcessing() {
        return this.AlarmProcessingCount > 0;
    }
    get isAlarmProcessWaiting() {
        return this.AlarmProcessWaitingCount > 0;
    }
    get isAlarmClosed() {
        return this.AlarmClosedCount > 0;
    }
}
exports.AreaState = AreaState;
