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
exports.ChatManager = void 0;
const rxjs_1 = require("rxjs");
const dateHelper_1 = require("../common/dateHelper");
const chat_1 = require("../data/chat");
class ChatManager {
    constructor(http, fileManager) {
        this.http = http;
        this.fileManager = fileManager;
        this.newMessages = new rxjs_1.Subject();
        this.talksUpdated = new rxjs_1.Subject();
        this.talksDeleted = new rxjs_1.Subject();
    }
    /**
     * Send a new message
     * @param msg Message object
     * @returns The created message
     */
    addMessage(msg) {
        var _a;
        return __awaiter(this, void 0, void 0, function* () {
            const json = {
                TalkId: msg.talkId,
                MessageType: (_a = msg.type) !== null && _a !== void 0 ? _a : "Text",
            };
            if (msg.type == "File" && msg.attachedFile) {
                const today = dateHelper_1.DateHelper.today();
                const path = "Config/Histo/Chat/" + today.getFullYear() + "_" + ("0" + (today.getMonth() + 1)).slice(-2) + "/" + today.getDate() +
                    "/Talk_" + msg.talkId + "/" + msg.attachedFile.fileName;
                yield this.fileManager.putFile(path, msg.attachedFile.blob);
                json.Message = path;
            }
            else {
                json.Message = msg.content;
            }
            const resp = yield this.http.post("AppVisionService.svc/ChatManager/AddMessage", JSON.stringify(json), "POST");
            const jsonResp = yield resp.json();
            return new chat_1.ChatMessage(jsonResp);
        });
    }
    /**
     * Create a new conversation talk
     * @param talk Talk object
     * @returns Created talk
     */
    addTalk(talk) {
        return __awaiter(this, void 0, void 0, function* () {
            const json = {
                Members: talk.members,
                Title: talk.title
            };
            const resp = yield this.http.post("AppVisionService.svc/ChatManager/AddTalk", JSON.stringify(json), "POST");
            return new chat_1.Talk(yield resp.json());
        });
    }
    /**
     * Get the messages of a conversation talk
     * @param talkdId Id of the talk
     * @param count Max count of messages returned
     * @param page Pagination system
     * @returns Array of messages
     */
    getMessages(talkdId, count = 50, page = 0) {
        return __awaiter(this, void 0, void 0, function* () {
            const resp = yield this.http.get("AppVisionService.svc/ChatManager/GetMessages?talkid=" + talkdId + "&count=" + count + "&page=" + page);
            return (yield resp.json()).map(msg => new chat_1.ChatMessage(msg));
        });
    }
    /**
     * Get a talk
     * @param id Id of the talk
     * @returns Talk object
     */
    getTalk(id) {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get("AppVisionService.svc/ChatManager/GetTalk?id=" + id);
            const json = yield resp.json();
            return new chat_1.Talk(json);
        });
    }
    /**
     * Get talks
     * @returns List of talk
     */
    getTalks() {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.get("AppVisionService.svc/ChatManager/GetTalks");
            const json = yield resp.json();
            return json.map(talk => new chat_1.Talk(talk));
        });
    }
    /**
     * Update a talk
     * @param talk Talk to update
     * @returns Updated talk object
     */
    updateTalk(talk) {
        return __awaiter(this, void 0, void 0, function* () {
            const json = {
                Id: talk.id,
                Members: talk.members,
                Title: talk.title
            };
            let resp = yield this.http.post("AppVisionService.svc/ChatManager/UpdateTalk", JSON.stringify(json));
            return new chat_1.Talk(yield resp.json());
        });
    }
    /**
     * Set the last read message id for a talk
     * @param talkId Talk id
     * @param messageId Message id
     * @returns Updated talk object
     */
    readMessage(talkId, messageId) {
        return __awaiter(this, void 0, void 0, function* () {
            let resp = yield this.http.post("AppVisionService.svc/ChatManager/SetMessageRead?talkId=" + talkId + "&messageId=" + messageId, null);
            return new chat_1.Talk(yield resp.json());
        });
    }
    /**
     * Leave a talk
     * @param talkId Talk Id
     */
    leaveTalk(talkId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post("AppVisionService.svc/ChatManager/LeaveTalk?talkId=" + talkId, null);
        });
    }
    /**
     * Close a talk (only if owner)
     * @param talkId Talk Id
     */
    closeTalk(talkId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.http.post("AppVisionService.svc/ChatManager/CloseTalk?talkId=" + talkId, null);
        });
    }
    /** @internal */
    updateMessagesInternal(msgs) {
        this.newMessages.next(msgs);
    }
    /** @internal */
    updateTalksInternal(msgs) {
        const updated = msgs.filter(t => t.Id >= 0);
        const deleted = msgs.filter(t => t.Id < 0);
        if (updated.length > 0)
            this.talksUpdated.next(updated);
        if (deleted.length > 0)
            this.talksDeleted.next(deleted);
    }
}
exports.ChatManager = ChatManager;
