import { ContainerModel, DocumentModel } from "./data/environmentModels";
export interface LoadDocumentRequest {
    /**
     * Name of the container (c1, c2...)
     */
    containerName: string;
    /**
    * Document model
    */
    document: DocumentModel;
    /**
     * (optionnal) Windows id (0 for main window ...)
     */
    windowId?: number;
}
export interface OpenWindowRequest {
    title?: string;
    left?: number;
    top?: number;
    width?: number;
    height?: number;
    resizeable?: boolean;
    document: DocumentModel;
}
export interface AppClient {
    /**
     * Load a view
     * @param obj View load parameters
     */
    load(obj: LoadDocumentRequest): void;
    /**
     * Open a new window
     * @param obj Window openning parameters
     */
    open(obj: OpenWindowRequest): void;
    /**
     * Get a specific container by it's name
     * @param container Container name (c1, c2 ...)
     */
    getContainer(container: string): ContainerModel;
    /**
     * Return all containers displayed
     */
    getContainers(): ContainerModel[];
    /**
     * Open the change password dialog
     */
    changePassword(): void;
    /**
     * Disconnect the client (send logout + refresh page)
     */
    disconnect(): Promise<void>;
    /**
     * Play an audio file
     * @param filename audio filepath
     * @param loop looping or not
     */
    playSound(filename: string, loop: boolean): void;
    /**
     * Stop the playing
     */
    stopSound(): void;
}
