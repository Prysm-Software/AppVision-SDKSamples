import { FileInfoRow, FileInfoType } from "../data/fileInfoRow";
import { HttpService } from "../services/httpService";
import { Subject } from "rxjs";
export declare class FileManager {
    private http;
    private fileInfos;
    rowsUpdated: Subject<FileInfoRow[]>;
    constructor(http: HttpService);
    /**
     * Build file tree
     * @param types FileType filter (FileType)
     * @returns Tree of FileInfoRow
     */
    getFileTree(...types: FileInfoType[]): FileInfoRow[];
    /**
     * Get all files (flat)
     * @param types FileType filter (FileType)
     * @returns Array of FileInfoRow
     */
    getAll(...types: FileInfoType[]): FileInfoRow[];
    /**
     * Download file from server
     * @param url Url of the file
     * @returns File content as string
     *
     * @example
     * server.fileManager.getFile('Config/Client/MySuperFile.txt')
     */
    getFile(url: string): Promise<string>;
    /**
     * Delete a file on the server
     * @param url Url of the file
     *
     * @example
     * server.fileManager.getFile('Config/Client/MySuperFile.txt')
     */
    deleteFile(url: string): Promise<void>;
    /**
     * Update a file on the server
     * @param filePath Url of the file
     * @param file File content as Blob
     * @param header Http headers of the request
     */
    putFile(filePath: string, file: Blob, header?: HeadersInit): Promise<void>;
    /**
     * Get file url inside the config folder
     * @param filePath
     * @returns
     */
    getConfigFileUrl(filePath: string): string;
    /**
     * Get file url inside the Resources folder
     * @param filePath
     * @returns
     */
    getResourceFileUrl(filePath: string): string;
    /**
     * Get file url from Config/Param/Client foler
     * @param filePath
     * @returns
     */
    getFileUrl(filePath: string): string;
    /**
     * Get file url from Config/Param/AppControl/Photos/ folder
     * @param filePath
     * @returns
     */
    getAppControlPhotoUrl(filePath: string): string;
}
