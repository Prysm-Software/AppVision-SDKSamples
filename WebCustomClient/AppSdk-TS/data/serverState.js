"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ServerState = void 0;
class ServerState {
    constructor(obj) {
        Object.assign(this, obj);
    }
    get cpuPercent() {
        return +(this.CpuCounter * 100).toFixed(1);
    }
}
exports.ServerState = ServerState;
