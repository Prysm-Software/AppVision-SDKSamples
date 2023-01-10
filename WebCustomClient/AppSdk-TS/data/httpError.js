"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.HttpError = void 0;
class HttpError extends Error {
    constructor(statusCode, statusText, body) {
        super(statusText);
        this.statusCode = statusCode;
        this.body = body;
        // Set the prototype explicitly.
        Object.setPrototypeOf(this, HttpError.prototype);
    }
}
exports.HttpError = HttpError;
