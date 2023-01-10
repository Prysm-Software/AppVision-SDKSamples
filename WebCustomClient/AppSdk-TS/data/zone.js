"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ZoneReaderOut = exports.ZoneReaderIn = exports.Zone = void 0;
class Zone {
    constructor(obj) {
        var _a, _b;
        Object.assign(this, obj);
        if (((_a = obj.ZoneReaderIns) === null || _a === void 0 ? void 0 : _a.length) > 0)
            this.ZoneReaderIns = obj.ZoneReaderIns.map(zi => new ZoneReaderIn(zi));
        if (((_b = obj.ZoneReaderOuts) === null || _b === void 0 ? void 0 : _b.length) > 0)
            this.ZoneReaderOuts = obj.ZoneReaderOuts.map(zi => new ZoneReaderOut(zi));
    }
}
exports.Zone = Zone;
class ZoneReaderIn {
    constructor(obj) {
        Object.assign(this, obj);
    }
}
exports.ZoneReaderIn = ZoneReaderIn;
class ZoneReaderOut {
    constructor(obj) {
        Object.assign(this, obj);
    }
}
exports.ZoneReaderOut = ZoneReaderOut;
