import { Camera } from "../data/camera";
import { HttpService } from "../services/httpService";
import { VariableManager } from "./variableManager";
export declare class VideoManager {
    private http;
    private variableManager;
    constructor(http: HttpService, variableManager: VariableManager);
    createLiveCamera(cameraName: string): Camera;
    createPlaybackCamera(cameraName: string, dateStart: Date): Camera;
}
