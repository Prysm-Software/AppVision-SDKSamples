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
exports.LicenseManager = void 0;
class LicenseManager {
    constructor(http) {
        this.infos = new Map(); // libellés dans la langue choisie
        this.http = http;
    }
    /**
     * Get licence field value
     * @param key Licence field name
     * @returns Value as string
     */
    get(key) {
        return this.infos.get(key) || null;
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            this.infos.clear();
            const resp = yield this.http.get('AppVisionService.svc/GetLicenseInfos');
            const json = yield resp.json();
            json.forEach(nv => {
                this.infos.set(nv.Name, nv.Value);
            });
        });
    }
}
exports.LicenseManager = LicenseManager;
