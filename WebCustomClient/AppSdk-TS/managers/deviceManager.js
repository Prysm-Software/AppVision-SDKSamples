"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.DeviceManager = void 0;
const deviceInfos_1 = require("../data/deviceInfos");
class DeviceManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Register a device to the server
     * @param infos Device infos
     * @returns Device created infos
     */
    registerDevice(infos) {
        return __awaiter(this, void 0, void 0, function* () {
            const json = {
                DeviceId: infos.deviceId,
                DeviceName: infos.deviceName,
                Type: infos.type
            };
            const resp = yield this.http.post("AppVisionService.svc/DeviceManager/Register", JSON.stringify(json), "POST");
            const jsonResp = yield resp.json();
            return new deviceInfos_1.DeviceInfos(jsonResp);
        });
    }
}
exports.DeviceManager = DeviceManager;
