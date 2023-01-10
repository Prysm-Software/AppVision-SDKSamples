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
exports.Camera = exports.CameraMode = void 0;
const dateHelper_1 = require("../common/dateHelper");
const httpService_1 = require("../services/httpService");
const helper_1 = require("../common/helper");
var CameraMode;
(function (CameraMode) {
    CameraMode[CameraMode["Live"] = 0] = "Live";
    CameraMode[CameraMode["Playback"] = 1] = "Playback";
})(CameraMode = exports.CameraMode || (exports.CameraMode = {}));
class Camera {
    constructor(http, row, start = null, speed = 1) {
        this.http = http;
        this.sessionId = helper_1.Helper.guid();
        this.mode = CameraMode.Live;
        this.row = row;
        this.playbackSpeed = speed;
        if (start) {
            this.dateStart = start;
            this.mode = CameraMode.Playback;
        }
    }
    connect() {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = 'videogateway/streaming?video=' + this.row.Name + '&id=' + this.sessionId;
            if (this.dateStart) {
                uri += '&date=' + dateHelper_1.DateHelper.toDateTimeString(this.dateStart);
            }
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            if (json.server.id > 0) {
                // If local gateway streamer
                this.streamerHttp = new httpService_1.HttpService(json.server.uri + json.streamer);
            }
            else {
                // If remote streamer
                this.streamerHttp = new httpService_1.HttpService(this.http.host + json.streamer);
            }
        });
    }
    /**
     * Uri of the camera streamer
     */
    get streamerUri() { return this.streamerHttp.host; }
    /**
     * Build stream uri and returns it
     * @param options Options of the uri
     * @returns Uri
     */
    getStreamingUri(options = null) {
        if (options) {
            let parameters = "";
            const host = (options.protocol === "websocket" && options.streamType === "app") ?
                this.streamerHttp.host.replace("http", 'ws') :
                this.streamerHttp.host;
            if (options.httpMultipartType != null)
                parameters += "&multipart_content_type=" + options.httpMultipartType;
            return host + "video?id=" + this.sessionId + "&stream_type=" + options.streamType + parameters;
        }
        return this.streamerHttp.host + "video?id=" + this.sessionId;
    }
    /**
     * Get current info of the stream (current time, status...)
     * @returns CameraStreamInfo
     */
    getStreamInfo() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.streamerHttp.get('streaminfo/?id=' + this.sessionId + '&rnd=' + new Date().getTime());
            const json = yield resp.json();
            return json;
        });
    }
    continuousMove(x, y) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=continuousmove&x=" + x + "&y=" + y);
        });
    }
    continuousZoom(z) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=continuouszoom&z=" + z);
        });
    }
    getPresets() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=getpresets");
            const json = yield resp.json();
            return json;
        });
    }
    getPatterns() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=getpatterns");
            const json = yield resp.json();
            return json;
        });
    }
    gotoPreset(preset) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=gotopreset&preset=" + preset);
        });
    }
    runPattern(pattern) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=runpattern&pattern=" + pattern);
        });
    }
    goToHome() {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('ptz/?id=' + this.sessionId + "&command=gotohomeposition");
        });
    }
    setPlaybackPosition(date) {
        return __awaiter(this, void 0, void 0, function* () {
            this.dateStart = date;
            yield this.streamerHttp.get('playback/?id=' + this.sessionId + "&command=playbackposition&date=" + dateHelper_1.DateHelper.toDateTimeString(this.dateStart));
        });
    }
    setPlaybackSpeed(speed) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('playback/?id=' + this.sessionId + "&command=playbackspeed&speed=" + speed);
            this.playbackSpeed = speed;
        });
    }
    resume() {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('video/?id=' + this.sessionId + "&command=resume");
        });
    }
    pause() {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('video/?id=' + this.sessionId + "&command=pause");
        });
    }
    stepBackward() {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('playback/?id=' + this.sessionId + "&command=stepBackward");
        });
    }
    stepForward() {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.streamerHttp.get('playback/?id=' + this.sessionId + "&command=stepForward");
        });
    }
    snapshot() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.streamerHttp.get('video/?id=' + this.sessionId + "&command=snapshot");
            const blob = yield resp.blob();
            return blob;
        });
    }
    getCapabilities(details = "all") {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.streamerHttp.get('capabilities?details=' + details);
            const json = yield resp.json();
            return json;
        });
    }
}
exports.Camera = Camera;
