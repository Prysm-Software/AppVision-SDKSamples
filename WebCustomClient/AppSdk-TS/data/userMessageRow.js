"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserMessageRow = exports.UserMessageStatus = void 0;
const dateHelper_1 = require("../common/dateHelper");
var UserMessageStatus;
(function (UserMessageStatus) {
    UserMessageStatus[UserMessageStatus["InProgress"] = 1] = "InProgress";
    UserMessageStatus[UserMessageStatus["WaitAck"] = 5] = "WaitAck";
    UserMessageStatus[UserMessageStatus["Completed"] = 10] = "Completed";
})(UserMessageStatus = exports.UserMessageStatus || (exports.UserMessageStatus = {}));
class UserMessageRow {
    constructor(obj) {
        Object.assign(this, obj);
    }
    get dateCreate() {
        return dateHelper_1.DateHelper.toJSDate(this.DateCreate);
    }
}
exports.UserMessageRow = UserMessageRow;
