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
exports.OverlayManager = void 0;
const rxjs_1 = require("rxjs");
const overlayItem_1 = require("../data/overlayItem");
class OverlayManager {
    constructor(http) {
        this.http = http;
        this.items = new Map();
        this.itemAdded = new rxjs_1.Subject();
        this.itemDeleted = new rxjs_1.Subject();
        this.itemUpdated = new rxjs_1.Subject();
    }
    /**
     * Get overlay items from cache
     * @returns Overlays
     */
    getAll() {
        return [...this.items.values()];
    }
    /**
     * Add or update overlay
     * @param o Overlay
     * @returns Overlay updated/created
     */
    update(o) {
        return __awaiter(this, void 0, void 0, function* () {
            const ol = new overlayItem_1.OverlayItem(o);
            ol.Date = undefined;
            const resp = yield this.http.post("AppVisionService.svc/UpdateOverlay", JSON.stringify(ol));
            const json = yield resp.json();
            return new overlayItem_1.OverlayItem(json);
        });
    }
    /**
     * Delete an overlay
     * @param id Overlay id to delete
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get("AppVisionService.svc/RemoveOverlay?id=" + id);
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get("AppVisionService.svc/GetOverlays");
            const json = yield resp.json();
            const overlays = json.map(o => new overlayItem_1.OverlayItem(o));
            for (const o of overlays) {
                this.items.set(o.Id, o);
            }
        });
    }
    /** @internal */
    updateInternal(items) {
        const added = [];
        const deleted = [];
        const updated = [];
        for (const o of items) {
            if (o.Synoptic === "@deleted") {
                this.items.delete(o.Id);
                deleted.push(o);
            }
            else {
                if (this.items.has(o.Id)) {
                    updated.push(o);
                }
                else {
                    added.push(o);
                }
                this.items.set(o.Id, o);
            }
        }
        if (added.length > 0)
            this.itemAdded.next(added);
        if (deleted.length > 0)
            this.itemDeleted.next(deleted);
        if (updated.length > 0)
            this.itemUpdated.next(updated);
    }
}
exports.OverlayManager = OverlayManager;
