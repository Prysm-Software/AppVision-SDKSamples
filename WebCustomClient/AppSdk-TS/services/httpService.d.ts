export declare class HttpService {
    private headers;
    private _host;
    constructor(host: string);
    get host(): string;
    addGlobalHeaders(headerKey: string, headerValue: string): void;
    get(url: string, headers?: HeadersInit, throwIfError?: boolean, timeout?: number): Promise<Response>;
    post(url: string, body: any, method?: string, headers?: HeadersInit, throwIfError?: boolean, timeout?: number): Promise<Response>;
}
