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
exports.HttpHelper = exports.DEFAULT_HTTP_TIMEOUT = void 0;
exports.DEFAULT_HTTP_TIMEOUT = 10000;
class HttpHelper {
    static fetchWithTimeout(uri, timeout = exports.DEFAULT_HTTP_TIMEOUT, options = {}) {
        return __awaiter(this, void 0, void 0, function* () {
            const controller = new AbortController();
            const id = setTimeout(() => controller.abort("Timeout"), timeout);
            const response = yield fetch(uri, Object.assign(Object.assign({}, options), { signal: controller.signal }));
            clearTimeout(id);
            return response;
        });
    }
}
exports.HttpHelper = HttpHelper;
