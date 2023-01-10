"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GroupState = void 0;
class GroupState {
    constructor(state) {
        Object.assign(this, state);
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
exports.GroupState = GroupState;
