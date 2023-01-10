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
exports.EnvironmentManager = void 0;
const fileParser_1 = require("../common/fileParser");
class EnvironmentManager {
    constructor(http, fileManager, optionManager) {
        this.http = http;
        this.fileManager = fileManager;
        this.optionManager = optionManager;
    }
    /**
     * Create a new environment file on the server.
     * {filename}.webenv
     * @param filename Filename
     * @param env Environment object
     */
    createEnvironment(filename, env) {
        var _a, _b;
        return __awaiter(this, void 0, void 0, function* () {
            if (!filename.toLowerCase().endsWith('.webenv'))
                filename = filename + '.webenv';
            const obj = {
                isSideBarVisible: (_a = env.isSideBarVisible) !== null && _a !== void 0 ? _a : true,
                isHeaderVisible: (_b = env.isHeaderVisible) !== null && _b !== void 0 ? _b : true,
                layout: {
                    containers: env.layout.containers,
                    mode: env.layout.mode,
                    properties: {}
                }
            };
            if (env.layout.properties) {
                env.layout.properties.forEach((prop, key) => {
                    obj.layout.properties[key] = prop;
                });
            }
            const json = JSON.stringify(obj);
            //const json = LayoutModel.stringify(layout);
            yield this.fileManager.putFile('Config/Param/Client/' + filename, new Blob([json], { type: "application/json" }));
            if (this.environments.indexOf(filename) < 0) {
                this.environments.push(filename);
            }
        });
    }
    /**
     * Download an environment from the server
     * @param filename Environment filename
     * @returns Environment object
     */
    getEnvironment(filename) {
        return __awaiter(this, void 0, void 0, function* () {
            const file = yield this.fileManager.getFile("Config/Param/Client/" + filename);
            if (filename.endsWith(".webenv")) {
                return fileParser_1.FileParser.parseWebEnvironment(file);
            }
            else if (filename.endsWith(".ctx")) {
                return fileParser_1.FileParser.parseClassicContext(file);
            }
            return null;
        });
    }
    /** @internal */
    loadInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            // Load start up env
            const h1 = this.optionManager.get("WebApp.Main.Homepage");
            if (h1 === null || h1 === void 0 ? void 0 : h1.endsWith('.webenv')) {
                try {
                    const env = yield this.fileManager.getFile("Config/Param/Client/" + h1);
                    this.homeEnv = fileParser_1.FileParser.parseWebEnvironment(env);
                }
                catch (e) {
                    console.error(e);
                }
            }
            // Load mobile start up env
            const h2 = this.optionManager.get("WebApp.Main.Homepage.Mobile");
            if (h2 === null || h2 === void 0 ? void 0 : h2.endsWith('.webenv')) {
                try {
                    const env = yield this.fileManager.getFile("Config/Param/Client/" + h2);
                    this.homeEnvMobile = fileParser_1.FileParser.parseWebEnvironment(env);
                }
                catch (e) {
                    console.error(e);
                }
            }
            // Load envs files list
            try {
                const resp = yield this.http.get("Config/Param/Client/?action=PROPFIND&pattern=*.webenv");
                const json = yield resp.json();
                this.environments = json;
            }
            catch (e) {
                console.error(e);
            }
            // Load contexts files list
            try {
                const resp = yield this.http.get("Config/Param/Client/?action=PROPFIND&pattern=*.ctx");
                const json = yield resp.json();
                this.contexts = json;
            }
            catch (e) {
                console.error(e);
            }
        });
    }
}
exports.EnvironmentManager = EnvironmentManager;
