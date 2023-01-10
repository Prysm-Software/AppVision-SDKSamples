export declare const DEFAULT_HTTP_TIMEOUT = 10000;
export declare abstract class HttpHelper {
    static fetchWithTimeout(uri: string, timeout?: number, options?: RequestInit): Promise<Response>;
}
