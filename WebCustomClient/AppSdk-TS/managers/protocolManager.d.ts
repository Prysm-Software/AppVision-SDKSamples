import { Subject } from "rxjs";
import { ProtocolRow } from "../data/protocolRow";
import { ProtocolState } from "../data/protocolState";
import { HttpService } from "../services/httpService";
export declare class ProtocolManager {
    private http;
    private rows;
    private states;
    statesChanged: Subject<ProtocolState[]>;
    constructor(http: HttpService);
    /**
     * ProtocolRow array
     */
    get protocols(): ProtocolRow[];
    /**
     * ProtocolState array
     */
    get protocolsStates(): ProtocolState[];
    /**
     * Start a protocol by name
     * @param protocolName Protocol name
     */
    startProtocol(protocolName: string): Promise<void>;
    /**
     * Stop a protocol by name
     * @param protocolName
     */
    stopProtocol(protocolName: string): Promise<void>;
    /**
     * Send a download request to protocols
     * @param rows Array of protocol names
     * @param parameters Parameters
     */
    sendDownload(rows: string[], parameters: string): Promise<void>;
}
