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
exports.ProtocolManager = void 0;
const rxjs_1 = require("rxjs");
const protocolRow_1 = require("../data/protocolRow");
const protocolState_1 = require("../data/protocolState");
class ProtocolManager {
    constructor(http) {
        this.http = http;
        this.statesChanged = new rxjs_1.Subject();
    }
    /**
     * ProtocolRow array
     */
    get protocols() {
        return [...this.rows.values()];
    }
    /**
     * ProtocolState array
     */
    get protocolsStates() {
        return [...this.states.values()];
    }
    /**
     * Start a protocol by name
     * @param protocolName Protocol name
     */
    startProtocol(protocolName) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/StartProtocol?protocolName=' + protocolName);
        });
    }
    /**
     * Stop a protocol by name
     * @param protocolName
     */
    stopProtocol(protocolName) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/StopProtocol?protocolName=' + protocolName);
        });
    }
    /**
     * Send a download request to protocols
     * @param rows Array of protocol names
     * @param parameters Parameters
     */
    sendDownload(rows, parameters) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get('AppVisionService.svc/SendDownloadToProtocol?varNames=' + rows.join() + '&parameter=' + parameters);
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get('AppVisionService.svc/GetProtocolStates');
            let json = yield resp.json();
            this.states = new Map();
            for (const s of json)
                this.states.set(s.Name, new protocolState_1.ProtocolState(s));
            resp = yield this.http.get('AppVisionService.svc/GetProtocols');
            json = yield resp.json();
            this.rows = new Map();
            for (const r of json) {
                const row = new protocolRow_1.ProtocolRow(r);
                const state = this.states.get(r.Name);
                if (state != null) { // liaison row <-> state
                    state.Row = row;
                    row.State = state;
                }
                this.rows.set(r.Name, row);
            }
        });
    }
    /** @internal */
    updateStatesInternal(states) {
        const updatedStates = [];
        states.forEach(state => {
            const row = this.rows.get(state.Name);
            if (row) {
                row.State = state;
                state.Row = row;
                this.states.set(state.Name, state);
                updatedStates.push(state);
            }
        });
        if (updatedStates.length > 0) {
            this.statesChanged.next(updatedStates);
        }
    }
}
exports.ProtocolManager = ProtocolManager;
