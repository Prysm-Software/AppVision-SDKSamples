import { UserMessageCategory } from "../data/userMessageCategory";
import { UserMessageRow } from "../data/userMessageRow";
import { HttpService } from "../services/httpService";
import { OptionManager } from "./optionManager";
export declare class UserMessageManager {
    private http;
    private categories;
    constructor(http: HttpService, optionManager: OptionManager);
    /**
     * Get user messages categories
     * @returns
     */
    getCategories(): UserMessageCategory[];
    /**
     * Get user messages from the server
     * @param dateStart Date start filter
     * @param dateEnd Date end filter
     * @param filters Filter operand
     * @param maxCount Max count result. Default is 2500
     * @returns Array of UserMessageRow
     */
    getAll(dateStart: Date, dateEnd?: Date, filters?: string, maxCount?: number): Promise<UserMessageRow[]>;
}
