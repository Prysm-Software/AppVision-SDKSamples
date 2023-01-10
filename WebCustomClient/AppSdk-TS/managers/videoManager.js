"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VideoManager = void 0;
const camera_1 = require("../data/camera");
class VideoManager {
    constructor(http, variableManager) {
        this.http = http;
        this.variableManager = variableManager;
    }
    createLiveCamera(cameraName) {
        if (!cameraName.startsWith('$V.'))
            cameraName = '$V.' + cameraName;
        const cameraRow = this.variableManager.getRows().find(i => '$V.' + i.Name === cameraName);
        if (!cameraRow)
            return null;
        return new camera_1.Camera(this.http, cameraRow);
    }
    createPlaybackCamera(cameraName, dateStart) {
        if (!cameraName.startsWith('$V.'))
            cameraName = '$V.' + cameraName;
        const cameraRow = this.variableManager.getRows().find(i => '$V.' + i.Name === cameraName);
        if (!cameraRow)
            return null;
        return new camera_1.Camera(this.http, cameraRow, dateStart);
    }
}
exports.VideoManager = VideoManager;
