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
exports.HttpService = void 0;
const httpHelper_1 = require("../common/httpHelper");
const httpError_1 = require("../data/httpError");
class HttpService {
    constructor(host) {
        this.headers = { 'Accept': 'application/json', 'Content-Type': 'application/json' };
        this._host = host;
        if (!this._host.endsWith('/'))
            this._host += '/';
    }
    get host() {
        return this._host;
    }
    addGlobalHeaders(headerKey, headerValue) {
        this.headers[headerKey] = headerValue;
    }
    get(url, headers = null, throwIfError = true, timeout = httpHelper_1.DEFAULT_HTTP_TIMEOUT) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield httpHelper_1.HttpHelper.fetchWithTimeout(this._host + url, timeout, {
                headers: Object.assign(Object.assign({}, this.headers), headers),
                cache: 'no-store'
            });
            if (throwIfError && !resp.ok) {
                let body = null;
                try {
                    body = yield resp.text();
                }
                catch (_a) {
                    console.log("cannot json resp");
                }
                throw new httpError_1.HttpError(resp.status, resp.statusText, body);
            }
            return resp;
        });
    }
    post(url, body, method = "POST", headers = null, throwIfError = true, timeout = httpHelper_1.DEFAULT_HTTP_TIMEOUT) {
        return __awaiter(this, void 0, void 0, function* () {
            const localHeaders = Object.assign(Object.assign({}, this.headers), headers);
            const resp = yield httpHelper_1.HttpHelper.fetchWithTimeout(this._host + url, timeout, {
                method: method,
                body: body,
                headers: localHeaders,
                cache: 'no-store'
            });
            if (throwIfError && !resp.ok) {
                let body = null;
                try {
                    body = yield resp.text();
                }
                catch (_a) { }
                throw new httpError_1.HttpError(resp.status, resp.statusText, body);
            }
            return resp;
        });
    }
}
exports.HttpService = HttpService;
