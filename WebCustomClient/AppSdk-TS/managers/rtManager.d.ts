import { HttpService } from "../services/httpService";
export declare class RTManager {
    private http;
    private labels;
    constructor(http: HttpService);
    /**
     * Get RT label value with the current language
     * @param key
     * @returns
     */
    get(key: string): string;
    /**
     * Download file from 'Bin/Resources' directory and load it
     * @param file Filename to load (ex: applab.txt)
     * @param language Two char language key (ex: fr, en ...)
     */
    loadFile(file: string, language: string): Promise<void>;
    private downloadAndLoadFile;
}
