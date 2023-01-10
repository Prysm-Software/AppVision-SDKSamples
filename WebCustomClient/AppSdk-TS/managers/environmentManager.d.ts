import { HttpService } from "../services/httpService";
import { FileManager } from "./fileManager";
import { EnvironmentModel } from "../data/environmentModels";
import { OptionManager } from "./optionManager";
export declare class EnvironmentManager {
    private http;
    private fileManager;
    private optionManager;
    /**
     * Startup environment for desktop browser
     */
    homeEnv: EnvironmentModel;
    /**
     * Startup environment for mobile browser
     */
    homeEnvMobile: EnvironmentModel;
    /**
     * List of all web environment filenames
     */
    environments: string[];
    /**
     * List of all context file from appClient.exe
     */
    contexts: string[];
    constructor(http: HttpService, fileManager: FileManager, optionManager: OptionManager);
    /**
     * Create a new environment file on the server.
     * {filename}.webenv
     * @param filename Filename
     * @param env Environment object
     */
    createEnvironment(filename: string, env: EnvironmentModel): Promise<void>;
    /**
     * Download an environment from the server
     * @param filename Environment filename
     * @returns Environment object
     */
    getEnvironment(filename: string): Promise<EnvironmentModel>;
}
