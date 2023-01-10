"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserState = void 0;
const chat_1 = require("./chat");
class UserState {
    constructor(obj) {
        var _a;
        this.UnreadTalks = [];
        Object.assign(this, obj);
        if (((_a = obj.UnreadTalks) === null || _a === void 0 ? void 0 : _a.length) > 0) {
            this.UnreadTalks = obj.UnreadTalks.map(t => new chat_1.ChatMessage(t));
        }
    }
}
exports.UserState = UserState;
