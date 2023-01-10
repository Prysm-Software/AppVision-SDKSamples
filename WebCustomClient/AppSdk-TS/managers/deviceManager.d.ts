import { DeviceInfos, DeviceType } from "../data/deviceInfos";
import { HttpService } from "../services/httpService";
interface RegisterDeviceInfos {
    deviceId: string;
    deviceName: string;
    type: DeviceType;
}
export declare class DeviceManager {
    private http;
    constructor(http: HttpService);
    /**
     * Register a device to the server
     * @param infos Device infos
     * @returns Device created infos
     */
    registerDevice(infos: RegisterDeviceInfos): Promise<DeviceInfos>;
}
export {};
