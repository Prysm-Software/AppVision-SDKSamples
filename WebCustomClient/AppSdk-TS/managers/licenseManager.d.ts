import { HttpService } from "../services/httpService";
export declare class LicenseManager {
    private http;
    private infos;
    constructor(http: HttpService);
    /**
     * Get licence field value
     * @param key Licence field name
     * @returns Value as string
     */
    get(key: string): string;
}
