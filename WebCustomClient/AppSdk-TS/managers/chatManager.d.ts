import { Subject } from "rxjs";
import { ChatMessage, ChatMessageType, Talk } from "../data/chat";
import { HttpService } from "../services/httpService";
import { FileManager } from "./fileManager";
interface MessageModel {
    talkId: number;
    content?: string;
    type?: ChatMessageType;
    attachedFile?: {
        blob: Blob;
        fileName: string;
    };
}
interface TalkModel {
    members: number[];
    title?: string;
}
interface UpdateTalkModel extends TalkModel {
    id: number;
}
export declare class ChatManager {
    private http;
    private fileManager;
    newMessages: Subject<ChatMessage[]>;
    talksUpdated: Subject<Talk[]>;
    talksDeleted: Subject<Talk[]>;
    constructor(http: HttpService, fileManager: FileManager);
    /**
     * Send a new message
     * @param msg Message object
     * @returns The created message
     */
    addMessage(msg: MessageModel): Promise<ChatMessage>;
    /**
     * Create a new conversation talk
     * @param talk Talk object
     * @returns Created talk
     */
    addTalk(talk: TalkModel): Promise<Talk>;
    /**
     * Get the messages of a conversation talk
     * @param talkdId Id of the talk
     * @param count Max count of messages returned
     * @param page Pagination system
     * @returns Array of messages
     */
    getMessages(talkdId: number, count?: number, page?: number): Promise<ChatMessage[]>;
    /**
     * Get a talk
     * @param id Id of the talk
     * @returns Talk object
     */
    getTalk(id: number): Promise<Talk>;
    /**
     * Get talks
     * @returns List of talk
     */
    getTalks(): Promise<Talk[]>;
    /**
     * Update a talk
     * @param talk Talk to update
     * @returns Updated talk object
     */
    updateTalk(talk: UpdateTalkModel): Promise<Talk>;
    /**
     * Set the last read message id for a talk
     * @param talkId Talk id
     * @param messageId Message id
     * @returns Updated talk object
     */
    readMessage(talkId: number, messageId: number): Promise<Talk>;
    /**
     * Leave a talk
     * @param talkId Talk Id
     */
    leaveTalk(talkId: number): Promise<void>;
    /**
     * Close a talk (only if owner)
     * @param talkId Talk Id
     */
    closeTalk(talkId: number): Promise<void>;
}
export {};
