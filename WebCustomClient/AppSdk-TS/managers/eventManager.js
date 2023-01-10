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
exports.EventManager = void 0;
const rxjs_1 = require("rxjs");
const eventRow_1 = require("../data/eventRow");
const modifRow_1 = require("../data/modifRow");
class EventManager {
    constructor(http) {
        this.http = http;
        this.newEvents = new rxjs_1.Subject();
    }
    /**
     * Get histo events from the server
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param filters Operand filter ($A.xxx ...). Pipe separated
     * @param types Types filter (EventType)
     * @param subtypes Sub types filter (EventSubType)
     * @param maxCount Max count of results. Default is 2500
     * @returns Array of EventRow
     *
     * @example
     * Get all events from 2020/10/10 to now with AppControl group and Area1 area :
     * server.eventManager.getHistoEvents(new Date(2020, 10, 10), new Date(), '$G.AppControl|$A.Area1')
     */
    getHistoEvents(dateStart, dateEnd, filters = null, types = [], subtypes = [], maxCount = 2500) {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = `AppVisionService.svc/GetHistoEvents?minId=0&maxCount=${maxCount}&dateStart=${dateStart.toISOString()}&dateEnd=${dateEnd.toISOString()}`;
            if (filters != null && filters !== '')
                uri += '&filters=' + filters;
            if ((types === null || types === void 0 ? void 0 : types.length) > 0)
                uri += '&types=' + types.join(',');
            if ((subtypes === null || subtypes === void 0 ? void 0 : subtypes.length) > 0)
                uri += '&subtypes=' + subtypes.join(',');
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            return json.map(e => new eventRow_1.EventRow(e));
        });
    }
    /**
     * Get modifications history
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param maxCount Max count results. Default is 2500
     * @returns Array of ModifRow
     */
    getHistoModifs(dateStart, dateEnd, maxCount = 2500) {
        return __awaiter(this, void 0, void 0, function* () {
            let uri = `AppVisionService.svc/GetHistoModifs?minId=0&maxCount=${maxCount}&dateStart=${dateStart.toISOString()}&dateEnd=${dateEnd.toISOString()}`;
            const resp = yield this.http.get(uri);
            const json = yield resp.json();
            return json.map(e => new modifRow_1.ModifRow(e));
        });
    }
    /**
     * Update an eventRow.
     * @param event EventRow to update.
     * @returns true if the event has been successfully updated.
     */
    updateEvent(event) {
        return __awaiter(this, void 0, void 0, function* () {
            const response = yield this.http.post('AppVisionService.svc/UpdateEvent', JSON.stringify(event));
            return yield response.json();
        });
    }
    /**
     * Adds an event.
     * @param event The event to add.
     */
    addEvent(event) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post(encodeURI('AppVisionService.svc/AddEventRow'), JSON.stringify(event));
        });
    }
    /** @internal */
    updateEventsInternal(events) {
        if (events) {
            this.newEvents.next(events);
        }
    }
}
exports.EventManager = EventManager;
