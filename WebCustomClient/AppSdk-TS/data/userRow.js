"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.UserRow = void 0;
const dateHelper_1 = require("../common/dateHelper");
const userState_1 = require("./userState");
class UserRow {
    constructor(user) {
        Object.assign(this, user);
        if (user.State)
            this.State = new userState_1.UserState(user.State);
        this.options = {};
        if (this.DbOptions)
            this.DbOptions
                .split(/[\r\n]/)
                .filter(a => a)
                .map(l => l.split('='))
                .forEach(i => this.options[i[0]] = i[1]);
    }
    get fullName() {
        return (this.FirstName ? this.FirstName + ' ' : '') + this.LastName;
    }
    set passwordDate(d) {
        this.PasswordDate = "/Date(" + d.getTime() + ")/";
    }
    get passwordDate() {
        return dateHelper_1.DateHelper.strToJSDate(this.PasswordDate);
    }
    checkRight(right, commandException = "") {
        if (this.profile) {
            if (this.profile.checkRight(right))
                return true;
            if (this.profile.checkCommandException(commandException))
                return true;
        }
        return false;
    }
}
exports.UserRow = UserRow;
