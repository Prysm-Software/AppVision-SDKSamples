import { VariableRow } from "./variableRow";
import { HttpService } from "../services/httpService";
export declare enum CameraMode {
    Live = 0,
    Playback = 1
}
export interface CameraStreamInfo {
    timestamp: string;
    status: string;
}
interface StreamOptions {
    /**
     * StreamType: App : custom appvision stream with frames and infos, HttpMultipart: Standart Http multipart stream with only frames
     */
    streamType: "app" | "http_multipart";
    /**
     * Http or websocket protocol
     */
    protocol?: "http" | "websocket";
    /**
     * Http multipart type
     */
    httpMultipartType?: "mixed" | "x-mixed-replace";
}
export interface CameraCapabilities {
    canPresets: boolean;
    canPatterns: Boolean;
    canContinuousMove: Boolean;
    canPlayback: boolean;
    presets: string[];
    patterns: string[];
}
declare type CapabilitiesDetails = "all" | "minimal";
export declare class Camera {
    private http;
    private streamerHttp;
    private dateStart;
    sessionId: string;
    row: VariableRow;
    mode: CameraMode;
    playbackSpeed: number;
    constructor(http: HttpService, row: VariableRow, start?: Date, speed?: number);
    connect(): Promise<void>;
    /**
     * Uri of the camera streamer
     */
    get streamerUri(): string;
    /**
     * Build stream uri and returns it
     * @param options Options of the uri
     * @returns Uri
     */
    getStreamingUri(options?: StreamOptions): string;
    /**
     * Get current info of the stream (current time, status...)
     * @returns CameraStreamInfo
     */
    getStreamInfo(): Promise<CameraStreamInfo>;
    continuousMove(x: number, y: number): Promise<void>;
    continuousZoom(z: number): Promise<void>;
    getPresets(): Promise<string[]>;
    getPatterns(): Promise<string[]>;
    gotoPreset(preset: string): Promise<void>;
    runPattern(pattern: string): Promise<void>;
    goToHome(): Promise<void>;
    setPlaybackPosition(date: Date): Promise<void>;
    setPlaybackSpeed(speed: number): Promise<void>;
    resume(): Promise<void>;
    pause(): Promise<void>;
    stepBackward(): Promise<void>;
    stepForward(): Promise<void>;
    snapshot(): Promise<Blob>;
    getCapabilities(details?: CapabilitiesDetails): Promise<CameraCapabilities>;
}
export {};
