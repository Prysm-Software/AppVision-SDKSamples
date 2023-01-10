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
exports.RTManager = void 0;
class RTManager {
    constructor(http) {
        this.labels = new Map(); // libellés dans la langue choisie
        this.http = http;
    }
    /**
     * Get RT label value with the current language
     * @param key
     * @returns
     */
    get(key) {
        return this.labels.get(key) || key;
    }
    /**
     * Download file from 'Bin/Resources' directory and load it
     * @param file Filename to load (ex: applab.txt)
     * @param language Two char language key (ex: fr, en ...)
     */
    loadFile(file, language) {
        return __awaiter(this, void 0, void 0, function* () {
            if (language.length > 2) {
                language = language.substring(0, 2);
            }
            yield this.downloadAndLoadFile(file, language);
        });
    }
    downloadAndLoadFile(filename, lang) {
        return __awaiter(this, void 0, void 0, function* () {
            let lines;
            try {
                const resp = yield this.http.get('Resources/' + filename);
                // UTF16
                const data = yield resp.arrayBuffer(); // .text() tjrs en utf-8
                lines = new TextDecoder('utf-16').decode(data).split('\n');
                // TODO : voir pour passer en UTF8
                // const data = await resp.text(); // .text() tjrs en utf-8
                // lines = data.split('\n');
                // lines = new TextDecoder('utf-16').decode(data).split('\n');
            }
            catch (e) {
                console.error("[RTManager] - Cannot load languages file : '" + filename + "'.", e);
                return;
            }
            let langidx = -1;
            const languages = new Map();
            let currentLang = null;
            let nb = 0;
            for (let i = 0; i < lines.length; i++) {
                const line = lines[i].split('\t');
                if (!line[0].match(/^\w/)) // ligne de commentaires
                    continue;
                // 1ère ligne = id lang
                if (langidx < 0) {
                    const langDesc = lines[++i].split('\t');
                    line.forEach((langId, j) => languages.set(langId, langDesc[j]));
                    langidx = line.indexOf(lang.toLowerCase());
                    if (langidx < 0) // langue demandée n'existe pas
                        langidx = 1;
                    currentLang = line[langidx];
                }
                // ajoute les libéllés
                this.labels.set(line[0], line[langidx] || line[1]); // en anglais si pas de traduction
                nb++;
            }
            console.log(`[RTManager] ${filename} loaded, Language: ${currentLang}, ${nb} labels`);
        });
    }
}
exports.RTManager = RTManager;
