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
exports.ZoneManager = void 0;
const helper_1 = require("../common/helper");
const zone_1 = require("../data/zone");
class ZoneManager {
    constructor(http) {
        this.http = http;
    }
    /**
     * Get all zones from server
     * @returns Array of zone
     */
    getAll() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Zones?$expand=ZoneReaderIns,ZoneReaderOuts');
            if (resp.status === 204)
                return [];
            const json = yield resp.json();
            return json.value.map(z => new zone_1.Zone(z));
        });
    }
    /**
     * Count zones
     * @returns Zones number
     */
    count() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppControlDataService.svc/Zones/$count', { 'Accept': '*/*' });
            const result = yield resp.text();
            return parseInt(result);
        });
    }
    /**
     * Add new zone
     * @param zone Zone object
     * @returns Zone created
     */
    add(zone) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(zone);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Zone";
            const resp = yield this.http.post('AppControlDataService.svc/Zones', JSON.stringify(odataRow));
            const json = yield resp.json();
            const created = new zone_1.Zone(json);
            created.ZoneReaderIns = [];
            created.ZoneReaderOuts = [];
            if (((_a = zone.ZoneReaderIns) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                for (const r of zone.ZoneReaderIns) {
                    yield this.http.post(`AppControlDataService.svc/Zones(${created.Id})/ZoneReaderIns`, JSON.stringify({
                        ZoneId: created.Id,
                        ReaderId: r.ReaderId,
                        'odata.type': "Prysm.AppControl.Data.ZoneReaderIn"
                    }));
                    created.ZoneReaderIns.push(new zone_1.ZoneReaderIn({ ZoneId: created.Id, ReaderId: r.ReaderId }));
                }
            }
            if (((_b = zone.ZoneReaderOuts) === null || _b === void 0 ? void 0 : _b.length) > 0) {
                for (const r of zone.ZoneReaderOuts) {
                    yield this.http.post(`AppControlDataService.svc/Zones(${created.Id})/ZoneReaderOuts`, JSON.stringify({
                        ZoneId: created.Id,
                        ReaderId: r.ReaderId,
                        'odata.type': "Prysm.AppControl.Data.ZoneReaderOut"
                    }));
                    created.ZoneReaderOuts.push(new zone_1.ZoneReaderOut({ ZoneId: created.Id, ReaderId: r.ReaderId }));
                }
            }
            return created;
        });
    }
    /**
     * Delete zone by id
     * @param id Id of the zone
     */
    delete(id) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post('AppControlDataService.svc/Zones(' + id + ')', null, 'DELETE');
        });
    }
    /**
     * Update zone
     * @param zone Zone object
     * @param updateReaders True to update the readers props of the zone
     */
    update(zone, updateReaders) {
        var _a, _b, _c, _d;
        return __awaiter(this, void 0, void 0, function* () {
            const odataRow = helper_1.Helper.lightCloneObject(zone);
            odataRow["odata.type"] = "Prysm.AppControl.Data.Zone";
            if (updateReaders) {
                const resp = yield this.http.get('AppControlDataService.svc/Zones(' + zone.Id + ')?$expand=ZoneReaderIns,ZoneReaderOuts');
                if (resp.status !== 204) {
                    const json = yield resp.json();
                    const oldZone = new zone_1.Zone(json);
                    if (((_a = oldZone.ZoneReaderIns) === null || _a === void 0 ? void 0 : _a.length) > 0) {
                        for (const r of oldZone.ZoneReaderIns) {
                            yield this.http.post(`AppControlDataService.svc/ZoneReaderIns(ReaderId=${r.ReaderId},ZoneId=${r.ZoneId})`, null, 'DELETE');
                        }
                    }
                    if (((_b = oldZone.ZoneReaderOuts) === null || _b === void 0 ? void 0 : _b.length) > 0) {
                        for (const r of oldZone.ZoneReaderOuts) {
                            yield this.http.post(`AppControlDataService.svc/ZoneReaderOuts(ReaderId=${r.ReaderId},ZoneId=${r.ZoneId})`, null, 'DELETE');
                        }
                    }
                }
                if (((_c = zone.ZoneReaderIns) === null || _c === void 0 ? void 0 : _c.length) > 0) {
                    for (const r of zone.ZoneReaderIns) {
                        const d = helper_1.Helper.lightCloneObject(r);
                        d["odata.type"] = "Prysm.AppControl.Data.ZoneReaderIn";
                        yield this.http.post(`AppControlDataService.svc/Zones(${zone.Id})/ZoneReaderIns`, JSON.stringify(r));
                    }
                }
                if (((_d = zone.ZoneReaderOuts) === null || _d === void 0 ? void 0 : _d.length) > 0) {
                    for (const r of zone.ZoneReaderOuts) {
                        const d = helper_1.Helper.lightCloneObject(r);
                        d["odata.type"] = "Prysm.AppControl.Data.ZoneReaderOut";
                        yield this.http.post(`AppControlDataService.svc/Zones(${zone.Id})/ZoneReaderOuts`, JSON.stringify(r));
                    }
                }
            }
            yield this.http.post('AppControlDataService.svc/Zones(' + zone.Id + ')', JSON.stringify(odataRow), 'MERGE');
        });
    }
}
exports.ZoneManager = ZoneManager;
