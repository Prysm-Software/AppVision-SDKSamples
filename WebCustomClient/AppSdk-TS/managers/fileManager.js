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
exports.FileManager = void 0;
const treeRow_1 = require("../data/treeRow");
const fileInfoRow_1 = require("../data/fileInfoRow");
const rxjs_1 = require("rxjs");
class FileManager {
    constructor(http) {
        this.http = http;
        this.rowsUpdated = new rxjs_1.Subject();
    }
    /**
     * Build file tree
     * @param types FileType filter (FileType)
     * @returns Tree of FileInfoRow
     */
    getFileTree(...types) {
        const copy = (types === null || types === void 0 ? void 0 : types.length) > 0 ?
            this.fileInfos
                .filter(f => f.TypeInt === 5 || types.indexOf(f.TypeInt) > -1) //  5=dossier + types demandés
                .map(a => new fileInfoRow_1.FileInfoRow(a)) :
            this.fileInfos.map(a => new fileInfoRow_1.FileInfoRow(a));
        return treeRow_1.TreeRow.init(copy, '\\');
    }
    /**
     * Get all files (flat)
     * @param types FileType filter (FileType)
     * @returns Array of FileInfoRow
     */
    getAll(...types) {
        return (types === null || types === void 0 ? void 0 : types.length) > 0 ?
            this.fileInfos.filter(f => types.indexOf(f.TypeInt) > -1).map(f => new fileInfoRow_1.FileInfoRow(f)) :
            this.fileInfos.map(f => new fileInfoRow_1.FileInfoRow(f));
    }
    /**
     * Download file from server
     * @param url Url of the file
     * @returns File content as string
     *
     * @example
     * server.fileManager.getFile('Config/Client/MySuperFile.txt')
     */
    getFile(url) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get(url);
            return yield resp.text();
        });
    }
    /**
     * Delete a file on the server
     * @param url Url of the file
     *
     * @example
     * server.fileManager.getFile('Config/Client/MySuperFile.txt')
     */
    deleteFile(url) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.get(url + "?action=DELETE");
        });
    }
    /**
     * Update a file on the server
     * @param filePath Url of the file
     * @param file File content as Blob
     * @param header Http headers of the request
     */
    putFile(filePath, file, header) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post(filePath, file, "PUT", header);
        });
    }
    /**
     * Get file url inside the config folder
     * @param filePath
     * @returns
     */
    getConfigFileUrl(filePath) {
        return this.http.host + "config/" + filePath;
    }
    /**
     * Get file url inside the Resources folder
     * @param filePath
     * @returns
     */
    getResourceFileUrl(filePath) {
        return this.http.host + "resources/" + filePath;
    }
    /**
     * Get file url from Config/Param/Client foler
     * @param filePath
     * @returns
     */
    getFileUrl(filePath) {
        const uri = new URL(filePath, this.http.host + "Config/Param/Client/");
        return uri.toString();
    }
    /**
     * Get file url from Config/Param/AppControl/Photos/ folder
     * @param filePath
     * @returns
     */
    getAppControlPhotoUrl(filePath) {
        const uri = new URL(filePath, this.http.host + "Config/Param/AppControl/Photos/");
        return uri.toString();
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get('AppVisionService.svc/GetFileInfos');
            const json = yield resp.json();
            this.fileInfos = json.filter(i => (i.ContextInt & 2) === 2);
        });
    }
    /** @internal */
    updateRowsInternal(infoRows) {
        // Rows to add or update
        for (const row of infoRows.filter(i => (i.ContextInt & 2) === 2)) {
            const index = this.fileInfos.findIndex(r => r.Id === row.Id);
            if (index >= 0) {
                // modified
                this.fileInfos[index] = row;
            }
            else {
                // new
                this.fileInfos.push(row);
            }
        }
        // The deleted rows have ContextInt to 0
        for (const row of infoRows.filter(i => i.Id < 0)) {
            const index = this.fileInfos.findIndex(r => r.Id === Math.abs(row.Id));
            if (index >= 0) {
                this.fileInfos.splice(index, 1);
            }
        }
        this.rowsUpdated.next(infoRows);
    }
}
exports.FileManager = FileManager;
