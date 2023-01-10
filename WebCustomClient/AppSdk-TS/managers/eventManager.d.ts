import { Subject } from "rxjs";
import { EventRow } from "../data/eventRow";
import { ModifRow } from "../data/modifRow";
import { HttpService } from "../services/httpService";
export declare class EventManager {
    private http;
    newEvents: Subject<EventRow[]>;
    constructor(http: HttpService);
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
    getHistoEvents(dateStart: Date, dateEnd: Date, filters?: string, types?: number[], subtypes?: number[], maxCount?: number): Promise<EventRow[]>;
    /**
     * Get modifications history
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param maxCount Max count results. Default is 2500
     * @returns Array of ModifRow
     */
    getHistoModifs(dateStart: Date, dateEnd: Date, maxCount?: number): Promise<ModifRow[]>;
    /**
     * Update an eventRow.
     * @param event EventRow to update.
     * @returns true if the event has been successfully updated.
     */
    updateEvent(event: EventRow): Promise<boolean>;
    /**
     * Adds an event.
     * @param event The event to add.
     */
    addEvent(event: EventRow): Promise<void>;
}
