"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DeviceInfos = exports.DeviceType = void 0;
var DeviceType;
(function (DeviceType) {
    DeviceType[DeviceType["Mobile"] = 0] = "Mobile";
    DeviceType[DeviceType["PC"] = 1] = "PC";
    DeviceType[DeviceType["Other"] = 100] = "Other";
})(DeviceType = exports.DeviceType || (exports.DeviceType = {}));
class DeviceInfos {
    constructor(obj) {
        Object.assign(this, obj);
    }
}
exports.DeviceInfos = DeviceInfos;
