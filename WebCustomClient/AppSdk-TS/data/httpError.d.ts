export declare class HttpError extends Error {
    statusCode: number;
    body: string;
    constructor(statusCode: number, statusText: string, body: string);
}
