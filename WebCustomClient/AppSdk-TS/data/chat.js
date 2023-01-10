"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Talk = exports.ChatMessage = void 0;
const dateHelper_1 = require("../common/dateHelper");
class ChatMessage {
    constructor(msg) {
        Object.assign(this, msg);
    }
    get date() {
        return dateHelper_1.DateHelper.toJSDate(this.Date);
    }
}
exports.ChatMessage = ChatMessage;
class Talk {
    constructor(talk) {
        Object.assign(this, talk);
        if (talk.LastMessage) {
            this.LastMessage = new ChatMessage(talk.LastMessage);
        }
    }
    get createdDate() {
        return dateHelper_1.DateHelper.toJSDate(this.CreatedDate);
    }
}
exports.Talk = Talk;
