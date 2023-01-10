import { Subject } from "rxjs";
import { OverlayItem } from "../data/overlayItem";
import { HttpService } from "../services/httpService";
export declare class OverlayManager {
    private http;
    private items;
    itemUpdated: Subject<OverlayItem[]>;
    itemAdded: Subject<OverlayItem[]>;
    itemDeleted: Subject<OverlayItem[]>;
    constructor(http: HttpService);
    /**
     * Get overlay items from cache
     * @returns Overlays
     */
    getAll(): OverlayItem[];
    /**
     * Add or update overlay
     * @param o Overlay
     * @returns Overlay updated/created
     */
    update(o: OverlayItem): Promise<OverlayItem>;
    /**
     * Delete an overlay
     * @param id Overlay id to delete
     */
    delete(id: string): Promise<void>;
}
